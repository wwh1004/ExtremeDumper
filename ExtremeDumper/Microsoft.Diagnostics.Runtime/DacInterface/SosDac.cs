// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Diagnostics.Runtime.Utilities;

namespace Microsoft.Diagnostics.Runtime.DacInterface {
	/// <summary>
	/// This is an undocumented, untested, and unsupported interface.  Do not use.
	/// </summary>
	internal sealed unsafe class SOSDac : CallableCOMWrapper {
		internal static readonly Guid IID_ISOSDac = new("436f00f2-b42a-4b9f-870c-e73db66ae930");

		private readonly DacLibrary _library;

		public SOSDac(DacLibrary? library, IntPtr ptr)
			: base(library?.OwningLibrary, IID_ISOSDac, ptr) {
			_library = library ?? throw new ArgumentNullException(nameof(library));
		}

		private ref readonly ISOSDacVTable VTable => ref Unsafe.AsRef<ISOSDacVTable>(_vtable);

		public SOSDac(DacLibrary lib, CallableCOMWrapper toClone) : base(toClone) {
			_library = lib;
		}

		public HResult GetWorkRequestData(ulong request, out WorkRequestData data) {
			return VTable.GetWorkRequestData(Self, request, out data);
		}

		public ClrDataModule? GetClrDataModule(ulong module) {
			if (module == 0)
				return null;
			if (VTable.GetModule(Self, module, out var iunk))
				return new ClrDataModule(_library, iunk);

			return null;
		}

		public ClrDataAddress[] GetAssemblyList(ulong appDomain) {
			return GetAssemblyList(appDomain, 0);
		}

		public ClrDataAddress[] GetAssemblyList(ulong appDomain, int count) {
			return GetModuleOrAssembly(appDomain, count, VTable.GetAssemblyList);
		}

		public ClrDataAddress[] GetModuleList(ulong assembly) {
			return GetModuleList(assembly, 0);
		}

		public ClrDataAddress[] GetModuleList(ulong assembly, int count) {
			return GetModuleOrAssembly(assembly, count, VTable.GetAssemblyModuleList);
		}

		public HResult GetAssemblyData(ulong domain, ulong assembly, out AssemblyData data) {
			// The dac seems to have an issue where the assembly data can be filled in for a minidump.
			// If the data is partially filled in, we'll use it.

			var hr = VTable.GetAssemblyData(Self, domain, assembly, out data);
			if (!hr && data.Address == assembly)
				return HResult.S_FALSE;

			return hr;
		}

		public HResult GetAppDomainData(ulong addr, out AppDomainData data) {
			// We can face an exception while walking domain data if we catch the process
			// at a bad state.  As a workaround we will return partial data if data.Address
			// and data.StubHeap are set.

			var hr = VTable.GetAppDomainData(Self, addr, out data);
			if (!hr && data.Address == addr && data.StubHeap != 0)
				return HResult.S_FALSE;

			return hr;
		}

		public string? GetAppDomainName(ulong appDomain) {
			return GetString(VTable.GetAppDomainName, appDomain);
		}

		public string? GetAssemblyName(ulong assembly) {
			return GetString(VTable.GetAssemblyName, assembly);
		}

		public HResult GetAppDomainStoreData(out AppDomainStoreData data) {
			return VTable.GetAppDomainStoreData(Self, out data);
		}

		public string? GetPEFileName(ulong pefile) {
			return GetString(VTable.GetPEFileName, pefile);
		}

		private string? GetString(delegate* unmanaged[Stdcall]<IntPtr, ClrDataAddress, int, byte*, out int, HResult> func, ulong addr, bool skipNull = true) {
			var hr = func(Self, addr, 0, null, out int needed);
			if (!hr)
				return null;

			if (needed == 0)
				return string.Empty;

			int size = needed * sizeof(char);
			byte[] buffer = new byte[size];

			fixed (byte* bufferPtr = buffer)
				hr = func(Self, addr, needed, bufferPtr, out needed);

			if (!hr)
				return null;

			if (skipNull)
				needed--;

			fixed (byte* ptr = buffer)
				return Encoding.Unicode.GetString(ptr, needed * sizeof(char));
		}

