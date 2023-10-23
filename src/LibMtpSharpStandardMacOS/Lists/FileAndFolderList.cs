using System;
using LibMtpSharpStandardMacOS.Structs;

namespace LibMtpSharpStandardMacOS.Lists
{
    internal class FileAndFolderList : UnmanagedList<FileStruct>
    {
        public FileAndFolderList(IntPtr mptDeviceStructPointer, uint storageId, uint parentId)
            : base(NativeAPI.LibMtpLibrary.GetParentContent(mptDeviceStructPointer, storageId, parentId))
        {
        }

        protected override IntPtr GetPointerToNextItem(ref FileStruct item) => item.next;

        protected override void FreeItem(IntPtr item)
        {
            NativeAPI.LibMtpLibrary.DestroyFile(item);
        }
    }
}