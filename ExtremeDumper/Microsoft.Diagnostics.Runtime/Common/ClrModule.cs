// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Microsoft.Diagnostics.Runtime {
	/// <summary>
	/// Represents a managed module in the target process.
	/// </summary>
	public abstract class ClrModule :
#nullable disable // to enable use with both T and T? for reference types due to IEquatable<T> being invariant
		IEquatable<ClrModule>
#nullable restore
	{
		/// <summary>
		/// Gets the address of the clr!Module object.
		/// </summary>
		public abstract ulong Address { get; }

		/// <summary>
		/// Gets the AppDomain parent of this module.
		/// </summary>
		public abstract ClrAppDomain AppDomain { get; }

		/// <summary>
		/// Gets the name of the assembly that this module is defined in.
		/// </summary>
		public abstract string? AssemblyName { get; }

		/// <summary>
		/// Gets an identifier to uniquely represent this assembly.  This value is not used by any other
		/// function in ClrMD, but can be used to group modules by their assembly.  (Do not use AssemblyName
		/// for this, as reflection and other special assemblies can share the same name, but actually be
		/// different.)
		/// </summary>
		public abstract ulong AssemblyAddress { get; }

		/// <summary>
		/// Gets the name of the module.
		/// </summary>
		public abstract string? Name { get; }

		/// <summary>
		/// Gets a value indicating whether this module was created through <c>System.Reflection.Emit</c> (and thus has no associated
		/// file).
		/// </summary>
		public abstract bool IsDynamic { get; }

		/// <summary>
		/// Gets a value indicating whether this module is an actual PEFile on disk.
		/// </summary>
		public abstract bool IsPEFile { get; }

		/// <summary>
		/// Gets the base of the image loaded into memory.  This may be 0 if there is not a physical
		/// file backing it.
		/// </summary>
		public abstract ulong ImageBase { get; }

		/// <summary>
		/// Returns the in memory layout for PEImages.
		/// </summary>
		public abstract ModuleLayout Layout { get; }

		/// <summary>
		/// Gets the size of the image in memory.
		/// </summary>
		public abstract ulong Size { get; }

		/// <summary>
		/// Gets the location of metadata for this module in the process's memory.  This is useful if you
		/// need to manually create IMetaData* objects.
		/// </summary>
		public abstract ulong MetadataAddress { get; }

		/// <summary>
		/// Gets the length of the metadata for this module.
		/// </summary>
		public abstract ulong MetadataLength { get; }

		/// <summary>
		/// Returns a name for the assembly.
		/// </summary>
		/// <returns>A name for the assembly.</returns>
		public override string? ToString() {
			if (string.IsNullOrEmpty(Name)) {
				if (!string.IsNullOrEmpty(AssemblyName))
					return AssemblyName;

				if (IsDynamic)
					return "dynamic";
			}

			return Name;
		}

		public override bool Equals(object? obj) {
			return Equals(obj as ClrModule);
		}

		public bool Equals(ClrModule? other) {
			if (ReferenceEquals(this, other))
				return true;

			if (other is null)
				return false;

			return Address == other.Address;
		}

		public override int GetHashCode() {
			return Address.GetHashCode();
		}

		public static bool operator ==(ClrModule? left, ClrModule? right) {
			if (right is null)
				return left is null;

			return right.Equals(left);
		}

		public static bool operator !=(ClrModule? left, ClrModule? right) => !(left == right);
	}
}
