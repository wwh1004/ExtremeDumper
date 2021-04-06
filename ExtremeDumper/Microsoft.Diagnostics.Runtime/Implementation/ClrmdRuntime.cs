// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Microsoft.Diagnostics.Runtime.Implementation {
	internal sealed class ClrmdRuntime : ClrRuntime {
		private readonly IRuntimeHelpers _helpers;
		private ClrAppDomain[]? _domains;
		private ClrAppDomain? _systemDomain;
		private ClrAppDomain? _sharedDomain;
		private bool _disposed;

		public override bool IsThreadSafe => _helpers.DataReader.IsThreadSafe;
		public override DataTarget? DataTarget => ClrInfo?.DataTarget;
		internal override DacLibrary DacLibrary { get; }
		public override ClrInfo ClrInfo { get; }

		public override ClrAppDomain[] AppDomains {
			get {
				if (_domains is null)
					_domains = _helpers.GetAppDomains(this, out _systemDomain, out _sharedDomain);

				return _domains;
			}
		}

		public override ClrAppDomain? SharedDomain {
			get {
				if (_domains is null)
					_ = AppDomains;

				return _sharedDomain;
			}
		}

		public override ClrAppDomain? SystemDomain {
			get {
				if (_domains is null)
					_ = AppDomains;

				return _systemDomain;
			}
		}

		public ClrmdRuntime(ClrInfo info, DacLibrary dac, IRuntimeHelpers helpers) {
			ClrInfo = info;
			DacLibrary = dac;
			_helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
		}

		public void Initialize() {
			_ = AppDomains;
		}

		protected override void Dispose(bool disposing) {
			if (!_disposed) {
				_disposed = true;
				_helpers?.Dispose();
			}
		}

		/// <summary>
		/// Flushes the DAC cache.  This function <b>must</b> be called any time you expect to call the same function
		/// but expect different results.  For example, after walking the heap, you need to call Flush before
		/// attempting to walk the heap again.
		/// </summary>
		public override void FlushCachedData() {
			if (_disposed)
				throw new ObjectDisposedException(nameof(ClrRuntime));

			_domains = null;
			_systemDomain = null;
			_sharedDomain = null;

			_helpers.DataReader.FlushCachedData();
			_helpers.FlushCachedData();

			DacLibrary.Flush();
		}

		public override IEnumerable<ClrModule> EnumerateModules() {
			// In Desktop CLR, modules in the SharedDomain can potentially also be in every other domain.
			// To prevent duplicates we'll first enumerate all shared modules, then we'll make sure every
			// module we yield return after that isn't in the SharedDomain.
			// In .NET Core, there's only one AppDomain and no shared domain, so "sharedModules" will always be
			// Empty and we'll enumerate everything in the single domain.

			var sharedModules = SharedDomain?.Modules ?? Array.Empty<ClrModule>();

			foreach (var module in sharedModules)
				yield return module;

			// sharedModules will always contain a small number of items, so using the raw array will be better
			// than creating a tiny HashSet.
			foreach (var domain in AppDomains)
				foreach (var module in domain.Modules)
					if (Array.IndexOf(sharedModules, module) == -1)
						yield return module;

			if (SystemDomain != null)
				foreach (var module in SystemDomain.Modules)
					if (Array.IndexOf(sharedModules, module) == -1)
						yield return module;
		}
	}
}
