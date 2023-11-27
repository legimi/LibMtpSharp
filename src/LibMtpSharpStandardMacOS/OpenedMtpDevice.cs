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
        private readonly MemoryStream _memoryStream = new MemoryStream();

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

        public Option<FileStruct> GetFile(uint storageId, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException(nameof(filePath));

            var pathParts = filePath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

            return FindFileRecursive(storageId, LibMtpLibrary.LibmtpFilesAndFoldersRoot, pathParts, 0);
        }

        private Option<FileStruct> FindFileRecursive(uint storageId, uint folderId, IReadOnlyList<string> pathParts, int pathIndex)
        {
            var filesAndFolders = GetFolderContent(storageId, folderId);

            foreach (var item in filesAndFolders)
            {
                if (item.Filetype == FileTypeEnum.Folder)
                {
                    if (item.FileName == pathParts[pathIndex] && pathIndex < pathParts.Count - 1)
                    {
                        var result = FindFileRecursive(storageId, item.ItemId, pathParts, pathIndex + 1);
                        if (result.HasValue)
                            return result;
                    }
                }
                else
                {
                    if (item.FileName == pathParts[pathIndex] && pathIndex == pathParts.Count - 1)
                        return item.Some();
                }
            }

            return Option.None<FileStruct>();
        }

        public Option<FileStruct> GetDirectory(uint storageId, string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new ArgumentException(nameof(directoryPath));

            var pathParts = directoryPath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

            return FindFolderRecursive(storageId, LibMtpLibrary.LibmtpFilesAndFoldersRoot, pathParts, 0);
        }

        private Option<FileStruct> FindFolderRecursive(uint storageId, uint folderId, string[] pathParts, int pathIndex)
        {
            var filesAndFolders = GetFolderContent(storageId, folderId);

            foreach (var item in filesAndFolders)
            {
                if (item.Filetype != FileTypeEnum.Folder)
                    continue;

                if (item.FileName == pathParts[pathIndex])
                {
                    if (pathIndex == pathParts.Length - 1)
                        return item.Some();

                    var result = FindFolderRecursive(storageId, item.ItemId, pathParts, pathIndex + 1);
                    if (result.HasValue)
                        return result;
                }
            }

            return Option.None<FileStruct>();
        }

        public void CopyFileToDevice(string filePath, string destFileName, uint storageId, uint parentId = LibMtpLibrary.LibmtpFilesAndFoldersRoot)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException(nameof(filePath));
            if (string.IsNullOrWhiteSpace(destFileName))
                throw new ArgumentException(nameof(destFileName));

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
            if (string.IsNullOrWhiteSpace(destinationPath))
                throw new ArgumentException(nameof(destinationPath));

            var result = LibMtpLibrary.GetFileToFile(_mptDeviceStructPointer, fileId, destinationPath, null);
            if (result != 0)
            {
                LibMtpLibrary.DumpErrorStack(_mptDeviceStructPointer);
                throw new CopyFileFromDeviceException(fileId, destinationPath);
            }
        }

        public byte[] GetFileToByteArray(uint fileId)
        {
            LibMtpLibrary.GetFileToHandler(_mptDeviceStructPointer, fileId, PutFuncToGetFile, null);

            var data = _memoryStream.ToArray();
            _memoryStream.SetLength(0);
            _memoryStream.Position = 0;
            return data;
        }

        private ushort PutFuncToGetFile(IntPtr parameters, IntPtr priv, uint sendlen, IntPtr data, out uint putlen)
        {
            var buffer = new byte[sendlen];
            Marshal.Copy(data, buffer, 0, (int)sendlen);
            _memoryStream.Write(buffer, 0, (int)sendlen);

            putlen = sendlen;
            return 0;
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
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

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