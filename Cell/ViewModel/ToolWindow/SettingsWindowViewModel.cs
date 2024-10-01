using Cell.Persistence;
using Cell.ViewModel.Application;
using System.Windows.Forms;

namespace Cell.ViewModel.ToolWindow
{
    public class SettingsWindowViewModel : ToolWindowViewModel
    {
        private readonly ApplicationSettings _applicationSettings;
        public SettingsWindowViewModel(ApplicationSettings applicationSettings)
        {
            _applicationSettings = applicationSettings;
        }

        public ApplicationSettings? ApplicationSettings => _applicationSettings;

        /// <summary>
        /// Gets the string displayed in top bar of this tool window.
        /// </summary>
        public override string ToolWindowTitle => "Settings";

        /// <summary>
        /// Gets the default height of this tool window when it is shown.
        /// </summary>
        public override double DefaultHeight => 380;

        /// <summary>
        /// Gets the default width of this tool window when it is shown.
        /// </summary>
        public override double DefaultWidth => 350;

        public override double MinimumHeight => 100;

        public override double MinimumWidth => 100;

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
