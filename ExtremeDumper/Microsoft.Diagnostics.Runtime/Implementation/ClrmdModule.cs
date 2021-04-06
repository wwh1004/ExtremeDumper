// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Microsoft.Diagnostics.Runtime.Implementation {
	internal sealed class ClrmdModule : ClrModule {
		public override ClrAppDomain AppDomain { get; }
		public override string? Name { get; }
		public override string? AssemblyName { get; }
		public override ulong AssemblyAddress { get; }
		public override ulong Address { get; }
		public override bool IsPEFile { get; }
		public override ulong ImageBase { get; }
		public override ModuleLayout Layout { get; }
		public override ulong Size { get; }
		public override ulong MetadataAddress { get; }
		public override ulong MetadataLength { get; }
		public override bool IsDynamic { get; }

		public ClrmdModule(ClrAppDomain parent, IModuleData data) {
			if (data is null)
				throw new ArgumentNullException(nameof(data));

			AppDomain = parent;
			Name = data.Name;
			AssemblyName = data.AssemblyName;
			AssemblyAddress = data.AssemblyAddress;
			Address = data.Address;
			IsPEFile = data.IsPEFile;
			ImageBase = data.ILImageBase;
			Layout = data.IsFlatLayout ? ModuleLayout.Flat : ModuleLayout.Unknown;
			Size = data.Size;
			MetadataAddress = data.MetadataStart;
			MetadataLength = data.MetadataLength;
			IsDynamic = data.IsReflection || string.IsNullOrWhiteSpace(Name);
		}

		public ClrmdModule(ClrAppDomain parent, ulong addr) {
			AppDomain = parent;
			Address = addr;
		}
	}
}
