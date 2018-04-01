using System;
using static ExtremeDumper.Dumper.NativeMethods;

namespace ExtremeDumper.Dumper
{
    internal struct SafeNativeHandle : IDisposable
    {
        private IntPtr _handle;

        private bool _isDisposed;

        public bool IsValid => _handle != IntPtr.Zero;

        public static implicit operator SafeNativeHandle(IntPtr value) => new SafeNativeHandle() { _handle = value };

        public static implicit operator IntPtr(SafeNativeHandle value) => value._handle;

        public void Dispose()
        {
            if (_isDisposed)
                return;

            if (_handle != CURRENT_PROCESS)
                CloseHandle(_handle);
            _isDisposed = true;
        }
    }
}
