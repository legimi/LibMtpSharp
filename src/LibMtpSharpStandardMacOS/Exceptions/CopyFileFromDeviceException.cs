using System;

namespace LibMtpSharpStandardMacOS.Exceptions
{
    public class CopyFileFromDeviceException : Exception
    {
        public CopyFileFromDeviceException(uint fileId, string destPath)
            : base(
                $"Failed to retrieve the file from the device. {nameof(fileId)}: {fileId} {nameof(destPath)}: {destPath}")
        {
            if (string.IsNullOrEmpty(destPath))
                throw new ArgumentException("Value cannot be null or empty.", nameof(destPath));
        }
    }
}