// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Diagnostics.Runtime.Utilities;

namespace Microsoft.Diagnostics.Runtime.DacInterface {
	internal unsafe class DacDataTargetWrapper : COMCallableIUnknown {
		public const ulong MagicCallbackConstant = 0x43;

		private static readonly Guid IID_IDacDataTarget = new("3E11CCEE-D08B-43e5-AF01-32717A64DA03");
		private static readonly Guid IID_IMetadataLocator = new("aa8fa804-bc05-4642-b2c5-c353ed22fc63");

		private readonly DataTarget _dataTarget;
		private readonly IDataReader _dataReader;
		private volatile ModuleInfo[]? _modules;

		private Action? _callback;
		private volatile int _callbackContext;

		public IntPtr IDacDataTarget { get; }

		public DacDataTargetWrapper(DataTarget dataTarget) {
			_dataTarget = dataTarget;
			_dataReader = _dataTarget.DataReader;

			var builder = AddInterface(IID_IDacDataTarget, false);
			builder.AddMethod(new GetMachineTypeDelegate(GetMachineType));
			builder.AddMethod(new GetPointerSizeDelegate(GetPointerSize));
			builder.AddMethod(new GetImageBaseDelegate(GetImageBase));
			builder.AddMethod(new ReadVirtualDelegate(ReadVirtual));
			builder.AddMethod(new WriteVirtualDelegate(WriteVirtual));
			builder.AddMethod(new GetTLSValueDelegate(GetTLSValue));
			builder.AddMethod(new SetTLSValueDelegate(SetTLSValue));
			builder.AddMethod(new GetCurrentThreadIDDelegate(GetCurrentThreadID));
			builder.AddMethod(new GetThreadContextDelegate(GetThreadContext));
			builder.AddMethod(new RequestDelegate(Request));
			IDacDataTarget = builder.Complete();

			builder = AddInterface(IID_IMetadataLocator, false);
			builder.AddMethod(new GetMetadataDelegate(GetMetadata));
			builder.Complete();
		}

		public void EnterMagicCallbackContext() {
			Interlocked.Increment(ref _callbackContext);
		}

		public void ExitMagicCallbackContext() {
			Interlocked.Decrement(ref _callbackContext);
		}

		public void SetMagicCallback(Action flushCallback) {
			_callback = flushCallback;
		}

		public HResult GetMachineType(IntPtr self, out IMAGE_FILE_MACHINE machineType) {
			machineType = _dataReader.Architecture switch {
				Architecture.Amd64 => IMAGE_FILE_MACHINE.AMD64,
				Architecture.X86 => IMAGE_FILE_MACHINE.I386,
				Architecture.Arm => IMAGE_FILE_MACHINE.THUMB2,
				Architecture.Arm64 => IMAGE_FILE_MACHINE.ARM64,
				_ => IMAGE_FILE_MACHINE.UNKNOWN,
			};

			return HResult.S_OK;
		}

		public void Flush() {
			_modules = null;
		}

		private ModuleInfo[] GetModules() {
			var modules = _modules;
			if (modules is null) {
				modules = _dataTarget.EnumerateModules().ToArray();
				Array.Sort(modules, (left, right) => left.ImageBase.CompareTo(right.ImageBase));

				_modules = modules;
			}

			return modules;
		}

		public HResult GetPointerSize(IntPtr self, out int pointerSize) {
			pointerSize = _dataReader.PointerSize;
			return HResult.S_OK;
		}

		public HResult GetImageBase(IntPtr self, string imagePath, out ulong baseAddress) {
			imagePath = Path.GetFileNameWithoutExtension(imagePath);

			foreach (var module in GetModules()) {
				string? moduleName = Path.GetFileNameWithoutExtension(module.FileName);
				if (imagePath.Equals(moduleName, StringComparison.CurrentCultureIgnoreCase)) {
					baseAddress = module.ImageBase;
					return HResult.S_OK;
				}
			}

			baseAddress = 0;
			return HResult.E_FAIL;
		}

