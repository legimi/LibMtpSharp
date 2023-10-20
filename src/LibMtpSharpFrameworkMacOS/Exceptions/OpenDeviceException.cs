using System;
using LibMtpSharpFrameworkMacOS.Structs;

namespace LibMtpSharpFrameworkMacOS.Exceptions
{
    public class OpenDeviceException : ApplicationException
    {
        public OpenDeviceException(RawDevice rawDevice)
            : base($"Failed to open {rawDevice}")
        {
        }
    }
}