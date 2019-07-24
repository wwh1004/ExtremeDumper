using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using InternalMetadataInfo = MetadataLocator.MetadataInfo;

namespace ExtremeDumper.AntiAntiDump {
	/// <summary>
	/// Provide AntiAntiDump service
	/// </summary>
	public sealed unsafe class AntiAntiDumpService : MarshalByRefObject {
		private static readonly Dictionary<IntPtr, Module> _cachedModules = new Dictionary<IntPtr, Module>();

		/// <summary>
		/// Get AntiAntiDump info
		/// </summary>
		/// <param name="moduleHandle"></param>
		/// <returns></returns>
		public AntiAntiDumpInfo GetAntiAntiDumpInfo(IntPtr moduleHandle) {
			Module module;
			bool canAntiAntiDump;
			AntiAntiDumpInfo info;

			module = GetModuleFromNativeModuleHandle(moduleHandle);
			canAntiAntiDump = CanAntiAntiDump(module);
			if (canAntiAntiDump) {
				void* pCor20Header;
				void* pMetadata;
				uint metadataSize;
				MetadataInfo metadataInfo;

				LocateMetadata(module, out pCor20Header, out pMetadata, out metadataSize);
				metadataInfo = GetMetadataInfo(module);
				info = new AntiAntiDumpInfo(true, GetImageLayout(module), (uint)((byte*)pCor20Header - (byte*)moduleHandle), (uint)((byte*)pMetadata - (byte*)moduleHandle), metadataSize, metadataInfo);
			}
			else
				info = new AntiAntiDumpInfo(false, default, default, default, default, default);
			return info;
		}

		private static Module GetModuleFromNativeModuleHandle(IntPtr moduleHandle) {
			Module module;

			if (!_cachedModules.TryGetValue(moduleHandle, out module)) {
				module = EnumerateLoadedModules().FirstOrDefault(t => Marshal.GetHINSTANCE(t) == moduleHandle);
				if (!(module is null))
					// 如果获取成功，缓存结果，EnumerateLoadedModules()是很耗时的操作
					_cachedModules.Add(moduleHandle, module);
			}
			if (module is null)
				throw new InvalidOperationException($"Can't get System.Reflection.Module from a native module handle (0x{moduleHandle.ToString(IntPtr.Size == 4 ? "X8" : "X16")}).");
			return module;
		}

		private static IEnumerable<Module> EnumerateLoadedModules() {
			return AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetLoadedModules());
		}

		private static bool CanAntiAntiDump(Module module) {
			return AntiAntiDumpImpl.CanAntiAntiDump(module);
		}

		private static ImageLayout GetImageLayout(Module module) {
			string name;

			name = module.FullyQualifiedName;
			if (name.Length > 0 && name[0] == '<' && name[name.Length - 1] == '>')
				return ImageLayout.File;
			return ImageLayout.Memory;
		}

		private static void LocateMetadata(Module module, out void* pCor20Header, out void* pMetadata, out uint metadataSize) {
			AntiAntiDumpImpl.LocateMetadata(module, out pCor20Header, out pMetadata, out metadataSize);
		}

		private static MetadataInfo GetMetadataInfo(Module module) {
			return new MetadataInfo(new InternalMetadataInfo(module));
		}
	}
}
