// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Diagnostics.Runtime {
	internal abstract class CommonMemoryReader : IMemoryReader {
		public virtual int PointerSize => IntPtr.Size;

		public abstract int Read(ulong address, ref byte buffer, uint length);

		public unsafe bool Read<T>(ulong address, out T value)
			where T : unmanaged {
			byte[] buffer = new byte[sizeof(T)];
			if (Read(address, ref buffer[0], (uint)sizeof(T)) == buffer.Length) {
				value = Unsafe.As<byte, T>(ref buffer[0]);
				return true;
			}

			value = default;
			return false;
		}

		public T Read<T>(ulong address)
			where T : unmanaged {
			Read(address, out T result);
			return result;
		}

		public bool ReadPointer(ulong address, out ulong value) {
			byte[] buffer = new byte[IntPtr.Size];
			if (Read(address, ref buffer[0], (uint)IntPtr.Size) == IntPtr.Size) {
				value = Unsafe.As<byte, nuint>(ref buffer[0]);
				return true;
			}

			value = 0;
			return false;
		}

		public ulong ReadPointer(ulong address) {
			ReadPointer(address, out ulong value);
			return value;
		}
	}
}
