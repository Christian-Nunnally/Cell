using Cell.Data;
using System.IO;

namespace Cell.Persistence
{
    public class BackupManager
    {
        private readonly SheetTracker _sheetTracker;
        private readonly UserCollectionLoader _userCollectionLoader;
        private readonly PluginFunctionLoader _pluginFunctionLoader;
        private readonly CellTracker _cellTracker;
        private readonly PersistenceManager _persistenceManager;

        public BackupManager(PersistenceManager persistenceManager, CellTracker cellTracker, SheetTracker sheetTracker, UserCollectionLoader userCollectionLoader, PluginFunctionLoader pluginFunctionLoader)
        {
            _sheetTracker = sheetTracker;
            _userCollectionLoader = userCollectionLoader;
            _pluginFunctionLoader = pluginFunctionLoader;
            _cellTracker = cellTracker;
            _persistenceManager = persistenceManager;
        }

        public void CreateBackup(string backupName = "backup")
        {
            var oldSaveLocation = _persistenceManager.RootPath;
            var backupPath = Path.Combine("CellBackups", $"{oldSaveLocation}_{backupName}_{CreateFileFriendlyCurrentDateTime()}");
            _persistenceManager.RootPath = backupPath;
            SaveAll();
            _persistenceManager.ZipFolder();
            _persistenceManager.RootPath = oldSaveLocation;
        }

        private void SaveAll()
        {
            _pluginFunctionLoader.SavePlugins();
            _userCollectionLoader.SaveCollections();
            foreach (var sheet in _sheetTracker.Sheets) _cellTracker.SaveSheet(sheet.Name);
            _persistenceManager.SaveVersion();
        }

        private static string CreateFileFriendlyCurrentDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        }
    }
}
