using Cell.Persistence.Migration;
using System.IO;

namespace Cell.Persistence
{
    public class PersistedProject
    {
        public const string TemplateDirectory = "Templates";
        private const string VersionFileName = "version";
        private readonly PersistedDirectory _projectDirectory;
        private readonly Dictionary<string, IMigrator> _registeredMigrators = [];
        public string Version = "1";
        public PersistedProject(PersistedDirectory projectDirectory)
        {
            _projectDirectory = projectDirectory;
        }

        public bool IsReadOnly { get => _projectDirectory.IsReadOnly; set => _projectDirectory.IsReadOnly = value; }

        public bool CanMigrate() => _registeredMigrators.ContainsKey(IMigrator.GetMigratorKey(LoadVersion(), Version));

        public IEnumerable<string> GetTemplateNames()
        {
            if (!_projectDirectory.DirectoryExists(TemplateDirectory)) return [];
            return _projectDirectory.GetDirectories(TemplateDirectory).Select(Path.GetFileName).OfType<string>();
        }

        public string LoadVersion()
        {
            var version = _projectDirectory.LoadFile(VersionFileName);
            return version is null ? Version : version;
        }

        public bool Migrate()
        {
            var migratorKey = IMigrator.GetMigratorKey(LoadVersion(), Version);
            var migrator = _registeredMigrators[migratorKey];
            var migrateSucessful = migrator.Migrate(_projectDirectory);
            if (migrateSucessful) SaveVersion();
            return migrateSucessful;
        }

        public bool NeedsMigration() => Version != LoadVersion();

        public void RegisterMigrator(string fromVersion, string toVersion, IMigrator migrator)
        {
            var migratorKey = IMigrator.GetMigratorKey(fromVersion, toVersion);
            _registeredMigrators.Add(migratorKey, migrator);
        }

        public void SaveVersion()
        {
            _projectDirectory.SaveFile(VersionFileName, Version);
        }
    }
}