		private string? GetAsciiString(delegate* unmanaged[Stdcall]<IntPtr, ClrDataAddress, int, byte*, out int, HResult> func, ulong addr) {
			var hr = func(Self, addr, 0, null, out int needed);
			if (!hr)
				return null;

			if (needed == 0)
				return string.Empty;

			byte[] buffer = new byte[needed];

			fixed (byte* bufferPtr = buffer)
				hr = func(Self, addr, needed, bufferPtr, out needed);

			if (!hr)
				return null;

			int len = Array.IndexOf(buffer, (byte)'\0');
			if (len >= 0)
				needed = len;

			fixed (byte* ptr = buffer)
				return Encoding.ASCII.GetString(ptr, needed);
		}

		public HResult GetModuleData(ulong module, out ModuleData data) {
			return VTable.GetModuleData(Self, module, out data);
		}

		private ClrDataAddress[] GetModuleOrAssembly(ulong address, int count, delegate* unmanaged[Stdcall]<IntPtr, ClrDataAddress, int, ClrDataAddress*, out int, HResult> func) {
			int needed;
			if (count <= 0) {
				if (func(Self, address, 0, null, out needed) < 0)
					return Array.Empty<ClrDataAddress>();

				count = needed;
			}

			// We ignore the return value here since the list may be partially filled
			var modules = new ClrDataAddress[count];
			fixed (ClrDataAddress* ptr = modules)
				func(Self, address, modules.Length, ptr, out needed);

			return modules;
		}

