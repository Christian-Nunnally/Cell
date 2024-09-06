using Cell.Data;

namespace Cell.Persistence
{
    public class BackupManager
    {
        private static readonly TimeSpan MinimumBackupInterval = TimeSpan.FromMinutes(1);
        private static DateTime _lastBackupDate = DateTime.Now - MinimumBackupInterval;
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

        public void CreateBackup()
        {
            if (_lastBackupDate.Add(MinimumBackupInterval) > DateTime.Now) return;
            var oldSaveLocation = _persistenceManager.RootPath;
            _persistenceManager.RootPath = oldSaveLocation + "_backup_" + CreateFileFriendlyCurrentDateTime();
            SaveAll();
            _persistenceManager.ZipFolder();
            _lastBackupDate = DateTime.Now;
            _persistenceManager.RootPath = oldSaveLocation;
        }

        public void SaveAll()
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
