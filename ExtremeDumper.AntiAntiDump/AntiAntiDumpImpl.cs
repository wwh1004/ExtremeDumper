using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NativeSharp;
using Pointer = NativeSharp.Pointer;

namespace ExtremeDumper.AntiAntiDump {
	internal static unsafe class AntiAntiDumpImpl {
		private static FieldInfo _moduleHandleField;
		private static void*[] _testModuleHandles;
		private static void*[] _testMemoryModuleHandles;
		private static Pointer _cor20HeaderAddressPointerTemplate;
		private static Pointer _metadataAddressPointerTemplate;
		private static Pointer _metadataSizePointerTemplate;

		private static FieldInfo ModuleHandleField {
			get {
				if (_moduleHandleField == null)
					switch (Environment.Version.Major) {
					case 2:
						_moduleHandleField = typeof(ModuleHandle).GetField("m_ptr", BindingFlags.NonPublic | BindingFlags.Instance);
						break;
					case 4:
						_moduleHandleField = typeof(object).Module.GetType("System.Reflection.RuntimeModule").GetField("m_pData", BindingFlags.NonPublic | BindingFlags.Instance);
						break;
					default:
						throw new NotSupportedException();
					}
				return _moduleHandleField;
			}
		}

		private static void*[] TestModuleHandles {
			get {
				if (_testModuleHandles == null) {
					IntPtr[] testModuleHandles;

					testModuleHandles = Enumerable.Range(0, 5).Select(t => (IntPtr)GetModuleHandle(GenerateAssembly(false).ManifestModule)).ToArray();
					_testModuleHandles = new void*[testModuleHandles.Length];
					for (int i = 0; i < testModuleHandles.Length; i++)
						_testModuleHandles[i] = (void*)testModuleHandles[i];
				}
				return _testModuleHandles;
			}
		}

		private static void*[] TestMemoryModuleHandles {
			get {
				if (_testMemoryModuleHandles == null) {
					IntPtr[] testModuleHandles;

					testModuleHandles = Enumerable.Range(0, 3).Select(t => (IntPtr)GetModuleHandle(GenerateAssembly(true).ManifestModule)).ToArray();
					_testMemoryModuleHandles = new void*[testModuleHandles.Length];
					for (int i = 0; i < testModuleHandles.Length; i++)
						_testMemoryModuleHandles[i] = (void*)testModuleHandles[i];
				}
				return _testMemoryModuleHandles;
			}
		}

		private static Pointer Cor20HeaderAddressPointerTemplate {
			get {
				if (_cor20HeaderAddressPointerTemplate == null) {
					_cor20HeaderAddressPointerTemplate = GetFirstValidTemplate(ScanCor20HeaderAddressPointerTemplates(), pCorHeader => VerifyCor20HeaderAddressPointer((void*)pCorHeader));
					if (_cor20HeaderAddressPointerTemplate == null)
						throw new InvalidOperationException();
				}
				return _cor20HeaderAddressPointerTemplate;
			}
		}

		private static Pointer MetadataAddressPointerTemplate {
			get {
				if (_metadataAddressPointerTemplate == null) {
					_metadataAddressPointerTemplate = GetFirstValidTemplate(ScanMetadataAddressPointerTemplates(), pMetadata => VerifyMetadataAddressPointer((void*)pMetadata));
					if (_metadataAddressPointerTemplate == null)
						throw new InvalidOperationException();
				}
				return _metadataAddressPointerTemplate;
			}
		}

		private static Pointer MetadataSizePointerTemplate {
			get {
				if (_metadataSizePointerTemplate == null) {
					IList<uint> offsets;

					_metadataSizePointerTemplate = new Pointer(MetadataAddressPointerTemplate);
					offsets = _metadataSizePointerTemplate.Offsets;
					offsets[offsets.Count - 1] += (uint)IntPtr.Size;
				}
				return _metadataSizePointerTemplate;
			}
		}

		/// <summary>
		/// 是否可以使用 AntiAntiDump
		/// </summary>
		/// <param name="module"></param>
		/// <returns></returns>
		public static bool CanAntiAntiDump(Module module) {
			if (module is null)
				throw new ArgumentNullException(nameof(module));

			return !IsNativeImage(module);
		}

