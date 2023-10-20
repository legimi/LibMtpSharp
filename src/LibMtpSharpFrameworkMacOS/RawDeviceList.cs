using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LibMtpSharpFrameworkMacOS.Enums;
using LibMtpSharpFrameworkMacOS.Exceptions;
using LibMtpSharpFrameworkMacOS.Structs;

namespace LibMtpSharpFrameworkMacOS
{
    public class RawDeviceList : IEnumerable<RawDevice>, IDisposable
    {
        private readonly int _numberOfDevices;
        private readonly IntPtr _deviceListPointer;

        public RawDeviceList()
        {
            var error = NativeAPI.LibMtpLibrary.DetectRawDevices(ref _deviceListPointer, ref _numberOfDevices);
            if (error == ErrorEnum.NoDeviceAttached)
                return;
            if (error != ErrorEnum.None)
                throw new DetectDeviceException(error);
        }

        public IEnumerator<RawDevice> GetEnumerator()
        {
            for (var i = 0; i < _numberOfDevices; i++)
            {
                var offset = _deviceListPointer + i * Marshal.SizeOf(typeof(RawDevice));
                var deviceObject = Marshal.PtrToStructure(offset, typeof(RawDevice));
                yield return (RawDevice)deviceObject!;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void ReleaseUnmanagedResources()
        {
            if (_deviceListPointer != IntPtr.Zero)
                NativeAPI.LibMtpLibrary.Free(_deviceListPointer);
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~RawDeviceList()
        {
            ReleaseUnmanagedResources();
        }
    }
}