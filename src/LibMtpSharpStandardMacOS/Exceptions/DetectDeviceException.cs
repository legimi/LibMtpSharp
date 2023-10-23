using System;
using LibMtpSharpStandardMacOS.Enums;

namespace LibMtpSharpStandardMacOS.Exceptions
{
    public class DetectDeviceException : ApplicationException
    {
        public DetectDeviceException(ErrorEnum error)
            : base($"Device detect error: {error}")
        {
        }
    }
}