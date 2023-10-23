using System;
using System.Runtime.InteropServices;
using LibMtpSharpStandardMacOS.Utils;
using Optional;

namespace LibMtpSharpStandardMacOS.Structs
{
    /// <summary>
    /// MTP device extension holder struct
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct DeviceExtension
    {
        /// <summary>
        /// Name of extension e.g. "foo.com"
        /// </summary>
        public IntPtr namePtr;

        /// <summary>
        /// Major revision of extension
        /// </summary>
        int major;

        /// <summary>
        /// Minor revision of extension
        /// </summary>
        int minor;

        /// <summary>
        /// Pointer to the next extension or NULL if this is the last extension.
        /// </summary>
        IntPtr next;

        public Option<string> Name => MarshalUtils.PtrToStringUTF8(namePtr);
    }
}