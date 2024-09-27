namespace Cell.Persistence
{
    public class BackupManager
    {
        private readonly PersistedDirectory _backupDirectory;
        private readonly PersistedDirectory _projectDirectory;
        public BackupManager(PersistedDirectory projectDirectory, PersistedDirectory backupDirectory)
        {
            _projectDirectory = projectDirectory;
            _backupDirectory = backupDirectory;
        }

        public PersistedDirectory BackupDirectory => _backupDirectory;

        public void CreateBackup(string backupName = "backup")
        {
            _projectDirectory.ZipTo(_backupDirectory, "", $"Cell_{backupName}_{CreateFileFriendlyCurrentDateTime()}");
        }

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
