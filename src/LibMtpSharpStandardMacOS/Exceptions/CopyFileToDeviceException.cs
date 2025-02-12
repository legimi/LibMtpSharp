using System;

namespace LibMtpSharpStandardMacOS.Exceptions
{
    public class CopyFileToDeviceException : Exception
    {
        public CopyFileToDeviceException(string message)
            : base(message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
        }
    }
}