
using Cell.Data;
using Cell.Exceptions;
using System.IO;
using System.IO.Compression;

namespace Cell.Persistence
{
    internal class PersistenceManager
    {
        public const string Version = "0.0.0";
        public static string SaveLocation = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\LGF\\Cell";
        private static readonly TimeSpan MinimumBackupInterval = TimeSpan.FromMinutes(1);
        private static DateTime _lastBackupDate = DateTime.Now - MinimumBackupInterval;

        public static void SaveAll()
        {
            PluginFunctionLoader.SavePlugins();
            UserCollectionLoader.SaveCollections();
            new CellLoader(SaveLocation).SaveCells();
            SaveVersion();
        }

        private static void SaveVersion()
        {
            var versionPath = Path.Combine(SaveLocation, "version");
            if (!Directory.Exists(SaveLocation)) Directory.CreateDirectory(SaveLocation);
            File.WriteAllText(versionPath, Version);
        }

        public static void LoadAll()
        {
            var versionSchema = LoadVersion();
            if (Version != versionSchema) throw new ProjectLoadException($"Error: The project you are trying to load need to be migrated from version {versionSchema} to version {Version}.");
            if (!Directory.Exists(SaveLocation)) Directory.CreateDirectory(SaveLocation);
            SaveVersion();
            UserCollectionLoader.LoadCollections();
            PluginFunctionLoader.LoadPlugins();
            new CellLoader(SaveLocation).LoadCells();
            CreateBackup();
        }

        private static string LoadVersion()
        {
            var versionPath = Path.Combine(SaveLocation, "version");
            if (!File.Exists(versionPath)) return Version;
            return File.ReadAllText(versionPath);}

        public static void CreateBackup()
        {
            // Make sure cells instance is created with the correct save location
            var _ = Cells.Instance;
            if (_lastBackupDate.Add(MinimumBackupInterval) > DateTime.Now) return;
            var oldSaveLocation = SaveLocation;
            SaveLocation = SaveLocation + "_backup_" + CreateFileFriendlyCurrentDateTime();
            SaveAll();
            ZipFolder(SaveLocation);
            _lastBackupDate = DateTime.Now;
            SaveLocation = oldSaveLocation;
        }

        private static string CreateFileFriendlyCurrentDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        }

        private static void ZipFolder(string folderPath)
        {
            var zipPath = folderPath + ".zip";
            ZipFile.CreateFromDirectory(folderPath, zipPath);
            Directory.Delete(folderPath, true);
        }
    }
}
