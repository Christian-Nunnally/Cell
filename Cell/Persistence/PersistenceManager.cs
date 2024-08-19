using Cell.Common;
using Cell.Data;
using System.IO;
using System.IO.Compression;

namespace Cell.Persistence
{
    internal class PersistenceManager
    {
        public const string Version = "0.0.0";
        private static readonly TimeSpan MinimumBackupInterval = TimeSpan.FromMinutes(1);
        public static string CurrentRootPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\LGF\\Cell";
        private static DateTime _lastBackupDate = DateTime.Now - MinimumBackupInterval;

        public static string CurrentTemplatePath => Path.Combine(CurrentRootPath, "Templates");
        public static string CurrentFunctionsPath => Path.Combine(CurrentRootPath, "Functions");
        public static string CurrentCollectionsPath => Path.Combine(CurrentRootPath, "Collections");
        public static string CurrentApplicationSettingsPath => Path.Combine(CurrentRootPath, "Application");
        public static string CurrentSheetsPath => Path.Combine(CurrentRootPath, "Sheets");

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
            new CellLoader(CurrentRootPath).ExportSheetTemplate(sheetName);
        }

        public static void ImportSheet(string templateName, string sheetName)
        {
            new CellLoader(CurrentRootPath).ImportSheetTemplate(templateName, sheetName);
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
            new CellLoader(CurrentRootPath).LoadAndAddCells();
            CreateBackup();
        }

        public static void SaveAll()
        {
            PluginFunctionLoader.SavePlugins();
            UserCollectionLoader.SaveCollections();
            new CellLoader(CurrentRootPath).SaveCells();
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
    }
}
