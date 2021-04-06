// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Diagnostics.Runtime.Utilities;

namespace Microsoft.Diagnostics.Runtime {
	/// <summary>
	/// A crash dump or live process to read out of.
	/// </summary>
	public sealed class DataTarget : IDisposable {
		private readonly CustomDataTarget _target;
		private bool _disposed;
		private ClrInfo[]? _clrs;
		private ModuleInfo[]? _modules;

		/// <summary>
		/// Gets the data reader for this instance.
		/// </summary>
		internal IDataReader DataReader { get; }

		/// <summary>
		/// Creates a DataTarget from the given reader.
		/// </summary>
		/// <param name="customTarget">The custom data target to use.</param>
		internal DataTarget(CustomDataTarget customTarget) {
			_target = customTarget ?? throw new ArgumentNullException(nameof(customTarget));
			DataReader = _target.DataReader;
		}

		public void Dispose() {
			if (!_disposed) {
				_target.Dispose();
				_disposed = true;
			}
		}

		[Conditional("DEBUG")]
		private void DebugOnlyLoadLazyValues() {
			// Prefetch these values in debug builds for easier debugging
			GetOrCreateClrVersions();
			EnumerateModules();
		}

		/// <summary>
		/// Gets the list of CLR versions loaded into the process.
		/// </summary>
		public ClrInfo[] ClrVersions => GetOrCreateClrVersions();

		private ClrInfo[] GetOrCreateClrVersions() {
			if (_disposed)
				throw new ObjectDisposedException(nameof(DataTarget));

			if (_clrs != null)
				return _clrs;

			var arch = DataReader.Architecture;
			var versions = new List<ClrInfo>(2);
			foreach (var module in EnumerateModules()) {
				byte[] runtimeBuildId = Array.Empty<byte>();
				int runtimeTimeStamp = 0;
				int runtimeFileSize = 0;

				if (ClrInfoProvider.IsSupportedRuntime(module, out var flavor)) {
					runtimeTimeStamp = module.IndexTimeStamp;
					runtimeFileSize = module.IndexFileSize;
					runtimeBuildId = module.BuildId;
				}
				else {
					continue;
				}

				string dacFileName = ClrInfoProvider.GetDacFileName(flavor);
				string? dacLocation = Path.Combine(Path.GetDirectoryName(module.FileName)!, dacFileName);

				if (!File.Exists(dacLocation) || !PlatformFunctions.IsEqualFileVersion(dacLocation, module.Version)) {
					dacLocation = null;
				}

				var version = module.Version;
				string dacAgnosticName = ClrInfoProvider.GetDacRequestFileName(flavor, arch, arch, version);
				string dacRegularName = ClrInfoProvider.GetDacRequestFileName(flavor, IntPtr.Size == 4 ? Architecture.X86 : Architecture.Amd64, arch, version);

				var dacInfo = new DacInfo(dacLocation, dacRegularName, dacAgnosticName, arch, runtimeFileSize, runtimeTimeStamp, version, runtimeBuildId);
				versions.Add(new ClrInfo(this, flavor, module, dacInfo));
			}

			_clrs = versions.ToArray();
			return _clrs;
		}

		/// <summary>
		/// Enumerates information about the loaded modules in the process (both managed and unmanaged).
		/// </summary>
		public IEnumerable<ModuleInfo> EnumerateModules() {
			if (_disposed)
				throw new ObjectDisposedException(nameof(DataTarget));

			if (_modules != null)
				return _modules;

			char[] invalid = Path.GetInvalidPathChars();
			var modules = DataReader.EnumerateModules().Where(m => m.FileName != null && m.FileName.IndexOfAny(invalid) < 0).ToArray();
			Array.Sort(modules, (a, b) => a.ImageBase.CompareTo(b.ImageBase));

			return _modules = modules;
		}

		/// <summary>
		/// Gets a set of helper functions that are consistently implemented across all platforms.
		/// </summary>
		internal static PlatformFunctions PlatformFunctions { get; } = new WindowsFunctions();

		/// <summary>
		/// Attaches to a running process.  Note that if <paramref name="suspend"/> is set to false the user
		/// of ClrMD is still responsible for suspending the process itself.  ClrMD does NOT support inspecting
		/// a running process and will produce undefined behavior when attempting to do so.
		/// </summary>
		/// <param name="processId">The ID of the process to attach to.</param> 
		/// <param name="suspend">Whether or not to suspend the process.</param>
		/// <returns>A <see cref="DataTarget"/> instance.</returns>
		[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
		public static DataTarget AttachToProcess(int processId, bool suspend) {
			var mode = suspend ? WindowsProcessDataReaderMode.Suspend : WindowsProcessDataReaderMode.Passive;
			return new DataTarget(new CustomDataTarget(new WindowsProcessDataReader(processId, mode)));
		}

		/// <summary>
		/// Creates a snapshot of a running process and attaches to it.  This method will pause a running process
		/// 
		/// </summary>
		/// <param name="processId">The ID of the process to attach to.</param>
		/// <returns>A <see cref="DataTarget"/> instance.</returns>
		/// <exception cref="ArgumentException">
		/// The process specified by <paramref name="processId"/> is not running.
		/// </exception>
		/// <exception cref="PlatformNotSupportedException">
		/// The current platform is not Windows.
		/// </exception>
		[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
		public static DataTarget CreateSnapshotAndAttach(int processId) {
			var customTarget = new CustomDataTarget(new WindowsProcessDataReader(processId, WindowsProcessDataReaderMode.Snapshot));
			return new DataTarget(customTarget);
		}
	}
}
