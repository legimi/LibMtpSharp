namespace LibMtpSharpStandardMacOS.Usb.Info
{
    public class CachedDeviceInfo
    {
        private readonly UsbDevice _device;

        public CachedDeviceInfo(UsbDevice device)
        {
            _device = device;
        }

        protected bool Equals(CachedDeviceInfo other)
        {
            return GetHashCode() == other.GetHashCode();
        }

        public override int GetHashCode() => _device.GetHashCode();
    }
}