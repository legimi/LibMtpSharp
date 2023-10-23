using System;
using System.Runtime.InteropServices;
using LibMtpSharpStandardMacOS.Enums;
using LibMtpSharpStandardMacOS.Utils;
using Optional;

namespace LibMtpSharpStandardMacOS.Structs
{
    /// <summary>
    /// MTP file struct
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct FileStruct
    {
        /// <summary>
        /// Unique item ID
        /// </summary>
        public uint ItemId;

        /// <summary>
        /// ID of parent folder
        /// </summary>
        public uint ParentId;

        /// <summary>
        /// ID of storage holding this file
        /// </summary>
        public uint StorageId;

        /// <summary>
        /// Filename of this file
        /// </summary>
        public IntPtr FileNamePtr;

        /// <summary>
        /// Size of file in bytes
        /// </summary>
        public ulong FileSize;

        //Todo: check long fits
        /// <summary>
        /// Date of last alteration of the file
        /// </summary>
        public long ModificationDate;

        /// <summary>
        /// Filetype used for the current file
        /// </summary>
        public FileTypeEnum Filetype;

        /// <summary>
        ///  Next file in list or NULL if last file 
        /// </summary>
        internal IntPtr next;

        public string FileName
        {
            get => MarshalUtils.PtrToStringUTF8(FileNamePtr).ValueOr(string.Empty);
            set
            {
                // Free the previous memory if it was allocated
                if (FileNamePtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(FileNamePtr);

                FileNamePtr = MarshalUtils.StringToPtrUTF8(value);
            }
        }
    }
}