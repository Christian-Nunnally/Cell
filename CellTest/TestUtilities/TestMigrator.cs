using Cell.Core.Persistence;
using Cell.Core.Persistence.Migration;

namespace CellTest.TestUtilities
{
    public class TestMigrator : IMigrator
    {
        public string FromVersion => "0";

        public string ToVersion => "1";

        public bool Migrated { get; private set; } = false;

        public bool Migrate(PersistedDirectory persistedDirectory)
        {
            Migrated = true;
            return Migrated;
        }
    }
}
