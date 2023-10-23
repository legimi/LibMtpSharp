using System;
using System.Runtime.InteropServices;
using LibMtpSharpStandardMacOS.Utils;
using Optional;

namespace LibMtpSharpStandardMacOS.Structs
{
    /// <summary>
    /// LIBMTP Device Storage structure
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DeviceStorageStruct
    {
        /// <summary>
        /// Unique ID for this storage
        /// </summary>
        public uint Id;

        /// <summary>
        /// Storage type
        /// </summary>
        ushort StorageType;

        /// <summary>
        /// Filesystem type
        /// </summary>
        ushort FilesystemType;

        /// <summary>
        ///  Access capability
        /// </summary>
        ushort AccessCapability;

        /// <summary>
        /// Maximum capability
        /// </summary>
        public ulong MaxCapacity;

        /// <summary>
        /// Free space in bytes
        /// </summary>
        public ulong FreeSpaceInBytes;

        /// <summary>
        /// Free space in objects
        /// </summary>
        public ulong FreeSpaceInObjects;

        /// <summary>
        /// A brief description of this storage
        /// </summary>
        public IntPtr StorageDescriptionPtr;

        /// <summary>
        /// A volume identifier
        /// </summary>
        public IntPtr VolumeIdentifierPtr;

        /// <summary>
        /// Next storage, follow this link until NULL
        /// </summary>
        internal IntPtr next;

        /// <summary>
        /// Previous storage
        /// </summary>
        IntPtr prev;

        public Option<string> StorageDescription => MarshalUtils.PtrToStringUTF8(StorageDescriptionPtr);

        public Option<string> VolumeIdentifier => MarshalUtils.PtrToStringUTF8(VolumeIdentifierPtr);
    }
}