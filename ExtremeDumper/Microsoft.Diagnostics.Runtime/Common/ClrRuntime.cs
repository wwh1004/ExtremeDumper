// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Microsoft.Diagnostics.Runtime {
	/// <summary>
	/// Represents a single runtime in a target process or crash dump.  This serves as the primary
	/// entry point for getting diagnostic information.
	/// </summary>
	public abstract class ClrRuntime : IDisposable {
		/// <summary>
		/// Used for internal purposes.
		/// </summary>
		internal abstract DacLibrary DacLibrary { get; }

		/// <summary>
		/// Gets the <see cref="ClrInfo"/> of the current runtime.
		/// </summary>
		public abstract ClrInfo ClrInfo { get; }

		/// <summary>
		/// Gets the <see cref="DataTarget"/> associated with this runtime.
		/// </summary>
		public abstract DataTarget? DataTarget { get; }

		/// <summary>
		/// Returns whether you are allowed to call into the transitive closure of ClrMD objects created from
		/// this runtime on multiple threads.
		/// </summary>
		public abstract bool IsThreadSafe { get; }

		/// <summary>
		/// Gets the list of appdomains in the process.
		/// </summary>
		public abstract ClrAppDomain[] AppDomains { get; }

		/// <summary>
		/// Gets the System AppDomain for Desktop CLR (<see langword="null"/> on .NET Core).
		/// </summary>
		public abstract ClrAppDomain? SystemDomain { get; }

		/// <summary>
		/// Gets the Shared AppDomain for Desktop CLR (<see langword="null"/> on .NET Core).
		/// </summary>
		public abstract ClrAppDomain? SharedDomain { get; }

		/// <summary>
		/// Enumerate all managed modules in the runtime.
		/// </summary>
		public abstract IEnumerable<ClrModule> EnumerateModules();

		/// <summary>
		/// Flushes the DAC cache.  This function MUST be called any time you expect to call the same function
		/// but expect different results.  For example, after walking the heap, you need to call Flush before
		/// attempting to walk the heap again.  After calling this function, you must discard ALL ClrMD objects
		/// you have cached other than DataTarget and ClrRuntime and re-request the objects and data you need.
		/// (e.g. if you want to use the ClrHeap object after calling flush, you must call ClrRuntime.GetHeap
		/// again after Flush to get a new instance.)
		/// </summary>
		public abstract void FlushCachedData();

		/// <summary>
		/// Cleans up all resources and releases them.  You may not use this ClrRuntime or any object it transitively
		/// created after calling this method.
		/// </summary>
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Called when disposing ClrRuntime.
		/// </summary>
		/// <param name="disposing">Whether Dispose() was called or not.</param>
		protected abstract void Dispose(bool disposing);
	}
}
