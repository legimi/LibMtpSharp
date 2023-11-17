using System;

namespace LibMtpSharpStandardMacOS.Structs
{
    [Flags]
    public enum HotplugEvent : byte
    {
        /// <summary>
        ///  A device has been plugged in and is ready to use 
        /// </summary>
        DeviceArrived = 0x1,

        /// <summary>
        /// A device has left and is no longer available.
        /// </summary>
        /// <remarks>
        /// It is the user's responsibility to call <see cref="UsbDevice.Close"/> on any handle associated with a disconnected device.
        /// It is safe to call <see cref="NativeMethods.GetDeviceDescriptor"/> on a device that has left.
        /// </remarks>
        DeviceLeft = 0x2
    }
}