﻿using Cell.Persistence;
using Cell.Persistence.Migration;

namespace CellTest.TestUtilities
{
    public class TestMigrator : IMigrator
    {
        public string FromVersion => "0";

        public string ToVersion => "1";

        public bool Migrated { get; private set; } = false;

        public bool Migrate(PersistedDirectory persistenceManager)
        {
            Migrated = true;
            return Migrated;
        }
    }
}
