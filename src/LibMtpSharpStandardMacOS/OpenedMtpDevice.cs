using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using LibMtpSharpStandardMacOS.Enums;
using LibMtpSharpStandardMacOS.Exceptions;
using LibMtpSharpStandardMacOS.Lists;
using LibMtpSharpStandardMacOS.NativeAPI;
using LibMtpSharpStandardMacOS.Structs;
using Optional;

namespace LibMtpSharpStandardMacOS
{
    public class OpenedMtpDevice : IDisposable
    {
        private const int KindleVendorId = 0x1949;
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

        public bool IsKindle() => _vendorId == KindleVendorId;

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

        public IEnumerable<FileStruct> EnumerateFiles(uint storageId,
            uint parentId = LibMtpLibrary.LibmtpFilesAndFoldersRoot) =>
            GetFolderContent(storageId, parentId).Where(x => x.Filetype != FileTypeEnum.Folder);

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

        public Option<FileStruct> GetDirectory(uint storageId, string directoryName) =>
            GetMatchingItem(storageId, x =>
                string.Compare(x.FileName, directoryName, StringComparison.InvariantCultureIgnoreCase) == 0
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

        public void CopyFileToDevice(string filePath, string destFileName, uint storageId, uint parentId = LibMtpLibrary.LibmtpFilesAndFoldersRoot)
        {
            var fileInfo = new FileInfo(filePath);
            var fileStruct = new FileStruct
            {
                FileName = destFileName,
                FileSize = (ulong)fileInfo.Length,
                Filetype = FileTypeEnum.Unknown,
                ParentId = parentId,
                StorageId = storageId
            };

            var result = LibMtpLibrary.SendFile(_mptDeviceStructPointer, filePath, ref fileStruct, null, IntPtr.Zero);
            if (result != 0)
                throw new CopyFileToDeviceException($"Cannot copy {fileInfo.Name} to device");
        }

        public void CopyFileFromDevice(uint fileId, string destinationPath)
        {
            if (string.IsNullOrEmpty(destinationPath))
                throw new ArgumentException("Value cannot be null or empty.", nameof(destinationPath));

            if (LibMtpLibrary.GetFileToFile(_mptDeviceStructPointer, fileId, destinationPath, null) != 0)
                throw new CopyFileFromDeviceException(fileId, destinationPath);
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

        public uint CreateFolder(string name, uint parentStorageId, uint parentFolderId = LibMtpLibrary.LibmtpFilesAndFoldersRoot)
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