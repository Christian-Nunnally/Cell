using Cell.Data;
using Cell.ViewModel.Application;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace Cell.Persistence
{
    public class PersistenceManager
    {
        public const string Version = "0.0.0";
        private static readonly TimeSpan MinimumBackupInterval = TimeSpan.FromMinutes(1);
        private static DateTime _lastBackupDate = DateTime.Now - MinimumBackupInterval;
        private readonly IFileIO _fileIO;
        private string _rootPath;

        public string CurrentTemplatePath => Path.Combine(_rootPath, "Templates");

        public PersistenceManager(string rootPath, IFileIO fileIO)
        {
            _rootPath = rootPath;
            _fileIO = fileIO;
        }

        public void DeleteFile(string path)
        {
            var file = Path.Combine(_rootPath, path);
            if (!File.Exists(file)) return;
            _fileIO.DeleteFile(path);
        }

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

        public static void ExportSheet(string sheetName)
        {
            ApplicationViewModel.Instance.CellLoader.ExportSheetTemplate(sheetName);
        }

        public static void ImportSheet(string templateName, string sheetName)
        {
            ApplicationViewModel.Instance.CellLoader.ImportSheetTemplate(templateName, sheetName);
        }

        public static void CopySheet(string sheetName)
        {
            ApplicationViewModel.Instance.CellLoader.CopySheet(sheetName);
        }

        public void SaveAll()
        {
            PluginFunctionLoader.SavePlugins();
            ApplicationViewModel.Instance.UserCollectionLoader.SaveCollections();
            ApplicationViewModel.Instance.CellLoader.SaveCells();
            SaveVersion();
        }

        internal IEnumerable<string> GetTemplateNames()
        {
            if (!Directory.Exists(CurrentTemplatePath)) return [];
            return Directory.GetDirectories(CurrentTemplatePath).Select(Path.GetFileName).OfType<string>();
        }

        private static string CreateFileFriendlyCurrentDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        }

        public string LoadVersion()
        {
            var versionPath = Path.Combine(_rootPath, "version");
            if (!_fileIO.Exists(versionPath)) return Version;
            return _fileIO.ReadFile(versionPath);
        }

        public void SaveVersion()
        {
            var versionPath = Path.Combine(_rootPath, "version");
            _fileIO.WriteFile(versionPath, Version);
        }

        private void ZipFolder(string folderPath)
        {
            var zipPath = folderPath + ".zip";
            ZipFile.CreateFromDirectory(folderPath, zipPath);
            Directory.Delete(folderPath, true);
        }

        public void OpenRootDirectoryInExplorer()
        {
            Process.Start("explorer.exe", _rootPath);
        }

        internal void SaveFile(string path, string serialized)
        {
            var fullPath = Path.Combine(_rootPath, path);
            var directory = Path.GetDirectoryName(fullPath);
            if (directory == null) return;
            Directory.CreateDirectory(directory);
            File.WriteAllText(fullPath, serialized);
        }

        public void MoveDirectory(string oldPath, string newPath)
        {
            var oldFullPath = Path.Combine(_rootPath, oldPath);
            var newFullPath = Path.Combine(_rootPath, newPath);
            _fileIO.MoveDirectory(oldFullPath, newFullPath);
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(Path.Combine(_rootPath, path));
        }

        public string[] GetDirectories(string path)
        {
            if (!DirectoryExists(path)) return [];
            return Directory.GetDirectories(Path.Combine(_rootPath, path));
        }

        internal bool FileExists(string path)
        {
            return File.Exists(Path.Combine(_rootPath, path));
        }

        internal string? LoadFile(string path)
        {
            path = Path.Combine(_rootPath, path);
            if (!File.Exists(path)) return null;
            return File.ReadAllText(path);
        }

        internal void DeleteDirectory(string path)
        {
            path = Path.Combine(_rootPath, path);
            _fileIO.DeleteDirectory(path);
        }

        internal void CopyDirectory(string fromDirectory, string toDirectory)
        {
            var oldFullPath = Path.Combine(_rootPath, fromDirectory);
            var newFullPath = Path.Combine(_rootPath, toDirectory);
            _fileIO.CopyDirectory(oldFullPath, newFullPath);
        }
    }
}
