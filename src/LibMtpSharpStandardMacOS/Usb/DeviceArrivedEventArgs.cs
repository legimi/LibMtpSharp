// Modifications made by Legimi in 2023.
// Based on https://github.com/LibUsbDotNet/LibUsbDotNet

namespace LibMtpSharpStandardMacOS.Usb
{
    public class DeviceArrivedEventArgs : DeviceEventArgs
    {
        public DeviceArrivedEventArgs(UsbDevice device)
        {
            Device = device;
        }

        public UsbDevice Device { get; }
    }
}