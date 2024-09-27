namespace Cell.Persistence.Migration
{
    public interface IMigrator
    {
        static string GetMigratorKey(string fromVersion, string toVersion) => $"{fromVersion}_{toVersion}";

        bool Migrate(PersistedDirectory persistenceManager);
    }
}
