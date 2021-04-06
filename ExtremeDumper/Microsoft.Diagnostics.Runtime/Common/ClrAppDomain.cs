// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Diagnostics.Runtime {
	/// <summary>
	/// Represents an AppDomain in the target runtime.
	/// </summary>
	public abstract class ClrAppDomain {
		/// <summary>
		/// Gets the runtime associated with this ClrAppDomain.
		/// </summary>
		public abstract ClrRuntime Runtime { get; }

		/// <summary>
		/// Gets address of the AppDomain.
		/// </summary>
		public abstract ulong Address { get; }

		/// <summary>
		/// Gets the AppDomain's ID.
		/// </summary>
		public abstract int Id { get; }

		/// <summary>
		/// Gets the name of the AppDomain, as specified when the domain was created.
		/// </summary>
		public abstract string? Name { get; }

		/// <summary>
		/// Gets a list of modules loaded into this AppDomain.
		/// </summary>
		public abstract ClrModule[] Modules { get; }

		/// <summary>
		/// To string override.
		/// </summary>
		/// <returns>The name of this AppDomain.</returns>
		public override string? ToString() {
			return Name;
		}
	}
}
