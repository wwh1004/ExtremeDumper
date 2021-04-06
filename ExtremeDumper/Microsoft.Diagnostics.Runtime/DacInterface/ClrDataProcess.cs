// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Diagnostics.Runtime.Utilities;

namespace Microsoft.Diagnostics.Runtime.DacInterface {
	/// <summary>
	/// This is an undocumented, untested, and unsupported interface.  Do not use.
	/// </summary>
	internal sealed unsafe class ClrDataProcess : CallableCOMWrapper {
		private static readonly Guid IID_IXCLRDataProcess = new("5c552ab6-fc09-4cb3-8e36-22fa03c798b7");
		private readonly DacLibrary _library;

		public ClrDataProcess(DacLibrary library, IntPtr pUnknown)
			: base(library?.OwningLibrary, IID_IXCLRDataProcess, pUnknown) {
			if (library is null)
				throw new ArgumentNullException(nameof(library));

			_library = library;
		}

		private ref readonly IXCLRDataProcessVTable VTable => ref Unsafe.AsRef<IXCLRDataProcessVTable>(_vtable);

		public ClrDataProcess(DacLibrary library, CallableCOMWrapper toClone) : base(toClone) {
			_library = library;
		}

		public SOSDac? GetSOSDacInterface() {
			var result = QueryInterface(SOSDac.IID_ISOSDac);
			if (result == IntPtr.Zero)
				return null;

			try {
				return new SOSDac(_library, result);
			}
			catch (InvalidOperationException) {
				return null;
			}
		}

		public void Flush() {
			VTable.Flush(Self);
		}

		public HResult Request(uint reqCode, byte[] input, byte[] output) {
			fixed (byte* pInput = input)
			fixed (byte* pOutput = output)
				return VTable.Request(Self, reqCode, input.Length, pInput, output.Length, pOutput);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal readonly unsafe struct IXCLRDataProcessVTable {
		public readonly delegate* unmanaged[Stdcall]<IntPtr, HResult> Flush;
		private readonly IntPtr Unused_StartEnumTasks;
		private readonly IntPtr EnumTask;
		private readonly IntPtr EndEnumTasks;
		private readonly IntPtr GetTaskByOSThreadID;
		private readonly IntPtr GetTaskByUniqueID;
		private readonly IntPtr GetFlags;
		private readonly IntPtr IsSameObject;
		private readonly IntPtr GetManagedObject;
		private readonly IntPtr GetDesiredExecutionState;
		private readonly IntPtr SetDesiredExecutionState;
		private readonly IntPtr GetAddressType;
		private readonly IntPtr GetRuntimeNameByAddress;
		private readonly IntPtr StartEnumAppDomains;
		private readonly IntPtr EnumAppDomain;
		private readonly IntPtr EndEnumAppDomains;
		private readonly IntPtr GetAppDomainByUniqueID;
		private readonly IntPtr StartEnumAssemblie;
		private readonly IntPtr EnumAssembly;
		private readonly IntPtr EndEnumAssemblies;
		private readonly IntPtr StartEnumModules;
		private readonly IntPtr EnumModule;
		private readonly IntPtr EndEnumModules;
		private readonly IntPtr GetModuleByAddress;
		private readonly IntPtr StartEnumMethodInstancesByAddress;
		private readonly IntPtr EnumMethodInstanceByAddress;
		private readonly IntPtr EndEnumMethodInstancesByAddress;
		private readonly IntPtr GetDataByAddress;
		private readonly IntPtr GetExceptionStateByExceptionRecord;
		private readonly IntPtr TranslateExceptionRecordToNotification;

		// (uint reqCode, uint inBufferSize, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] inBuffer, uint outBufferSize, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] outBuffer);
		public readonly delegate* unmanaged[Stdcall]<IntPtr, uint, int, byte*, int, byte*, HResult> Request;
	}
}
