using Cell.Model;
using Cell.Persistence;
using Cell.Persistence.Migration;
using CellTest.TestUtilities;
using System.Text.Json;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace CellTest
{
    public class V1ToV2MigrationTests
    {
        private TestFileIO _testFileIO;
        private PersistenceManager _persistenceManager;

        private V1ToV2Migrator CreateTestInstance()
        {
            _testFileIO = new TestFileIO();
            _persistenceManager = new PersistenceManager("", _testFileIO);
            _persistenceManager.RegisterMigrator("0", "1", new V1ToV2Migrator());
            return new V1ToV2Migrator();
        }

        [Fact]
        public void OneOldCellSaved_MigrationRun_DeserializesIntoNewCellModelAndBackgroundColorIsSaved()
        {
            var migrator = CreateTestInstance();
            var oldModel = new OldCellModel();
            var serialized = JsonSerializer.Serialize(oldModel);
            _testFileIO.WriteFile("Sheets\\Sheet1\\1", serialized);

            migrator.Migrate(_persistenceManager);

            var migratedSerialized = _testFileIO.ReadFile("Sheets\\Sheet1\\1");
            var newModel = JsonSerializer.Deserialize<CellModel>(migratedSerialized);
            Assert.Equal(oldModel.ColorHexes[0], newModel!.Style.BackgroundColor);
        }
    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.