namespace Cell.Persistence.Migration
{
    /// <summary>
    /// Interface for a migrator that can migrate a directory from one version to another.
    /// </summary>
    public interface IMigrator
    {
        /// <summary>
        /// Gets the key use when looking to migrate from one version to another.
        /// </summary>
        /// <param name="fromVersion">The version being migrated from.</param>
        /// <param name="toVersion">The version being migrated to.</param>
        /// <returns></returns>
        static string GetMigratorKey(string fromVersion, string toVersion) => $"{fromVersion}_{toVersion}";

        /// <summary>
        /// Migrates the directory to the version this migrator is responsible for.
        /// </summary>
        /// <param name="persistedDirectory">The directory being migrated.</param>
        /// <returns>True if the migration was succesful.</returns>
        bool Migrate(PersistedDirectory persistedDirectory);
    }
}
