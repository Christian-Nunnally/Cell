using Cell.Data;
using Cell.Execution;
using Cell.Persistence;
using Cell.ViewModel.Application;
using CellTest.TestUtilities;

namespace CellTest.ViewModel.Application

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
{
    public class ApplicationViewModelTests
    {
        private static TestFileIO _testFileIO;
        private PersistenceManager _persistenceManager;
        private UserCollectionLoader _userCollectionLoader;
        private CellPopulateManager _cellPopulateManager;
        private CellLoader _cellLoader;
        private CellTriggerManager _cellTriggerManager;
        private PluginFunctionLoader _pluginFunctionLoader;
        private CellTracker _cellTracker;
        private SheetTracker _sheetTracker;
        private TitleBarSheetNavigationViewModel _titleBarSheetNavigationViewModel;
        private ApplicationSettings _applicationSettings;
        private UndoRedoManager _undoRedoManager;
        private CellClipboard _cellClipboard;
        private BackupManager _backupManager;

        private ApplicationViewModel CreateTestInstance()
        {
            _testFileIO = new TestFileIO();
            _persistenceManager = new PersistenceManager("", _testFileIO);
            _pluginFunctionLoader = new PluginFunctionLoader(_persistenceManager);
            _cellLoader = new CellLoader(_persistenceManager);
            _cellTracker = new CellTracker(_cellLoader);
            _userCollectionLoader = new UserCollectionLoader(_persistenceManager, _pluginFunctionLoader, _cellTracker);
            _cellPopulateManager = new CellPopulateManager(_cellTracker, _pluginFunctionLoader, _userCollectionLoader);
            _cellTriggerManager = new CellTriggerManager(_cellTracker, _pluginFunctionLoader, _userCollectionLoader);
            _sheetTracker = new SheetTracker(_persistenceManager, _cellLoader, _cellTracker, _pluginFunctionLoader, _userCollectionLoader);
            _backupManager = new BackupManager(_persistenceManager, _cellTracker, _sheetTracker, _userCollectionLoader, _pluginFunctionLoader);
            return new ApplicationViewModel(_persistenceManager, _pluginFunctionLoader, _cellLoader, _cellTracker, _userCollectionLoader, _cellPopulateManager, _cellTriggerManager, _sheetTracker, _titleBarSheetNavigationViewModel, _applicationSettings, _undoRedoManager, _cellClipboard, _backupManager);
        }

        [Fact]
        public void BasicLaunchTest()
        {
            var _ = CreateTestInstance();
        }

        [Fact]
        public void UpgradeRequired_LoadStarted_FailsWithoutMigrator()
        {
            var testing = CreateTestInstance();
            _persistenceManager.Version = "0";
            Assert.NotEqual("1", _persistenceManager.Version);
            _persistenceManager.SaveVersion();
            _persistenceManager.Version = "1";

            var result = testing.Load();

            Assert.False(result.Success);
            Assert.Equal(ApplicationViewModel.NoMigratorForVersionError, result.Message);
        }

        [Fact]
        public void MigratorExists_LoadStarted_PromptsUserToMigrate()
        {
            var testing = CreateTestInstance();
            _persistenceManager.Version = "0";
            _persistenceManager.SaveVersion();
            _persistenceManager.Version = "1";
            var migrator = new TestMigrator();
            _persistenceManager.RegisterMigrator("0", "1", migrator);
            var dialog = new TestDialogWindow();

            testing.Load();

            Assert.True(dialog.WasShown);
        }

        [Fact]
        public void MigrationConfirmationDialogOpen_UserConfirms_MigratorInvoked()
        {
            var testing = CreateTestInstance();
            _persistenceManager.Version = "0";
            _persistenceManager.SaveVersion();
            _persistenceManager.Version = "1";
            var migrator = new TestMigrator();
            _persistenceManager.RegisterMigrator("0", "1", migrator);
            var _ = new TestDialogWindow(0);
            Assert.False(migrator.Migrated);

            testing.Load();

            Assert.True(migrator.Migrated);
        }
    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.