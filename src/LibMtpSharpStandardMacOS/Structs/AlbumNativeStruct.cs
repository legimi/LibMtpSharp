using System;
using System.Runtime.InteropServices;
using LibMtpSharpStandardMacOS.Utils;
using Optional;

namespace LibMtpSharpStandardMacOS.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct AlbumNativeStruct
    {
        /// <summary>
        /// Unique playlist ID
        /// </summary>
        public uint album_id;

        /// <summary>
        /// ID of parent folder
        /// </summary>
        public uint parent_id;

        /// <summary>
        /// ID of storage holding this album
        /// </summary>
        public uint storage_id;

        /// <summary>
        /// Name of album
        /// </summary>
        public IntPtr namePtr;

        /// <summary>
        /// Name of album artist
        /// </summary>
        public IntPtr artistPtr;

        /// <summary>
        /// Name of recording composer
        /// </summary>
        public IntPtr composerPtr;

        /// <summary>
        ///  Genre of album
        /// </summary>
        public IntPtr genrePtr;

        /// <summary>
        /// The tracks in this album
        /// </summary>
        public IntPtr tracks;

        /// <summary>
        /// The number of tracks in this album
        /// </summary>
        public uint no_tracks;

        /// <summary>
        /// Next album or NULL if last album
        /// </summary>
        internal IntPtr next;

        // Helper properties/methods for string conversion (optional)
        public Option<string> Name => MarshalUtils.PtrToStringUTF8(namePtr);

        public Option<string> Artist => MarshalUtils.PtrToStringUTF8(artistPtr);

        public Option<string> Composer => MarshalUtils.PtrToStringUTF8(composerPtr);

        public Option<string> Genre => MarshalUtils.PtrToStringUTF8(genrePtr);
    }
}