using System;
using System.Reflection;
using System.Runtime.InteropServices;
using static MetadataLocator.NativeMethods;

namespace MetadataLocator {
	/// <summary>
	/// Metadata helper
	/// </summary>
	internal static unsafe class MetadataHelper {
		private static readonly bool _isClr2x;
		private static readonly MethodInfo _getMetadataImport;

		static MetadataHelper() {
			_isClr2x = Environment.Version.Major == 2;
			_getMetadataImport = typeof(ModuleHandle).GetMethod("_GetMetadataImport", _isClr2x ? BindingFlags.NonPublic | BindingFlags.Instance : BindingFlags.NonPublic | BindingFlags.Static);
		}

		/// <summary>
		/// A wrapper for _GetMetadataImport
		/// </summary>
		/// <param name="module"></param>
		/// <returns></returns>
		public static void* GetMetadataImport(Module module) {
			if (module is null)
				throw new ArgumentNullException(nameof(module));

			return _isClr2x
				? Pointer.Unbox(_getMetadataImport.Invoke(module.ModuleHandle, null))
				: (void*)(IntPtr)_getMetadataImport.Invoke(null, new object[] { module });
		}

		/// <summary>
		/// Get the instance of <see cref="IMetaDataTables"/> from IMDInternalImport
		/// </summary>
		/// <param name="pIMDInternalImport">A pointer to the instance of IMDInternalImport</param>
		/// <returns></returns>
		public static IMetaDataTables GetIMetaDataTables(void* pIMDInternalImport) {
			if (pIMDInternalImport is null)
				throw new ArgumentNullException(nameof(pIMDInternalImport));

			int result;
			void* pIMetaDataTables;

			fixed (Guid* riid = &IID_IMetaDataTables)
				result = GetMetaDataPublicInterfaceFromInternal(pIMDInternalImport, riid, &pIMetaDataTables);
			return result == 0 ? GetManagedInterface<IMetaDataTables>(pIMetaDataTables) : null;
		}

		/// <summary>
		/// Get the instance of <see cref="IMetaDataTables2"/> from IMDInternalImport
		/// </summary>
		/// <param name="pIMDInternalImport">A pointer to the instance of IMDInternalImport</param>
		/// <returns></returns>
		public static IMetaDataTables2 GetIMetaDataTables2(void* pIMDInternalImport) {
			if (pIMDInternalImport is null)
				throw new ArgumentNullException(nameof(pIMDInternalImport));

			int result;
			void* pIMetaDataTables2;

			fixed (Guid* riid = &IID_IMetaDataTables2)
				result = GetMetaDataPublicInterfaceFromInternal(pIMDInternalImport, riid, &pIMetaDataTables2);
			return result == 0 ? GetManagedInterface<IMetaDataTables2>(pIMetaDataTables2) : null;
		}

		private static int GetMetaDataPublicInterfaceFromInternal(void* pv, Guid* riid, void** ppv) {
			return _isClr2x ? GetMetaDataPublicInterfaceFromInternal2(pv, riid, ppv) : GetMetaDataPublicInterfaceFromInternal4(pv, riid, ppv);
		}

		private static T GetManagedInterface<T>(void* pIUnknown) where T : class {
			if (pIUnknown is null)
				throw new ArgumentNullException(nameof(pIUnknown));

			return (T)Marshal.GetObjectForIUnknown((IntPtr)pIUnknown);
		}
	}
}
