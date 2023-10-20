using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using LibMtpSharpFrameworkMacOS.Enums;
using LibMtpSharpFrameworkMacOS.Exceptions;
using LibMtpSharpFrameworkMacOS.Lists;
using LibMtpSharpFrameworkMacOS.NativeAPI;
using LibMtpSharpFrameworkMacOS.Structs;
using Optional;

namespace LibMtpSharpFrameworkMacOS
{
    public class OpenedMtpDevice : IDisposable
    {
        private readonly IntPtr _mptDeviceStructPointer;
        private readonly bool _cached;
        private readonly uint _vendorId;

        public OpenedMtpDevice(ref RawDevice rawDevice, bool cached)
        {
            _vendorId = rawDevice.DeviceEntry.VendorId;
            _cached = cached;
            _mptDeviceStructPointer = _cached
                ? LibMtpLibrary.OpenRawDevice(ref rawDevice)
                : LibMtpLibrary.OpenRawDeviceUncached(ref rawDevice);
            if (_mptDeviceStructPointer == IntPtr.Zero)
                throw new OpenDeviceException(rawDevice);
        }

        public uint GetVendorId() => _vendorId;

        private MtpDeviceStruct CurrentMtpDeviceStruct =>
            Marshal.PtrToStructure<MtpDeviceStruct>(_mptDeviceStructPointer);

        public IEnumerable<DeviceStorageStruct> GetStorages()
        {
            var deviceStorage = Marshal.PtrToStructure<DeviceStorageStruct>(CurrentMtpDeviceStruct.storage);
            yield return deviceStorage;
            while (deviceStorage.next != IntPtr.Zero)
            {
                deviceStorage = Marshal.PtrToStructure<DeviceStorageStruct>(deviceStorage.next);
                yield return deviceStorage;
            }
        }

        private IEnumerable<FileStruct> GetFolderContent(uint storageId, uint? folderId)
        {
            if (_cached)
                throw new ApplicationException(
                    "GetFolderContent cannot be called on cached device. Open device with cached: false");
            using var fileList = new FileAndFolderList(_mptDeviceStructPointer, storageId,
                folderId ?? LibMtpLibrary.LibmtpFilesAndFoldersRoot);
            foreach (var file in fileList)
                yield return file;
        }

        private void ReleaseUnmanagedResources()
        {
            if (_mptDeviceStructPointer != IntPtr.Zero)
                LibMtpLibrary.ReleaseDevice(_mptDeviceStructPointer);
        }

        public IEnumerable<FileStruct> GetAllItems(uint storageId, uint parentId = LibMtpLibrary.LibmtpFilesAndFoldersRoot)
        {
            var result = new List<FileStruct>();
            var filesStruct = GetFolderContent(storageId, parentId);

            foreach (var fileStruct in filesStruct)
            {
                result.Add(fileStruct);
                if (fileStruct.Filetype != FileTypeEnum.Folder)
                    continue;

                result.AddRange(GetAllItems(storageId, fileStruct.ItemId));
            }

            return result;
        }

        public Option<FileStruct> GetFile(uint storageId, string fileName, string fileExtension) =>
            GetMatchingItem(storageId, x =>
            {
                if (x.Filetype == FileTypeEnum.Folder)
                    return false;

                var extension = Path.GetExtension(x.FileName);
                if (string.Compare(fileExtension.TrimStart('.'), extension.TrimStart('.'), StringComparison.InvariantCultureIgnoreCase) != 0)
                    return false;

                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(x.FileName);
                return string.Compare(fileNameWithoutExtension, fileName, StringComparison.InvariantCultureIgnoreCase) == 0;
            });

        public Option<FileStruct> GetDirectory(uint storageId, string fileName) =>
            GetMatchingItem(storageId, x =>
                string.Compare(x.FileName, fileName, StringComparison.InvariantCultureIgnoreCase) == 0
                && x.Filetype == FileTypeEnum.Folder);

        private Option<FileStruct> GetMatchingItem(uint storageId, Func<FileStruct, bool> predicate, uint parentId = LibMtpLibrary.LibmtpFilesAndFoldersRoot)
        {
            var filesStruct = GetFolderContent(storageId, parentId);

            foreach (var fileStruct in filesStruct)
            {
                if (predicate(fileStruct))
                    return fileStruct.Some();

                if (fileStruct.Filetype != FileTypeEnum.Folder)
                    continue;

                var matchingSubFile = GetMatchingItem(storageId, predicate, fileStruct.ItemId);
                if (matchingSubFile.HasValue)
                    return matchingSubFile;
            }

            return Option.None<FileStruct>();
        }

        public void CopyAzwFileToDevice(string filePath, uint storageId, uint parentId)
        {
            var fileInfo = new FileInfo(filePath);
            var fileStruct = new FileStruct
            {
                FileName = fileInfo.Name,
                FileSize = (ulong)fileInfo.Length,
                Filetype = FileTypeEnum.Unknown,
                ParentId = parentId,
                StorageId = storageId
            };

            var result = LibMtpLibrary.SendFile(_mptDeviceStructPointer, filePath, ref fileStruct, null, IntPtr.Zero);
            if (result != 0)
                throw new CopyFileToDeviceException($"Cannot copy {fileInfo.Name} to device");
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~OpenedMtpDevice()
        {
            ReleaseUnmanagedResources();
        }

        public uint CreateFolder(string name, uint parentFolderId, uint parentStorageId)
        {
            var newFolderId =
                LibMtpLibrary.CreateFolder(_mptDeviceStructPointer, name, parentFolderId, parentStorageId);
            if (newFolderId == 0)
                throw new FolderCreationException(name, parentFolderId, parentStorageId);
            return newFolderId;
        }

        public void DeleteObject(uint objectId)
        {
            if (0 != LibMtpLibrary.DeleteObject(_mptDeviceStructPointer, objectId))
                throw new ApplicationException($"Failed to delete the object with it {objectId}");
        }
    }
}