
using System.IO;
using System.IO.Compression;

namespace Cell.Persistence
{
    internal class PersistenceManager
    {
        public static string SaveLocation = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\LGF\\Cell";
        private static DateTime _lastBackupDate = DateTime.Now;
        private static readonly TimeSpan MinimumBackupInterval = TimeSpan.FromMinutes(1);

        public static void SaveAll()
        {
            PluginFunctionLoader.SavePlugins();
            UserCollectionLoader.SaveCollections();
            CellLoader.SaveCells();
        }

        public static void LoadAll()
        {
            UserCollectionLoader.LoadCollections();
            PluginFunctionLoader.LoadPlugins();
            CellLoader.LoadCells();
        }

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