		/// <summary>
		/// 获取关键.NET元数据信息
		/// </summary>
		/// <param name="module"></param>
		/// <param name="pCor20Header"></param>
		/// <param name="pMetadata"></param>
		/// <param name="metadataSize"></param>
		public static void LocateMetadata(Module module, out void* pCor20Header, out void* pMetadata, out uint metadataSize) {
			if (module is null)
				throw new ArgumentNullException(nameof(module));

			void* moduleHandle;

			moduleHandle = GetModuleHandle(module);
			pCor20Header = (void*)ReadIntPtr(MakePointer(Cor20HeaderAddressPointerTemplate, moduleHandle));
			pMetadata = (void*)ReadIntPtr(MakePointer(MetadataAddressPointerTemplate, moduleHandle));
			metadataSize = ReadUInt32(MakePointer(MetadataSizePointerTemplate, moduleHandle));
		}

		private static uint ReadUInt32(Pointer pointer) {
			void* address;
			uint value;

			if (!TryToAddress(pointer, out address))
				return default;
			if (!TryReadUInt32(address, out value))
				return default;
			return value;
		}

		private static IntPtr ReadIntPtr(Pointer pointer) {
			void* address;
			IntPtr value;

			if (!TryToAddress(pointer, out address))
				return default;
			if (!TryReadIntPtr(address, out value))
				return default;
			return value;
		}

		private static Assembly GenerateAssembly(bool isInMemory) {
			using (CodeDomProvider provider = CodeDomProvider.CreateProvider("cs")) {
				CompilerParameters options;
				CodeCompileUnit assembly;
				CodeNamespace @namespace;
				CompilerResults results;
				Assembly compiledAssembly;

				options = new CompilerParameters {
					GenerateExecutable = false
				};
				if (isInMemory)
					options.GenerateInMemory = true;
				else
					options.OutputAssembly = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".dll");
				assembly = new CodeCompileUnit();
				@namespace = new CodeNamespace("ns1");
				assembly.Namespaces.Add(@namespace);
				@namespace.Types.Add(new CodeTypeDeclaration("class1"));
				results = provider.CompileAssemblyFromDom(options, assembly);
				compiledAssembly = isInMemory ? results.CompiledAssembly : Assembly.LoadFile(results.PathToAssembly);
				return compiledAssembly;
			}
		}

		private static Pointer GetFirstValidTemplate(List<Pointer> templates, Predicate<IntPtr> verifier) {
			foreach (Pointer template in templates) {
				foreach (void* moduleHandle in TestModuleHandles) {
					void* address;
					IntPtr value;

					if (!TryToAddress(MakePointer(template, moduleHandle), out address) || !TryReadIntPtr(address, out value) || !verifier(value))
						goto next;
				}
				foreach (void* moduleHandle in TestMemoryModuleHandles) {
					void* address;
					IntPtr value;

					if (!TryToAddress(MakePointer(template, moduleHandle), out address) || !TryReadIntPtr(address, out value) || !verifier(value))
						goto next;
				}
				return template;
			next:
				continue;
			}
			return null;
		}

		private static bool IsNativeImage(Module module) {
			try {
				string moduleName;

				moduleName = Path.GetFileName(module.Assembly.Location);
				moduleName = Path.GetFileNameWithoutExtension(moduleName) + ".ni" + Path.GetExtension(moduleName);
				return NativeProcess.CurrentProcess.GetModule(moduleName) != null;
			}
			catch {
				return false;
			}
		}

		private static Pointer MakePointer(Pointer template, void* moduleHandle) {
			Pointer pointer;

			pointer = new Pointer(template);
			pointer.BaseAddress = (byte*)moduleHandle + (uint)pointer.BaseAddress;
			return pointer;
		}