		public HResult ReadVirtual(IntPtr _, ClrDataAddress cda, IntPtr buffer, int bytesRequested, out int bytesRead) {
			ulong address = cda;

			if (address == MagicCallbackConstant && _callbackContext > 0) {
				// See comment in RuntimeBuilder.FlushDac
				_callback?.Invoke();
				bytesRead = 0;
				return HResult.E_FAIL;
			}

			int read = _dataReader.Read(address, ref Unsafe.AsRef<byte>(buffer.ToPointer()), (uint)bytesRequested);
			if (read > 0) {
				bytesRead = read;
				return HResult.S_OK;
			}

			bytesRead = 0;
			return HResult.E_FAIL;
		}

		public HResult WriteVirtual(IntPtr self, ClrDataAddress address, IntPtr buffer, uint bytesRequested, out uint bytesWritten) {
			// This gets used by MemoryBarrier() calls in the dac, which really shouldn't matter what we do here.
			bytesWritten = bytesRequested;
			return HResult.S_OK;
		}

		public HResult GetTLSValue(IntPtr self, uint threadID, uint index, out ulong value) {
			value = 0;
			return HResult.E_FAIL;
		}

		public HResult SetTLSValue(IntPtr self, uint threadID, uint index, ClrDataAddress value) {
			return HResult.E_FAIL;
		}

		public HResult GetCurrentThreadID(IntPtr self, out uint threadID) {
			threadID = 0;
			return HResult.E_FAIL;
		}

		public HResult GetThreadContext(IntPtr self, uint threadID, uint contextFlags, int contextSize, IntPtr context) {
			if (_dataReader.GetThreadContext(threadID, contextFlags, ref Unsafe.AsRef<byte>(context.ToPointer()), (uint)contextSize))
				return HResult.S_OK;

			return HResult.E_FAIL;
		}

		public HResult Request(IntPtr self, uint reqCode, uint inBufferSize, IntPtr inBuffer, IntPtr outBufferSize, out IntPtr outBuffer) {
			outBuffer = IntPtr.Zero;
			return HResult.E_NOTIMPL;
		}

		public unsafe HResult GetMetadata(
			IntPtr self,
			string fileName,
			int imageTimestamp,
			int imageSize,
			IntPtr mvid,
			uint mdRva,
			uint flags,
			uint bufferSize,
			IntPtr buffer,
			int* pDataSize) {
			return HResult.E_FAIL;
		}

		private delegate HResult GetMetadataDelegate(IntPtr self, [In][MarshalAs(UnmanagedType.LPWStr)] string fileName, int imageTimestamp, int imageSize,
													 IntPtr mvid, uint mdRva, uint flags, uint bufferSize, IntPtr buffer, int* dataSize);
		private delegate HResult GetMachineTypeDelegate(IntPtr self, out IMAGE_FILE_MACHINE machineType);
		private delegate HResult GetPointerSizeDelegate(IntPtr self, out int pointerSize);
		private delegate HResult GetImageBaseDelegate(IntPtr self, [In][MarshalAs(UnmanagedType.LPWStr)] string imagePath, out ulong baseAddress);
		private delegate HResult ReadVirtualDelegate(IntPtr self, ClrDataAddress address, IntPtr buffer, int bytesRequested, out int bytesRead);
		private delegate HResult WriteVirtualDelegate(IntPtr self, ClrDataAddress address, IntPtr buffer, uint bytesRequested, out uint bytesWritten);
		private delegate HResult GetTLSValueDelegate(IntPtr self, uint threadID, uint index, out ulong value);
		private delegate HResult SetTLSValueDelegate(IntPtr self, uint threadID, uint index, ClrDataAddress value);
		private delegate HResult GetCurrentThreadIDDelegate(IntPtr self, out uint threadID);
		private delegate HResult GetThreadContextDelegate(IntPtr self, uint threadID, uint contextFlags, int contextSize, IntPtr context);
		private delegate HResult SetThreadContextDelegate(IntPtr self, uint threadID, uint contextSize, IntPtr context);
		private delegate HResult RequestDelegate(IntPtr self, uint reqCode, uint inBufferSize, IntPtr inBuffer, IntPtr outBufferSize, out IntPtr outBuffer);
	}
}
