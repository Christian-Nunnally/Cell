﻿using Cell.Common;
using System.IO;

namespace Cell.Persistence
{
    /// <summary>
    /// Wraps a single root directory and provides methods for interacting with the contents of that directory.
    /// </summary>
    public class PersistedDirectory
    {
        private readonly IFileIO _fileIO;
        private readonly string _rootPath;
        public PersistedDirectory(string rootPath, IFileIO fileIO)
        {
            _rootPath = rootPath;
            _fileIO = fileIO;
        }

        public bool IsReadOnly { get; internal set; } = false;

        public void CopyDirectory(string from, string to)
        {
            CheckIsReadOnly();
            var fullFrom = Path.Combine(_rootPath, from);
            var fullTo = Path.Combine(_rootPath, to);
            _fileIO.CopyDirectory(fullFrom, fullTo);
        }

        private void CheckIsReadOnly()
        {
            if (IsReadOnly) throw new CellError($"{nameof(PersistedDirectory)} with {_rootPath} is in read read-only mode and can not be written to");
        }

        public void CopyTo(PersistedDirectory to, string fromPath = "", string toPath = "")
        {
            var fullFrom = Path.Combine(_rootPath, fromPath);
            var fullTo = to.GetFullPath(toPath);
            _fileIO.CopyDirectory(fullFrom, fullTo);
        }

        public void DeleteDirectory(string path = "")
        {
            CheckIsReadOnly();
            var fullPath = GetFullPath(path);
            _fileIO.DeleteDirectory(fullPath);
        }

        public void DeleteFile(string path)
        {
            CheckIsReadOnly();
            var fullPath = GetFullPath(path);
            _fileIO.DeleteFile(fullPath);
        }

        public bool DirectoryExists(string path)
        {
            var fullPath = GetFullPath(path);
            return _fileIO.DirectoryExists(fullPath);
        }

        public IEnumerable<string> GetDirectories(string path)
        {
            if (!DirectoryExists(path)) return [];
            var fullPath = GetFullPath(path);
            var directories = _fileIO.GetDirectories(fullPath);
            var extraChar = string.IsNullOrEmpty(_rootPath) ? 0 : 1;
            return directories.Select(x => x[(_rootPath.Length + extraChar)..]);
        }

        public IEnumerable<string> GetFiles(string path)
        {
            var fullPath = GetFullPath(path);
            var fullPaths = _fileIO.GetFiles(fullPath);
            var extraChar = string.IsNullOrEmpty(_rootPath) ? 0 : 1;
            return fullPaths.Select(x => x[(_rootPath.Length + extraChar)..]);
        }

        public string GetFullPath(string additionalPath = "")
        {
            return Path.Combine(_rootPath, additionalPath);
        }

        public string? LoadFile(string path)
        {
            if (path.StartsWith("//") || path.StartsWith('\\')) throw new CellError("Invalid path");
            var fullPath = GetFullPath(path);
            if (!_fileIO.Exists(fullPath)) return null;
            return _fileIO.ReadFile(fullPath);
        }

        public void MoveDirectory(string from, string to)
        {
            CheckIsReadOnly();
            var fullFrom = Path.Combine(_rootPath, from);
            var fullTo = Path.Combine(_rootPath, to);
            _fileIO.MoveDirectory(fullFrom, fullTo);
        }

        public void SaveFile(string path, string content)
        {
            CheckIsReadOnly();
            var fullPath = GetFullPath(path);
            _fileIO.WriteFile(fullPath, content);
        }

        public void UnzipTo(PersistedDirectory to, string fromPathZip = "", string toPath = "")
        {
            var fullFrom = GetFullPath(fromPathZip);
            var fullTo = to.GetFullPath(toPath);
            var zipPath = fullTo + ".zip";
            _fileIO.ZipDirectory(fullFrom, zipPath);
        }

        public void ZipFolder(string path = "")
        {
            var fullPath = GetFullPath(path);
            var zipPath = fullPath + ".zip";
            _fileIO.ZipDirectory(fullPath, zipPath);
            _fileIO.DeleteDirectory(fullPath);
        }

        public void ZipTo(PersistedDirectory to, string fromPath = "", string toPathZip = "")
        {
            var fullFrom = GetFullPath(fromPath);
            var fullTo = to.GetFullPath(toPathZip);
            var zipPath = fullTo + ".zip";
            _fileIO.ZipDirectory(fullFrom, zipPath);
        }
    }
}
