﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Microsoft.Diagnostics.Runtime {
	[Flags]
	internal enum PSS_CAPTURE_FLAGS : uint {
		PSS_CAPTURE_NONE = 0x00000000,
		PSS_CAPTURE_VA_CLONE = 0x00000001,
		PSS_CAPTURE_RESERVED_00000002 = 0x00000002,
		PSS_CAPTURE_HANDLES = 0x00000004,
		PSS_CAPTURE_HANDLE_NAME_INFORMATION = 0x00000008,
		PSS_CAPTURE_HANDLE_BASIC_INFORMATION = 0x00000010,
		PSS_CAPTURE_HANDLE_TYPE_SPECIFIC_INFORMATION = 0x00000020,
		PSS_CAPTURE_HANDLE_TRACE = 0x00000040,
		PSS_CAPTURE_THREADS = 0x00000080,
		PSS_CAPTURE_THREAD_CONTEXT = 0x00000100,
		PSS_CAPTURE_THREAD_CONTEXT_EXTENDED = 0x00000200,
		PSS_CAPTURE_RESERVED_00000400 = 0x00000400,
		PSS_CAPTURE_VA_SPACE = 0x00000800,
		PSS_CAPTURE_VA_SPACE_SECTION_INFORMATION = 0x00001000,
		PSS_CREATE_BREAKAWAY_OPTIONAL = 0x04000000,
		PSS_CREATE_BREAKAWAY = 0x08000000,
		PSS_CREATE_FORCE_BREAKAWAY = 0x10000000,
		PSS_CREATE_USE_VM_ALLOCATIONS = 0x20000000,
		PSS_CREATE_MEASURE_PERFORMANCE = 0x40000000,
		PSS_CREATE_RELEASE_SECTION = 0x80000000
	}
}