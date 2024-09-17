namespace Cell.Persistence
{
    public class BackupManager
    {
        private readonly PersistedDirectory _projectDirectory;
        private readonly PersistedDirectory _backupDirectory;

        public PersistedDirectory BackupDirectory => _backupDirectory;

        public BackupManager(PersistedDirectory projectDirectory, PersistedDirectory backupDirectory)
        {
            _projectDirectory = projectDirectory;
            _backupDirectory = backupDirectory;
        }

        public void CreateBackup(string backupName = "backup")
        {
            _projectDirectory.ZipTo(_backupDirectory, "", $"Cell_{backupName}_{CreateFileFriendlyCurrentDateTime()}");
        }

        private static string CreateFileFriendlyCurrentDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        }

        internal void RestoreBackup(string backupName)
        {
            _backupDirectory.UnzipTo(_projectDirectory, backupName, "");
        }
    }
}
