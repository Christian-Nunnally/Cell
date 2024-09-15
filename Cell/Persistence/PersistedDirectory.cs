using Cell.Common;
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

        public void CopyDirectory(string from, string to)
        {
            var fullFrom = Path.Combine(_rootPath, from);
            var fullTo = Path.Combine(_rootPath, to);
            _fileIO.CopyDirectory(fullFrom, fullTo);
        }

        public void CopyTo(PersistedDirectory to, string path = "")
        {
            var fullFrom = Path.Combine(_rootPath, path);
            var fullTo = to.GetFullPath();
            _fileIO.CopyDirectory(fullFrom, fullTo);
        }

        public void ZipTo(PersistedDirectory to, string path = "")
        {
            var fullPath = GetFullPath();
            var fullTo = to.GetFullPath(path);
            var zipPath = fullTo + ".zip";
            _fileIO.ZipDirectory(fullPath, zipPath);
        }

        public void DeleteDirectory(string path)
        {
            var fullPath = GetFullPath(path);
            _fileIO.DeleteDirectory(fullPath);
        }

        public void DeleteFile(string path)
        {
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

        public string? LoadFile(string path)
        {
            if (path.StartsWith("//") || path.StartsWith('\\')) throw new CellError("Invalid path");
            var fullPath = GetFullPath(path);
            if (!_fileIO.Exists(fullPath)) return null;
            return _fileIO.ReadFile(fullPath);
        }

        public void MoveDirectory(string from, string to)
        {
            var fullFrom = Path.Combine(_rootPath, from);
            var fullTo = Path.Combine(_rootPath, to);
            _fileIO.MoveDirectory(fullFrom, fullTo);
        }

        public string GetFullPath(string additionalPath = "")
        {
            return Path.Combine(_rootPath, additionalPath);
        }

        public void SaveFile(string path, string content)
        {
            var fullPath = GetFullPath(path);
            _fileIO.WriteFile(fullPath, content);
        }

        public void ZipFolder(string path = "")
        {
            var fullPath = GetFullPath(path);
            var zipPath = fullPath + ".zip";
            _fileIO.ZipDirectory(fullPath, zipPath);
            _fileIO.DeleteDirectory(fullPath);
        }
    }
}
