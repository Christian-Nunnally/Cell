
namespace Cell.Persistence.Migration
{
    public interface IMigrator
    {
        bool Migrate(PersistedDirectory persistenceManager);

        static string GetMigratorKey(string fromVersion, string toVersion) => $"{fromVersion}_{toVersion}";
    }
}
