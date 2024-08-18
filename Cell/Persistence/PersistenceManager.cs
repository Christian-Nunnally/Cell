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
        public static string SaveLocation = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\LGF\\Cell";
        private static DateTime _lastBackupDate = DateTime.Now - MinimumBackupInterval;
        public static void CreateBackup()
        {
            // Make sure cells instance is created with the correct save location
            var _ = CellTracker.Instance;
            if (_lastBackupDate.Add(MinimumBackupInterval) > DateTime.Now) return;
            var oldSaveLocation = SaveLocation;
            SaveLocation = SaveLocation + "_backup_" + CreateFileFriendlyCurrentDateTime();
            SaveAll();
            ZipFolder(SaveLocation);
            _lastBackupDate = DateTime.Now;
            SaveLocation = oldSaveLocation;
        }

        public static void ExportSheet(string sheetName)
        {
            new CellLoader(SaveLocation).ExportSheetTemplate(sheetName);
        }

        public static void ImportSheet(string templateName, string sheetName)
        {
            new CellLoader(SaveLocation).ImportSheetTemplate(templateName, sheetName);
        }

        public static void LoadAll()
        {
            var versionSchema = LoadVersion();
            if (Version != versionSchema) throw new CellError($"Error: The project you are trying to load need to be migrated from version {versionSchema} to version {Version}.");
            if (!Directory.Exists(SaveLocation)) Directory.CreateDirectory(SaveLocation);
            SaveVersion();
            UserCollectionLoader.LoadCollections();
            PluginFunctionLoader.LoadPlugins();
            UserCollectionLoader.LinkUpBaseCollectionsAfterLoad();
            new CellLoader(SaveLocation).LoadAndAddCells();
            CreateBackup();
        }

        public static void SaveAll()
        {
            PluginFunctionLoader.SavePlugins();
            UserCollectionLoader.SaveCollections();
            new CellLoader(SaveLocation).SaveCells();
            SaveVersion();
        }

        private static string CreateFileFriendlyCurrentDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        }

        private static string LoadVersion()
        {
            var versionPath = Path.Combine(SaveLocation, "version");
            if (!File.Exists(versionPath)) return Version;
            return File.ReadAllText(versionPath);
        }

        private static void SaveVersion()
        {
            var versionPath = Path.Combine(SaveLocation, "version");
            if (!Directory.Exists(SaveLocation)) Directory.CreateDirectory(SaveLocation);
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
