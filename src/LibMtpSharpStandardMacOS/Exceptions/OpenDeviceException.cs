using System;
using LibMtpSharpStandardMacOS.Structs;

namespace LibMtpSharpStandardMacOS.Exceptions
{
    public class OpenDeviceException : ApplicationException
    {
        public OpenDeviceException(RawDevice rawDevice)
            : base($"Failed to open {rawDevice}")
        {
        }
    }
}