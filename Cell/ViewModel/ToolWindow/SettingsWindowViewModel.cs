using Cell.Persistence;
using Cell.ViewModel.Application;

namespace Cell.ViewModel.ToolWindow
{
    public class SettingsWindowViewModel : ResizeableToolWindowViewModel
    {
        public SettingsWindowViewModel()
        {
        }

        public ApplicationSettings? ApplicationSettings => ApplicationViewModel.SafeInstance?.ApplicationSettings;

        internal void LoadFromBackup()
        {
            ApplicationViewModel.Instance.BackupManager.CreateBackup();
        }
    }
}
