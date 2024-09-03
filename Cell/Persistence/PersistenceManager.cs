using Cell.Common;
using Cell.ViewModel.Application;
using System.Diagnostics;
using System.IO;

namespace Cell.Persistence
{
    public class PersistenceManager
    {
        public const string Version = "0.0.0";
        private static readonly TimeSpan MinimumBackupInterval = TimeSpan.FromMinutes(1);
        private readonly IFileIO _fileIO;
        private static DateTime _lastBackupDate = DateTime.Now - MinimumBackupInterval;
        private string _rootPath;
        public PersistenceManager(string rootPath, IFileIO fileIO)
        {
            _rootPath = rootPath;
            _fileIO = fileIO;
        }

        public string CurrentTemplatePath => Path.Combine(_rootPath, "Templates");

        // TODO: Move somewhere else
        public void CreateBackup()
        {
            // Make sure cells instance is created with the correct save location
            var _ = ApplicationViewModel.Instance.CellTracker;
            if (_lastBackupDate.Add(MinimumBackupInterval) > DateTime.Now) return;
            var oldSaveLocation = _rootPath;
            _rootPath = _rootPath + "_backup_" + CreateFileFriendlyCurrentDateTime();
            SaveAll();
            ZipFolder(_rootPath);
            _lastBackupDate = DateTime.Now;
            _rootPath = oldSaveLocation;
        }

        public void DeleteFile(string path)
        {
            var fullPath = Path.Combine(_rootPath, path);
            _fileIO.DeleteFile(fullPath);
        }

        public bool DirectoryExists(string path)
        {
            var fullPath = Path.Combine(_rootPath, path);
            return _fileIO.DirectoryExists(fullPath);
        }

        public IEnumerable<string> GetDirectories(string path)
        {
            if (!DirectoryExists(path)) return [];
            var fullPath = Path.Combine(_rootPath, path);
            var directories = _fileIO.GetDirectories(fullPath);
            return directories.Select(x => x[(_rootPath.Length + 1)..]);
        }

        public string LoadVersion()
        {
            var versionPath = Path.Combine(_rootPath, "version");
            if (!_fileIO.Exists(versionPath)) return Version;
            return _fileIO.ReadFile(versionPath);
        }

        public void MoveDirectory(string from, string to)
        {
            var fullFrom = Path.Combine(_rootPath, from);
            var fullTo = Path.Combine(_rootPath, to);
            _fileIO.MoveDirectory(fullFrom, fullTo);
        }

        public void OpenRootDirectoryInExplorer()
        {
            Process.Start("explorer.exe", _rootPath);
        }

        // TODO: Move somewhere else
        public void SaveAll()
        {
            ApplicationViewModel.Instance.PluginFunctionLoader.SavePlugins();
            ApplicationViewModel.Instance.UserCollectionLoader.SaveCollections();
            ApplicationViewModel.Instance.CellLoader.SaveCells();
            SaveVersion();
        }

        public void SaveVersion()
        {
            var versionPath = Path.Combine(_rootPath, "version");
            _fileIO.WriteFile(versionPath, Version);
        }

        internal void CopyDirectory(string from, string to)
        {
            var fullFrom = Path.Combine(_rootPath, from);
            var fullTo = Path.Combine(_rootPath, to);
            _fileIO.CopyDirectory(fullFrom, fullTo);
        }

        internal void DeleteDirectory(string path)
        {
            var fullPath = Path.Combine(_rootPath, path);
            _fileIO.DeleteDirectory(fullPath);
        }

        internal IEnumerable<string> GetFiles(string path)
        {
            var fullPath = Path.Combine(_rootPath, path);
            var fullPaths = _fileIO.GetFiles(fullPath);
            return fullPaths.Select(x => x[(_rootPath.Length + 1)..]);
        }

        internal IEnumerable<string> GetTemplateNames()
        {
            if (!DirectoryExists(CurrentTemplatePath)) return [];
            return GetDirectories(CurrentTemplatePath).Select(Path.GetFileName).OfType<string>();
        }

        internal string? LoadFile(string path)
        {
            if (path.StartsWith("//") || path.StartsWith('\\')) throw new CellError("Invalid path");
            var fullPath = Path.Combine(_rootPath, path);
            if (!_fileIO.Exists(fullPath)) return null;
            return _fileIO.ReadFile(fullPath);
        }

        internal void SaveFile(string path, string content)
        {
            var fullPath = Path.Combine(_rootPath, path);
            _fileIO.WriteFile(fullPath, content);
        }

        private static string CreateFileFriendlyCurrentDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        }

        private void ZipFolder(string path)
        {
            var zipPath = path + ".zip";
            _fileIO.ZipDirectory(path, zipPath);
            _fileIO.DeleteDirectory(path);
        }
    }
}
