// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Diagnostics.Runtime.DacInterface;
using Microsoft.Diagnostics.Runtime.Implementation;

namespace Microsoft.Diagnostics.Runtime.Builders {
	internal sealed unsafe class RuntimeBuilder : IRuntimeHelpers, IAppDomainHelpers {
		private bool _disposed;
		private readonly ClrInfo _clrInfo;
		private readonly DacLibrary _library;
		private readonly ClrDataProcess _dac;
		private readonly SOSDac _sos;

		private readonly Dictionary<ulong, ClrAppDomain> _domains = new();
		private readonly Dictionary<ulong, ClrModule> _modules = new();

		private readonly ClrmdRuntime _runtime;

		private ModuleBuilder _moduleBuilder;

		public bool IsThreadSafe => true;

		public IDataReader DataReader { get; }

		public RuntimeBuilder(ClrInfo clr, DacLibrary library, SOSDac sos) {
			_clrInfo = clr;
			_library = library;
			_sos = sos;

			_dac = _library.DacPrivateInterface;
			DataReader = _clrInfo.DataTarget.DataReader;

			byte[] output = new byte[4];
			if (!_dac.Request(DacRequests.VERSION, Array.Empty<byte>(), output))
				throw new InvalidDataException("This instance of CLR either has not been initialized or does not contain any data.  Failed to request DacVersion.");

			int version = BitConverter.ToInt32(output, 0);
			if (version != 9)
				throw new NotSupportedException($"The CLR debugging layer reported a version of {version} which this build of ClrMD does not support.");

			_moduleBuilder = new ModuleBuilder(_sos);

			_runtime = new ClrmdRuntime(clr, library, this);
			_runtime.Initialize();

			library.DacDataTarget.SetMagicCallback(_dac.Flush);
		}

		public void Dispose() {
			if (!_disposed) {
				_disposed = true;
				_runtime?.Dispose();
				_dac.Dispose();
				_sos.Dispose();
				_library.Dispose();
			}
		}

		private ClrModule? GetModule(ulong addr) {
			lock (_modules) {
				_modules.TryGetValue(addr, out var module);
				return module;
			}
		}

		public ClrModule GetOrCreateModule(ClrAppDomain domain, ulong addr) {
			CheckDisposed();
			lock (_modules) {
				if (_modules.TryGetValue(addr, out var result))
					return result;

				if (_moduleBuilder.Init(addr))
					result = _modules[addr] = new ClrmdModule(domain, _moduleBuilder);
				else
					result = _modules[addr] = new ClrmdModule(domain, addr);

				return result;
			}
		}

		private void CheckDisposed() {
			// We will blame the runtime for being disposed if it's there because that will be more meaningful to the user.
			if (_disposed)
				throw new ObjectDisposedException(nameof(ClrRuntime));
		}

		ClrAppDomain[] IRuntimeHelpers.GetAppDomains(ClrRuntime runtime, out ClrAppDomain? system, out ClrAppDomain? shared) {
			CheckDisposed();

			system = null;
			shared = null;

			var builder = new AppDomainBuilder(_sos, this);

			if (builder.SystemDomain != 0)
				system = GetOrCreateAppDomain(builder, builder.SystemDomain);

			if (builder.SharedDomain != 0)
				shared = GetOrCreateAppDomain(builder, builder.SharedDomain);

			var domainList = _sos.GetAppDomainList(builder.AppDomainCount);
			var result = new ClrAppDomain[domainList.Length];

			for (int i = 0; i < domainList.Length; i++)
				result[i] = GetOrCreateAppDomain(builder, domainList[i]);

			return result;
		}

		public ClrAppDomain GetOrCreateAppDomain(AppDomainBuilder? builder, ulong domain) {
			CheckDisposed();

			lock (_domains) {
				if (_domains.TryGetValue(domain, out var result))
					return result;

				builder ??= new AppDomainBuilder(_sos, this);

				if (builder.Init(domain))
					return _domains[domain] = new ClrmdAppDomain(GetOrCreateRuntime(), builder);

				return _domains[domain] = new ClrmdAppDomain(GetOrCreateRuntime(), this, domain);
			}
		}

		void IRuntimeHelpers.FlushCachedData() {
			FlushDac();

			lock (_domains)
				_domains.Clear();

			lock (_modules) {
				_modules.Clear();

				_moduleBuilder = new ModuleBuilder(_sos);
			}

			if (_runtime is ClrmdRuntime runtime)
				lock (runtime)
					runtime.Initialize();
		}

		private void FlushDac() {
			// IXClrDataProcess::Flush is unfortunately not wrapped with DAC_ENTER.  This means that
			// when it starts deleting memory, it's completely unsynchronized with parallel reads
			// and writes, leading to heap corruption and other issues.  This means that in order to
			// properly clear dac data structures, we need to trick the dac into entering the critical
			// section for us so we can call Flush safely then.

			// To accomplish this, we set a hook in our implementation of IDacDataTarget::ReadVirtual
			// which will call IXClrDataProcess::Flush if the dac tries to read the address set by
			// MagicCallbackConstant.  Additionally we make sure this doesn't interfere with other
			// reads by 1) Ensuring that the address is in kernel space, 2) only calling when we've
			// entered a special context.

			_library.DacDataTarget.EnterMagicCallbackContext();
			try {
				_sos.GetWorkRequestData(DacDataTargetWrapper.MagicCallbackConstant, out _);
			}
			finally {
				_library.DacDataTarget.ExitMagicCallbackContext();
			}
		}

		IEnumerable<ClrModule> IAppDomainHelpers.EnumerateModules(ClrAppDomain domain) {
			CheckDisposed();

			foreach (ulong assembly in _sos.GetAssemblyList(domain.Address))
				foreach (ulong module in _sos.GetModuleList(assembly))
					yield return GetOrCreateModule(domain, module);
		}

		public ClrRuntime GetOrCreateRuntime() {
			return _runtime;
		}
	}
}
