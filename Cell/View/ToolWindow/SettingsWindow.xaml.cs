using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows;
using System.Windows.Controls;

namespace Cell.View.ToolWindow
{
    public partial class SettingsWindow : ResizableToolWindow
    {
        public SettingsWindow(SettingsWindowViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }

        public override double MinimumHeight => 380;

        public override double MinimumWidth => 350;

        private SettingsWindowViewModel SettingsWindowViewModel => (SettingsWindowViewModel)ToolViewModel;

        private void CreateBackupButtonClicked(object sender, RoutedEventArgs e)
        {
            ApplicationViewModel.Instance.BackupManager.CreateBackup();
            DialogFactory.ShowDialog("Backup created", "Backup created successfully.");
        }

        private void DefaultCellFormatEditorButtonClicked(object sender, RoutedEventArgs e)
        {
            if (SettingsWindowViewModel == null) return;
            if (ApplicationViewModel.Instance.SheetViewModel == null) return;
            var styleCell = ApplicationViewModel.Instance.ApplicationSettings.DefaultCellStyleCellModel;
            var cellFormatEditorWindowViewModel = new CellFormatEditWindowViewModel([styleCell], ApplicationViewModel.Instance.CellTracker, ApplicationViewModel.Instance.PluginFunctionLoader);
            ApplicationViewModel.Instance.ShowToolWindow(cellFormatEditorWindowViewModel);
        }

        private void DefaultRowColumnCellFormatEditorButtonClicked(object sender, RoutedEventArgs e)
        {
            if (SettingsWindowViewModel == null) return;
            if (ApplicationViewModel.Instance.SheetViewModel == null) return;
            var styleCell = ApplicationViewModel.Instance.ApplicationSettings.DefaultSpecialCellStyleCellModel;
            var cellFormatEditorWindowViewModel = new CellFormatEditWindowViewModel([styleCell], ApplicationViewModel.Instance.CellTracker, ApplicationViewModel.Instance.PluginFunctionLoader);
            ApplicationViewModel.Instance.ShowToolWindow(cellFormatEditorWindowViewModel);
        }

        private void OpenSaveLocationButtonClicked(object sender, RoutedEventArgs e)
        {
            ApplicationViewModel.Instance.PersistenceManager.GetFullPath();
        }

        private void PrintCurrentSheetButtonClicked(object sender, RoutedEventArgs e)
        {
            //var printDialog = new PrintDialog();
            DialogFactory.ShowDialog("Under construction", "Not quite ready :)");
        }

        private void RestoreFromBackupButtonClicked(object sender, RoutedEventArgs e)
        {
            if (SettingsWindowViewModel == null) return;
            SettingsWindowViewModel.RestoreFromBackup();
        }

        private void ShowLogWindowButtonClick(object sender, RoutedEventArgs e)
        {
            var logWindowViewModel = new LogWindowViewModel();
            ApplicationViewModel.Instance.ShowToolWindow(logWindowViewModel);
        }

        private void ShowUndoRedoStackWindowButtonClick(object sender, RoutedEventArgs e)
        {
            var undoRedoStackWindowViewModel = new UndoRedoStackWindowViewModel();
            ApplicationViewModel.Instance.ShowToolWindow(undoRedoStackWindowViewModel);
        }

        private void TogglePanLockButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button) return;
            var activeSheetView = ApplicationViewModel.Instance.ActiveSheetView;
            if (activeSheetView is null) return;
            activeSheetView.PanCanvasTo(-((ApplicationViewModel.Instance.ApplicationWindowWidth / 2) - (activeSheetView.SheetViewModel.SheetWidth / 2)), -((ApplicationViewModel.Instance.ApplicationWindowHeight / 2) - (activeSheetView.SheetViewModel.SheetHeight / 2)));
            activeSheetView.ZoomCanvasTo(new Point(0, 0), 1);
            activeSheetView.IsPanningEnabled = !activeSheetView.IsPanningEnabled;
        }

        private void TogglePopulateCellDependencyButtonClick(object sender, RoutedEventArgs e)
        {
            ApplicationViewModel.Instance.ApplicationSettings.HighlightPopulateCellDependencies = !ApplicationViewModel.Instance.ApplicationSettings.HighlightPopulateCellDependencies;
        }

        private void TogglePopulateCollectionDependencyButtonClick(object sender, RoutedEventArgs e)
        {
            ApplicationViewModel.Instance.ApplicationSettings.HighlightPopulateCollectionDependencies = !ApplicationViewModel.Instance.ApplicationSettings.HighlightPopulateCollectionDependencies;
        }

        private void ToggleTriggerCellDependencyButtonClick(object sender, RoutedEventArgs e)
        {
            ApplicationViewModel.Instance.ApplicationSettings.HighlightTriggerCellDependencies = !ApplicationViewModel.Instance.ApplicationSettings.HighlightTriggerCellDependencies;
        }

        private void ToggleTriggerCollectionDependencyButtonClick(object sender, RoutedEventArgs e)
        {
            ApplicationViewModel.Instance.ApplicationSettings.HighlightTriggerCollectionDependencies = !ApplicationViewModel.Instance.ApplicationSettings.HighlightTriggerCollectionDependencies;
        }
    }
}
