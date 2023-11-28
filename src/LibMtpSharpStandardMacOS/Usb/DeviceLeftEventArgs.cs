// Modifications made by Legimi in 2023.
// Based on https://github.com/LibUsbDotNet/LibUsbDotNet

using LibMtpSharpStandardMacOS.Usb.Info;

namespace LibMtpSharpStandardMacOS.Usb
{
    public class DeviceLeftEventArgs : DeviceEventArgs
    {
        public DeviceLeftEventArgs(CachedDeviceInfo info)
        {
            DeviceInfo = info;
        }
    
        public CachedDeviceInfo DeviceInfo { get; }
    }
}