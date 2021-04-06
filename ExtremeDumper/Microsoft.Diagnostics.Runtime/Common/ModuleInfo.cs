// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Microsoft.Diagnostics.Runtime {
	/// <summary>
	/// Provides information about loaded modules in a <see cref="DataTarget"/>.
	/// </summary>
	public sealed class ModuleInfo {
		private byte[] _buildId;
		private Version? _version;
		private readonly IDataReader _dataReader;

		internal IDataReader DataReader => _dataReader;

		/// <summary>
		/// Gets the base address of the object.
		/// </summary>
		public ulong ImageBase { get; }

		/// <summary>
		/// Gets the specific file size of the image used to index it on the symbol server.
		/// </summary>
		public int IndexFileSize { get; }

		/// <summary>
		/// Gets the timestamp of the image used to index it on the symbol server.
		/// </summary>
		public int IndexTimeStamp { get; }

		/// <summary>
		/// Gets the file name of the module on disk.
		/// </summary>
		public string? FileName { get; }

		/// <summary>
		/// Gets the Linux BuildId of this module.  This will be <see langword="null"/> if the module does not have a BuildId.
		/// </summary>
		public byte[] BuildId {
			get {
				if (_buildId is null) {
					return _buildId = DataReader.GetBuildId(ImageBase);
				}

				return _buildId;
			}
		}

		public override string? ToString() {
			return FileName;
		}

		/// <summary>
		/// Gets the version information for this file.
		/// </summary>
		public Version Version {
			get {
				if (_version != null)
					return _version;

				_version = DataReader.GetVersionInfo(ImageBase, out var version) ? new Version(version.Major, version.Minor, version.Build, version.Revision) : new Version(0, 0, 0, 0);
				return _version;
			}
		}

		// DataTarget is one of the few "internal set" properties, and is initialized as soon as DataTarget asks
		// IDataReader to create ModuleInfo.  So even though we don't set it here, we will immediately set the
		// value to non-null and never change it.

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="reader">The <see cref="IDataReader"/> containing this module.</param>
		/// <param name="imageBase">The base of the image as loaded into the virtual address space.</param>
		/// <param name="fileName">The full path of the file as loaded from disk (if possible), otherwise only the filename.</param>
		/// <param name="indexFileSize">The index file size used by the symbol server to archive and request this binary.  Only for PEImages (not Elf or Mach-O binaries).</param>
		/// <param name="indexTimeStamp">The index timestamp used by the symbol server to archive and request this binary.  Only for PEImages (not Elf or Mach-O binaries).</param>
		/// <param name="buildId">The ELF buildid of this image.  Not valid for PEImages.</param>
		internal ModuleInfo(IDataReader reader, ulong imageBase, string? fileName, int indexFileSize, int indexTimeStamp, byte[] buildId) {
			_dataReader = reader;
			ImageBase = imageBase;
			IndexFileSize = indexFileSize;
			IndexTimeStamp = indexTimeStamp;
			FileName = fileName;
			_buildId = buildId;
		}
	}
}
