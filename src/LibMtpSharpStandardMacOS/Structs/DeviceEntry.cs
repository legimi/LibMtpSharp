using System;
using System.Runtime.InteropServices;
using LibMtpSharpStandardMacOS.Utils;
using Optional;

namespace LibMtpSharpStandardMacOS.Structs
{
    /// <summary>
    /// A data structure to hold MTP device entries.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DeviceEntry
    {
        /// <summary>
        /// The vendor of this device
        /// </summary>
        public IntPtr VendorPtr;

        /// <summary>
        ///  Vendor ID for this device
        /// </summary>
        public ushort VendorId;

        /// <summary>
        /// The product name of this device
        /// </summary>
        public IntPtr ProductPtr;

        /// <summary>
        /// Product ID for this device
        /// </summary>
        public ushort ProductId;

        /// <summary>
        /// Bugs, device specifics etc
        /// </summary>
        public uint DeviceFlags;

        /// <summary>
        /// Product ID for this device
        /// </summary>
        public ushort DeviceBcd;

        public Option<string> Vendor => MarshalUtils.PtrToStringUTF8(VendorPtr);

        public Option<string> Product => MarshalUtils.PtrToStringUTF8(ProductPtr);

        public override string ToString() => $"(VID={VendorId:X}, PID={ProductId:X}, REV={DeviceBcd:X})";
    }
}