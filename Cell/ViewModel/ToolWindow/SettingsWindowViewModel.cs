using Cell.Common;
using Cell.Persistence;
using Cell.ViewModel.Application;
using System.Windows.Forms;

namespace Cell.ViewModel.ToolWindow
{
    public class SettingsWindowViewModel : PropertyChangedBase
    {
        public SettingsWindowViewModel()
        {
        }

        public ApplicationSettings? ApplicationSettings => ApplicationViewModel.SafeInstance?.ApplicationSettings;

        public void RestoreFromBackup()
        {
            ApplicationViewModel.Instance.BackupManager.CreateBackup("PreRestore");
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Cell Backup Files (*.zip)",
                Title = "Load Backup",
                CheckPathExists = true,
                CheckFileExists = true,
                InitialDirectory = ApplicationViewModel.Instance.BackupManager.BackupDirectory.GetFullPath()
            };
            var result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                ApplicationViewModel.Instance.BackupManager.RestoreBackup(openFileDialog.FileName);
                App.Current.Shutdown();
            }
        }
    }
}
