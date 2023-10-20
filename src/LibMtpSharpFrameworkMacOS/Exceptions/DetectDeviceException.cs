using System;
using LibMtpSharpFrameworkMacOS.Enums;

namespace LibMtpSharpFrameworkMacOS.Exceptions
{
    public class DetectDeviceException : ApplicationException
    {
        public DetectDeviceException(ErrorEnum error)
            : base($"Device detect error: {error}")
        {
        }
    }
}