		private static List<Pointer> ScanCor20HeaderAddressPointerTemplates() {
			void* moduleHandle;
			List<Pointer> pointers;
			uint[] m_fileOffsets;
			uint[] m_identityOffsets;
			uint[] unknownOffset1s;
			uint[] m_pCorHeaderOffsets;

			moduleHandle = TestModuleHandles[0];
			pointers = new List<Pointer>();
			m_identityOffsets = IntPtr.Size == 4 ? new uint[] { 0x8 } : new uint[] { 0x10 };
			// PEFile.m_openedILimage
			unknownOffset1s = Enumerable.Range(0, (0x80 - 0x20) / 4).Select(t => 0x20 + ((uint)t * 4)).ToArray();
			// PEImage.????
			m_pCorHeaderOffsets = IntPtr.Size == 4 ? new uint[] { 0x14 } : new uint[] { 0x20 };
			// PEDecoder.m_pCorHeader
			m_fileOffsets = IntPtr.Size == 4 ? new uint[] { 0x4, 0x8 } : new uint[] { 0x8, 0x10 };
			// Module.m_file
			foreach (uint m_fileOffset in m_fileOffsets) {
				IntPtr m_file;

				if (!TryReadIntPtr((byte*)moduleHandle + m_fileOffset, out m_file))
					continue;
				foreach (uint m_identityOffset in m_identityOffsets) {
					IntPtr m_identity;

					if (!TryReadIntPtr((byte*)m_file + m_identityOffset, out m_identity))
						continue;
					foreach (uint unknownOffset1 in unknownOffset1s) {
						IntPtr unknown1;

						if (!TryReadIntPtr((byte*)m_identity + unknownOffset1, out unknown1))
							continue;
						foreach (uint m_pCorHeaderOffset in m_pCorHeaderOffsets) {
							IntPtr pCorHeader;

							if (!TryReadIntPtr((byte*)unknown1 + m_pCorHeaderOffset, out pCorHeader))
								continue;
							if (!VerifyCor20HeaderAddressPointer((void*)pCorHeader))
								continue;
							pointers.Add(new Pointer((void*)m_fileOffset, m_identityOffset, unknownOffset1, m_pCorHeaderOffset));
						}
					}
				}
			}
			return pointers;
		}

		private static bool VerifyCor20HeaderAddressPointer(void* pCorHeader) {
			uint cb;

			if (!TryReadUInt32(pCorHeader, out cb))
				return false;
			return cb == 0x48;
		}

		private static List<Pointer> ScanMetadataAddressPointerTemplates() {
			void* moduleHandle;
			List<Pointer> pointers;
			uint[] m_fileOffsets;
			uint[] unknownOffset1s;
			uint[] unknownOffset2s;

			moduleHandle = TestModuleHandles[0];
			pointers = new List<Pointer>();
			m_fileOffsets = IntPtr.Size == 4 ? new uint[] { 0x4, 0x8 } : new uint[] { 0x8, 0x10 };
			// Module.m_file
			unknownOffset1s = Enumerable.Range(0, (0x3C - 0x10) / 4).Select(t => 0x10 + ((uint)t * 4)).ToArray();
			// PEFile.????
			unknownOffset2s = IntPtr.Size == 4
				? Enumerable.Range(0, (0x39C - 0x350) / 4).Select(t => 0x350 + ((uint)t * 4)).ToArray()
				: Enumerable.Range(0, (0x5FC - 0x5B0) / 4).Select(t => 0x5B0 + ((uint)t * 4)).ToArray();
			// ????.????
			foreach (uint m_fileOffset in m_fileOffsets) {
				IntPtr m_file;

				if (!TryReadIntPtr((byte*)moduleHandle + m_fileOffset, out m_file))
					continue;
				foreach (uint unknownOffset1 in unknownOffset1s) {
					IntPtr unknown1;

					if (!TryReadIntPtr((byte*)m_file + unknownOffset1, out unknown1))
						continue;
					foreach (uint unknownOffset2 in unknownOffset2s) {
						IntPtr pMetadata;

						if (!TryReadIntPtr((byte*)unknown1 + unknownOffset2, out pMetadata))
							continue;
						if (!VerifyMetadataAddressPointer((void*)pMetadata))
							continue;
						pointers.Add(new Pointer((void*)m_fileOffset, unknownOffset1, unknownOffset2));
					}
				}
			}
			return pointers;
		}

		private static bool VerifyMetadataAddressPointer(void* pMetadata) {
			uint signature;

			if (!TryReadUInt32(pMetadata, out signature))
				return false;
			return signature == 0x424A5342;
		}

		private static void* GetModuleHandle(Module module) {
			switch (Environment.Version.Major) {
			case 2:
				return (void*)(IntPtr)ModuleHandleField.GetValue(module.ModuleHandle);
			case 4:
				return (void*)(IntPtr)ModuleHandleField.GetValue(module);
			default:
				throw new NotSupportedException();
			}
		}

		private static bool TryToAddress(Pointer pointer, out void* address) {
			return NativeProcess.CurrentProcess.TryToAddress(pointer, out address);
		}

		private static bool TryReadUInt32(void* address, out uint value) {
			return NativeProcess.CurrentProcess.TryReadUInt32(address, out value);
		}

		private static bool TryReadIntPtr(void* address, out IntPtr value) {
			return NativeProcess.CurrentProcess.TryReadIntPtr(address, out value);
		}
	}
}
