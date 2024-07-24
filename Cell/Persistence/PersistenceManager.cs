
using Cell.Exceptions;
using System.IO;
using System.IO.Compression;

namespace Cell.Persistence
{
    internal class PersistenceManager
    {
        public const string Version = "0.0.0";
        public static string SaveLocation = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\LGF\\Cell";
        private static DateTime _lastBackupDate = DateTime.Now + MinimumBackupInterval;
        private static readonly TimeSpan MinimumBackupInterval = TimeSpan.FromMinutes(1);

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
            File.WriteAllText(versionPath, Version);
        }

        public static void LoadAll()
        {
            var versionSchema = LoadVersion();
            if (Version != versionSchema) throw new ProjectLoadException($"Error: The project you are trying to load need to be migrated from version {versionSchema} to version {Version}.");
            SaveVersion();
            UserCollectionLoader.LoadCollections();
            PluginFunctionLoader.LoadPlugins();
            new CellLoader(SaveLocation).LoadCells();
        }

        private static string LoadVersion()
        {
            var versionPath = Path.Combine(SaveLocation, "version");
            if (!File.Exists(versionPath)) return Version;
            return File.ReadAllText(versionPath);}

        public static void CreateBackup()
        {
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
