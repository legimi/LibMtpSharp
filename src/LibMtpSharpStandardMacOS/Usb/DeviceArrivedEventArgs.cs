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