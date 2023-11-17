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