		public ClrDataAddress[] GetAppDomainList(int count = 0) {
			if (count <= 0) {
				if (!GetAppDomainStoreData(out var addata))
					return Array.Empty<ClrDataAddress>();

				count = addata.AppDomainCount;
			}

			var data = new ClrDataAddress[count];
			fixed (ClrDataAddress* ptr = data) {
				var hr = VTable.GetAppDomainList(Self, data.Length, ptr, out int needed);
				return hr ? data : Array.Empty<ClrDataAddress>();
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal readonly unsafe struct ISOSDacVTable {
		// ThreadStore
		public readonly IntPtr GetThreadStoreData;

		// AppDomains
		public readonly delegate* unmanaged[Stdcall]<IntPtr, out AppDomainStoreData, HResult> GetAppDomainStoreData;
		public readonly delegate* unmanaged[Stdcall]<IntPtr, int, ClrDataAddress*, out int, HResult> GetAppDomainList;
		public readonly delegate* unmanaged[Stdcall]<IntPtr, ClrDataAddress, out AppDomainData, HResult> GetAppDomainData;
		public readonly delegate* unmanaged[Stdcall]<IntPtr, ClrDataAddress, int, byte*, out int, HResult> GetAppDomainName;
		public readonly IntPtr GetDomainFromContext;

		// Assemblies
		public readonly delegate* unmanaged[Stdcall]<IntPtr, ClrDataAddress, int, ClrDataAddress*, out int, HResult> GetAssemblyList;
		public readonly delegate* unmanaged[Stdcall]<IntPtr, ClrDataAddress, ClrDataAddress, out AssemblyData, HResult> GetAssemblyData;
		public readonly delegate* unmanaged[Stdcall]<IntPtr, ClrDataAddress, int, byte*, out int, HResult> GetAssemblyName;

		// Modules
		public readonly delegate* unmanaged[Stdcall]<IntPtr, ClrDataAddress, out IntPtr, HResult> GetModule;
		public readonly delegate* unmanaged[Stdcall]<IntPtr, ClrDataAddress, out ModuleData, HResult> GetModuleData;
		public readonly IntPtr TraverseModuleMap;
		public readonly delegate* unmanaged[Stdcall]<IntPtr, ClrDataAddress, int, ClrDataAddress*, out int, HResult> GetAssemblyModuleList;
		public readonly delegate* unmanaged[Stdcall]<IntPtr, ClrDataAddress, uint, out ClrDataAddress, HResult> GetILForModule;

		// Threads
		public readonly IntPtr GetThreadData;
		public readonly IntPtr GetThreadFromThinlockID;
		public readonly IntPtr GetStackLimits;

		// MethodDescs
		public readonly IntPtr GetMethodDescData;
		public readonly IntPtr GetMethodDescPtrFromIP;
		public readonly IntPtr GetMethodDescName;
		public readonly IntPtr GetMethodDescPtrFromFrame;
		public readonly IntPtr GetMethodDescFromToken;
		private readonly IntPtr GetMethodDescTransparencyData;

		// JIT Data
		public readonly IntPtr GetCodeHeaderData;
		public readonly IntPtr GetJitManagerList;
		public readonly IntPtr GetJitHelperFunctionName;
		private readonly IntPtr GetJumpThunkTarget;

		// ThreadPool
		public readonly IntPtr GetThreadpoolData;
		public readonly delegate* unmanaged[Stdcall]<IntPtr, ClrDataAddress, out WorkRequestData, HResult> GetWorkRequestData;
		public readonly IntPtr GetHillClimbingLogEntry;

		// Objects
		public readonly IntPtr GetObjectData;
		public readonly IntPtr GetObjectStringData;
		public readonly IntPtr GetObjectClassName;

		// MethodTable
		public readonly IntPtr GetMethodTableName;
		public readonly IntPtr GetMethodTableData;
		public readonly IntPtr GetMethodTableSlot;
		public readonly IntPtr GetMethodTableFieldData;
		public readonly IntPtr GetMethodTableTransparencyData;

		// EEClass
		public readonly IntPtr GetMethodTableForEEClass;

		// FieldDesc
		public readonly IntPtr GetFieldDescData;

		// Frames
		public readonly IntPtr GetFrameName;

		// PEFiles
		public readonly IntPtr GetPEFileBase;
		public readonly delegate* unmanaged[Stdcall]<IntPtr, ClrDataAddress, int, byte*, out int, HResult> GetPEFileName;

		// GC
		public readonly IntPtr GetGCHeapData;
		public readonly IntPtr GetGCHeapList; // svr only
		public readonly IntPtr GetGCHeapDetails; // wks only
		public readonly IntPtr GetGCHeapStaticData;
		public readonly IntPtr GetHeapSegmentData;
		public readonly IntPtr GetOOMData;
		public readonly IntPtr GetOOMStaticData;
		public readonly IntPtr GetHeapAnalyzeData;
		public readonly IntPtr GetHeapAnalyzeStaticData;

		// DomainLocal
		public readonly IntPtr GetDomainLocalModuleData;
		public readonly IntPtr GetDomainLocalModuleDataFromAppDomain;
		public readonly IntPtr GetDomainLocalModuleDataFromModule;

		// ThreadLocal
		public readonly IntPtr GetThreadLocalModuleData;

		// SyncBlock
		public readonly IntPtr GetSyncBlockData;
		public readonly IntPtr GetSyncBlockCleanupData;

		// Handles
		public readonly IntPtr GetHandleEnum;
		public readonly IntPtr GetHandleEnumForTypes;
		public readonly IntPtr GetHandleEnumForGC;

		// EH
		public readonly IntPtr TraverseEHInfo;
		public readonly IntPtr GetNestedExceptionData;

		// StressLog
		public readonly IntPtr GetStressLogAddress;

		// Heaps
		public readonly IntPtr TraverseLoaderHeap;
		public readonly IntPtr GetCodeHeapList;
		public readonly IntPtr TraverseVirtCallStubHeap;

		// Other
		public readonly IntPtr GetUsefulGlobals;
		public readonly IntPtr GetClrWatsonBuckets;
		public readonly IntPtr GetTLSIndex;
		public readonly IntPtr GetDacModuleHandle;

		// COM
		public readonly IntPtr GetRCWData;
		public readonly IntPtr GetRCWInterfaces;
		public readonly IntPtr GetCCWData;
		public readonly IntPtr GetCCWInterfaces;
		public readonly IntPtr TraverseRCWCleanupList;

		// GC Reference Functions
		public readonly IntPtr GetStackReferences;
		public readonly IntPtr GetRegisterName;
		public readonly IntPtr GetThreadAllocData;
		public readonly IntPtr GetHeapAllocData;

		// For BindingDisplay plugin

		public readonly IntPtr GetFailedAssemblyList;
		public readonly IntPtr GetPrivateBinPaths;
		public readonly IntPtr GetAssemblyLocation;
		public readonly IntPtr GetAppDomainConfigFile;
		public readonly IntPtr GetApplicationBase;
		public readonly IntPtr GetFailedAssemblyData;
		public readonly IntPtr GetFailedAssemblyLocation;
		public readonly IntPtr GetFailedAssemblyDisplayName;
	}
}
