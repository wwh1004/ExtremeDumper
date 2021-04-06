// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;

namespace Microsoft.Diagnostics.Runtime {
	/// <summary>
	/// Infers clr info from module names, provides corresponding DAC details.
	/// </summary>
	internal static class ClrInfoProvider {
		private const string c_desktopModuleName1 = "clr.dll";
		private const string c_desktopModuleName2 = "mscorwks.dll";
		private const string c_coreModuleName = "coreclr.dll";

		private const string c_desktopDacFileNameBase = "mscordacwks";
		private const string c_coreDacFileNameBase = "mscordaccore";
		private const string c_desktopDacFileName = c_desktopDacFileNameBase + ".dll";
		private const string c_coreDacFileName = c_coreDacFileNameBase + ".dll";

		/// <summary>
		/// Checks if the provided module corresponds to a supported runtime, gets clr details inferred from the module name.
		/// </summary>
		/// <param name="moduleInfo">Module info.</param>
		/// <param name="flavor">CLR flavor.</param>
		/// <returns>true if module corresponds to a supported runtime.</returns>
		public static bool IsSupportedRuntime(ModuleInfo moduleInfo, out ClrFlavor flavor) {
			if (moduleInfo is null)
				throw new ArgumentNullException(nameof(moduleInfo));

			flavor = default;

			string? moduleName = Path.GetFileName(moduleInfo.FileName);
			if (moduleName is null)
				return false;

			if (moduleName.Equals(c_desktopModuleName1, StringComparison.OrdinalIgnoreCase) ||
				moduleName.Equals(c_desktopModuleName2, StringComparison.OrdinalIgnoreCase)) {
				flavor = ClrFlavor.Desktop;
				return true;
			}

			if (moduleName.Equals(c_coreModuleName, StringComparison.OrdinalIgnoreCase)) {
				flavor = ClrFlavor.Core;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Returns the file name of the DAC dll according to the specified parameters.
		/// </summary>
		public static string GetDacFileName(ClrFlavor flavor) {
			return flavor == ClrFlavor.Core ? c_coreDacFileName : c_desktopDacFileName;
		}

		/// <summary>
		/// Returns the file name of the DAC dll for the requests to the symbol server.
		/// </summary>
		public static string GetDacRequestFileName(ClrFlavor flavor, Architecture currentArchitecture, Architecture targetArchitecture, Version version) {
			string? dacNameBase = flavor == ClrFlavor.Core ? c_coreDacFileNameBase : c_desktopDacFileNameBase;
			return $"{dacNameBase}_{currentArchitecture}_{targetArchitecture}_{version.Major}.{version.Minor}.{version.Build}.{version.Revision:D2}.dll";
		}
	}
}
