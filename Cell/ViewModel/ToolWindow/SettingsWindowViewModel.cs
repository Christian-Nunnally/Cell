﻿using Cell.ViewModel.Application;
using System.Windows.Forms;

namespace Cell.ViewModel.ToolWindow
{
    /// <summary>
    /// A view model for the settings window, which provides application wide setting control.
    /// </summary>
    public class SettingsWindowViewModel : ToolWindowViewModel
    {
        /// <summary>
        /// Creates a new instance of a <see cref="SettingsWindowViewModel"/>.
        /// </summary>
        public SettingsWindowViewModel()
        {
        }

        /// <summary>
        /// Gets the default height of this tool window when it is shown.
        /// </summary>
        public override double DefaultHeight => 380;

        /// <summary>
        /// Gets the default width of this tool window when it is shown.
        /// </summary>
        public override double DefaultWidth => 350;

        /// <summary>
        /// Gets the minimum height this tool window is allowed to be reized to.
        /// </summary>
        public override double MinimumHeight => 100;

        /// <summary>
        /// Gets the minimum width this tool window is allowed to be reized to.
        /// </summary>
        public override double MinimumWidth => 100;

        /// <summary>
        /// Gets the string displayed in top bar of this tool window.
        /// </summary>
        public override string ToolWindowTitle => "Settings";

        /// <summary>
        /// Creates a backup of the current project, then opens a file dialog to select a backup to restore from.
        /// 
        /// When the backup file is selected, the application is closed and the backup is restored to the project directory location so that it is loadeded when the application is opened again.
        /// </summary>
        public void RestoreFromBackup()
        {
            var backup = ApplicationViewModel.Instance.BackupManager.GetBackups().FirstOrDefault();
            if (backup == null) return;
            ApplicationViewModel.Instance.BackupManager.CreateBackup("PreRestore");
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Cell Backup Files (*.zip)",
                Title = "Load Backup",
                CheckPathExists = true,
                CheckFileExists = true,
                InitialDirectory = backup
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
