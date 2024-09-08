using Cell.Data;
using Cell.Execution;
using Cell.Model;
using Cell.Persistence;
using Cell.Persistence.Migration;
using Cell.ViewModel.Application;
using CellTest.TestUtilities;
using System.Text.Json;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace CellTest
{
    public class V1ToV2MigrationTests
    {
        private TestFileIO _testFileIO;
        private PersistenceManager _persistenceManager;
        private UserCollectionLoader _userCollectionLoader;
        private CellPopulateManager _cellPopulateManager;
        private CellLoader _cellLoader;
        private CellTriggerManager _cellTriggerManager;
        private PluginFunctionLoader _pluginFunctionLoader;
        private CellTracker _cellTracker;

        private V1ToV2Migrator CreateTestInstance()
        {
            _testFileIO = new TestFileIO();
            _persistenceManager = new PersistenceManager("", _testFileIO);
            _pluginFunctionLoader = new PluginFunctionLoader(_persistenceManager);
            _cellLoader = new CellLoader(_persistenceManager);
            _cellTracker = new CellTracker(_cellLoader);
            _userCollectionLoader = new UserCollectionLoader(_persistenceManager, _pluginFunctionLoader, _cellTracker);
            _cellPopulateManager = new CellPopulateManager(_cellTracker, _pluginFunctionLoader, _userCollectionLoader);
            _cellTriggerManager = new CellTriggerManager(_cellTracker, _pluginFunctionLoader, _userCollectionLoader);
            return new V1ToV2Migrator();
        }

        [Fact]
        public void CurrentlyOnV0_DesiredVersionIsV1_GetsInvoked()
        {
            var migrator = CreateTestInstance();
            var loadProgress = ApplicationViewModel.Instance.LoadWithProgress();
            while (!loadProgress.IsComplete)
            {
                loadProgress.Continue();
            }
        }

        [Fact]
        public void BasicLaunchTest()
        {
            var oldModel = new OldCellModel();
            var serialized = JsonSerializer.Serialize(oldModel);
            var migrator = CreateTestInstance();
            _testFileIO.WriteFile("Sheets/Sheet1/1", serialized);

            migrator.Migrate();

            var migratedSerialized = _testFileIO.ReadFile("Sheets/Sheet1/1");
            var newModel = JsonSerializer.Deserialize<CellModel>(migratedSerialized);
            Assert.Equal(oldModel.ColorHexes[0], newModel.ColorHexes[0]);
        }
    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.