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
        /// <summary>
        /// Creates a new instance of <see cref="PersistedDirectory"/>.
        /// </summary>
        /// <param name="rootPath">The root path of this directory in the file system it resides in.</param>
        /// <param name="fileIO">The interface used for interacting with the file system.</param>
        public PersistedDirectory(string rootPath, IFileIO fileIO)
        {
            _rootPath = rootPath;
            _fileIO = fileIO;
        }

        /// <summary>
        /// Gets or sets whether this directory can be written to from this <see cref="PersistedDirectory"/>.
        /// </summary>
        public bool IsReadOnly { get; internal set; } = false;

        /// <summary>
        /// Copies a directory from one location to another within this <see cref="PersistedDirectory"/>.
        /// </summary>
        /// <param name="from">The path of the directory to copy within this <see cref="PersistedDirectory"/>.</param>
        /// <param name="to">The destination path within this <see cref="PersistedDirectory"/>.</param>
        public void CopyDirectory(string from, string to)
        {
            CheckIsReadOnly();
            var fullFrom = Path.Combine(_rootPath, from);
            var fullTo = Path.Combine(_rootPath, to);
            _fileIO.CopyDirectory(fullFrom, fullTo);
        }

        /// <summary>
        /// Copies the directory to the given path in a different <see cref="PersistedDirectory"/>.
        /// </summary>
        /// <param name="to">The different root directory to copy in to.</param>
        /// <param name="fromPath">The path in this directory to copy.</param>
        /// <param name="toPath">The path in the to directory to paste into.</param>
        public void CopyTo(PersistedDirectory to, string fromPath = "", string toPath = "")
        {
            var fullFrom = Path.Combine(_rootPath, fromPath);
            var fullTo = to.GetFullPath(toPath);
            _fileIO.CopyDirectory(fullFrom, fullTo);
        }

        /// <summary>
        /// Deletes a directory from this <see cref="PersistedDirectory"/>.
        /// </summary>
        /// <param name="path">The directory path to delete.</param>
        public void DeleteDirectory(string path = "")
        {
            CheckIsReadOnly();
            var fullPath = GetFullPath(path);
            _fileIO.DeleteDirectory(fullPath);
        }

        /// <summary>
        /// Deletes a file from this <see cref="PersistedDirectory"/>.
        /// </summary>
        /// <param name="path">The file path to delete.</param>
        public void DeleteFile(string path)
        {
            CheckIsReadOnly();
            var fullPath = GetFullPath(path);
            _fileIO.DeleteFile(fullPath);
        }

        /// <summary>
        /// Determines if a directory exists at the given path.
        /// </summary>
        /// <param name="path">The path to test whether a directory exists there.</param>
        /// <returns>True if the directory exists.</returns>
        public bool DirectoryExists(string path)
        {
            var fullPath = GetFullPath(path);
            return _fileIO.DirectoryExists(fullPath);
        }

        /// <summary>
        /// Gets the list of directories in the given path.
        /// </summary>
        /// <param name="path">The path to get the directories inside of.</param>
        /// <returns>A list of paths to directories.</returns>
        public IEnumerable<string> GetDirectories(string path)
        {
            var fullPath = GetFullPath(path);
            if (!DirectoryExists(fullPath)) return [];
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

        /// <summary>
        /// Zip a directory in place with a .zip extension.
        /// </summary>
        /// <param name="path">The path to the directory to zip.</param>
        public void ZipFolder(string path = "")
        {
            var fullPath = GetFullPath(path);
            var zipPath = fullPath + ".zip";
            _fileIO.ZipDirectory(fullPath, zipPath);
            _fileIO.DeleteDirectory(fullPath);
        }

        /// <summary>
        /// Zip the directory to the given path in a different <see cref="PersistedDirectory"/>.
        /// </summary>
        /// <param name="to">The different root directory to zip in to.</param>
        /// <param name="fromPath">The path in this directory to zip from.</param>
        /// <param name="toPathZip">The path in the to directory to put the zip. A .zip will be appended automatically.</param>
        public void ZipTo(PersistedDirectory to, string fromPath = "", string toPathZip = "")
        {
            var fullFrom = GetFullPath(fromPath);
            var fullTo = to.GetFullPath(toPathZip);
            var zipPath = fullTo + ".zip";
            _fileIO.ZipDirectory(fullFrom, zipPath);
        }

        private void CheckIsReadOnly()
        {
            if (IsReadOnly) throw new CellError($"{nameof(PersistedDirectory)} with {_rootPath} is in read read-only mode and can not be written to");
        }
    }
}
