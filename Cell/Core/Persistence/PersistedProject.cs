using Cell.Core.Persistence.Migration;
using System.IO;

namespace Cell.Core.Persistence
{
    /// <summary>
    /// A persisted directory that specifically represents the directory of a project.
    /// </summary>
    public class PersistedProject
    {
        /// <summary>
        /// The relative directory where templates are stored within the project directory.
        /// </summary>
        public const string TemplateDirectory = "Templates";
        private const string VersionFileName = "version";
        private readonly PersistedDirectory _projectDirectory;
        private readonly Dictionary<string, IMigrator> _registeredMigrators = [];
        /// <summary>
        /// The version of the projects persisted model.
        /// </summary>
        public string Version = "2";
        /// <summary>
        /// Creates a new instance of <see cref="PersistedProject"/> at the given directory.
        /// </summary>
        /// <param name="projectDirectory">The directory that is a project directory.</param>
        public PersistedProject(PersistedDirectory projectDirectory)
        {
            _projectDirectory = projectDirectory;
        }

        /// <summary>
        /// Gets or sets whether this directory can be written to from this <see cref="PersistedProject"/>.
        /// </summary>
        public bool IsReadOnly { get => _projectDirectory.IsReadOnly; set => _projectDirectory.IsReadOnly = value; }

        /// <summary>
        /// Gets whether the project actually stored here can be migrated to the current version of this project directory object.
        /// </summary>
        /// <returns></returns>
        public bool CanMigrate() => _registeredMigrators.ContainsKey(IMigrator.GetMigratorKey(LoadVersion(), Version));

        /// <summary>
        /// Gets the names of all templates stored in the project directory.
        /// </summary>
        /// <returns>A list of template names.</returns>
        public IEnumerable<string> GetTemplateNames()
        {
            if (!_projectDirectory.DirectoryExists(TemplateDirectory)) return [];
            return _projectDirectory.GetDirectories(TemplateDirectory).Select(Path.GetFileName).OfType<string>();
        }

        /// <summary>
        /// Loads the version of the project stored in the project directory.
        /// </summary>
        /// <returns>The version of the persisted model on disk.</returns>
        public string LoadVersion()
        {
            var version = _projectDirectory.LoadFile(VersionFileName);
            return version is null ? Version : version;
        }

        /// <summary>
        /// Runs the migration for the project stored in the project directory to make it compatible with the current version of this project directory object.
        /// </summary>
        /// <returns>True if the migration completed without errors.</returns>
        public bool Migrate()
        {
            var migratorKey = IMigrator.GetMigratorKey(LoadVersion(), Version);
            var migrator = _registeredMigrators[migratorKey];
            var migrateSucessful = migrator.Migrate(_projectDirectory);
            if (migrateSucessful) SaveVersion();
            return migrateSucessful;
        }

        /// <summary>
        /// Tests if the files in the project directory are in the correct format or need to be migrated.
        /// </summary>
        /// <returns>True if the project files need to be migrated before they are loaded.</returns>
        public bool NeedsMigration() => Version != LoadVersion();

        /// <summary>
        /// Registers a migrator to migrate the project from one version to another.
        /// </summary>
        /// <param name="fromVersion">The version to migrate from.</param>
        /// <param name="toVersion">The version to migrate to.</param>
        /// <param name="migrator">The migrator that can do the migration.</param>
        public void RegisterMigrator(string fromVersion, string toVersion, IMigrator migrator)
        {
            var migratorKey = IMigrator.GetMigratorKey(fromVersion, toVersion);
            _registeredMigrators.Add(migratorKey, migrator);
        }

        /// <summary>
        /// Saves the version of this project to the project directory. Should only be done if the project has been successfully migrated.
        /// </summary>
        public void SaveVersion()
        {
            _projectDirectory.SaveFile(VersionFileName, Version);
        }

        /// <summary>
        /// Gets the root path for this project;
        /// </summary>
        /// <returns>The full path to the project directory.</returns>
        internal string GetRootPath() => _projectDirectory.GetFullPath();
    }
}
