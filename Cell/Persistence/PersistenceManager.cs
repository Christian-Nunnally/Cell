using Cell.Common;
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
        public static string CurrentRootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LGF", "Cell");
        private static DateTime _lastBackupDate = DateTime.Now - MinimumBackupInterval;
        private readonly IFileIO _fileIO;
        private string _rootPath;

        public static string CurrentTemplatePath => Path.Combine(CurrentRootPath, "Templates");
        public static string CurrentFunctionsPath => Path.Combine(CurrentRootPath, "Functions");
        public static string CurrentCollectionsPath => Path.Combine(CurrentRootPath, "Collections");
        public static string CurrentApplicationSettingsPath => Path.Combine(CurrentRootPath, "Application");
        public static string CurrentSheetsPath => Path.Combine(CurrentRootPath, "Sheets");

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

        public static void CreateBackup()
        {
            // Make sure cells instance is created with the correct save location
            var _ = CellTracker.Instance;
            if (_lastBackupDate.Add(MinimumBackupInterval) > DateTime.Now) return;
            var oldSaveLocation = CurrentRootPath;
            CurrentRootPath = CurrentRootPath + "_backup_" + CreateFileFriendlyCurrentDateTime();
            SaveAll();
            ZipFolder(CurrentRootPath);
            _lastBackupDate = DateTime.Now;
            CurrentRootPath = oldSaveLocation;
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

        public static void LoadAll()
        {
            var versionSchema = LoadVersion();
            if (Version != versionSchema) throw new CellError($"Error: The project you are trying to load need to be migrated from version {versionSchema} to version {Version}.");
            if (!Directory.Exists(CurrentRootPath)) Directory.CreateDirectory(CurrentRootPath);
            SaveVersion();
            UserCollectionLoader.LoadCollections();
            PluginFunctionLoader.LoadPlugins();
            UserCollectionLoader.LinkUpBaseCollectionsAfterLoad();
            ApplicationViewModel.Instance.CellLoader.LoadAndAddCells();
            CreateBackup();
        }

        public static void SaveAll()
        {
            PluginFunctionLoader.SavePlugins();
            UserCollectionLoader.SaveCollections();
            ApplicationViewModel.Instance.CellLoader.SaveCells();
            SaveVersion();
        }

        internal static IEnumerable<string> GetTemplateNames()
        {
            if (!Directory.Exists(CurrentTemplatePath)) return new List<string>();
            return Directory.GetDirectories(CurrentTemplatePath).Select(Path.GetFileName);
        }

        private static string CreateFileFriendlyCurrentDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        }

        private static string LoadVersion()
        {
            var versionPath = Path.Combine(CurrentRootPath, "version");
            if (!File.Exists(versionPath)) return Version;
            return File.ReadAllText(versionPath);
        }

        private static void SaveVersion()
        {
            var versionPath = Path.Combine(CurrentRootPath, "version");
            if (!Directory.Exists(CurrentRootPath)) Directory.CreateDirectory(CurrentRootPath);
            File.WriteAllText(versionPath, Version);
        }

        private static void ZipFolder(string folderPath)
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
            Directory.Move(oldFullPath, newFullPath);
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
    }
}
