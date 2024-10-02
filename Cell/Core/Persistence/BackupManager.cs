namespace Cell.Persistence
{
    /// <summary>
    /// Captures and restores backups of a project directory.
    /// </summary>
    public class BackupManager
    {
        private readonly PersistedDirectory _backupDirectory;
        private readonly PersistedDirectory _projectDirectory;
        /// <summary>
        /// Creates a new instance of the <see cref="BackupManager"/> class.
        /// </summary>
        /// <param name="projectDirectory">The project directory the backups will be taken of.</param>
        /// <param name="backupDirectory">The backup directory to put backups in. Multiple backups will be put in this directory, each in thier own subdirectory.</param>
        public BackupManager(PersistedDirectory projectDirectory, PersistedDirectory backupDirectory)
        {
            _projectDirectory = projectDirectory;
            _backupDirectory = backupDirectory;
        }

        /// <summary>
        /// Captures a new backup with a name that includes the given backupName and the current date and time.
        /// </summary>
        /// <param name="backupName">An extra name to add to the backup file name.</param>
        public void CreateBackup(string backupName = "backup")
        {
            _projectDirectory.ZipTo(_backupDirectory, "", $"Cell_{backupName}_{CreateFileFriendlyCurrentDateTime()}");
        }

        /// <summary>
        /// Gets the paths of all of the backups in the backup directory.
        /// </summary>
        /// <returns>A list of paths to backups.</returns>
        public IEnumerable<string> GetBackups()
        {
            return _backupDirectory.GetDirectories("");
        }

        /// <summary>
        /// Replaces the contents of the project directory with the contents of the backup with the specified name.
        /// </summary>
        /// <param name="backupName">The backup to restore.</param>
        public void RestoreBackup(string backupName)
        {
            _backupDirectory.UnzipTo(_projectDirectory, backupName, "");
        }

        private static string CreateFileFriendlyCurrentDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        }
    }
}
