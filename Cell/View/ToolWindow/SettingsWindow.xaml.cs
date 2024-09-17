using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows;
using System.Windows.Controls;

namespace Cell.View.ToolWindow
{
    public partial class SettingsWindow : UserControl, IResizableToolWindow
    {
        private readonly SettingsWindowViewModel _viewModel;
        public SettingsWindow(SettingsWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel;
            InitializeComponent();
        }

        public Action? RequestClose { get; set; }

        public double GetMinimumHeight() => 350;

        public string GetTitle() => "Settings";

        public List<CommandViewModel> GetToolBarCommands()
        {
            return
            [
            ];
        }

        public double GetMinimumWidth() => 350;

        public bool HandleCloseRequested()
        {
            return true;
        }

        private void OpenSaveLocationButtonClicked(object sender, RoutedEventArgs e)
        {
            ApplicationViewModel.Instance.PersistenceManager.GetFullPath();
        }

        private void ShowLogWindowButtonClick(object sender, RoutedEventArgs e)
        {
            var logWindowViewModel = new LogWindowViewModel();
            var logWindow = new LogWindow(logWindowViewModel);
            ApplicationViewModel.Instance.ShowToolWindow(logWindow);
        }

        private void ShowUndoRedoStackWindowButtonClick(object sender, RoutedEventArgs e)
        {
            var undoRedoStackWindowViewModel = new UndoRedoStackWindowViewModel();
            var undoRedoStackWindow = new UndoRedoStackWindow(undoRedoStackWindowViewModel);
            ApplicationViewModel.Instance.ShowToolWindow(undoRedoStackWindow);
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

        private void CreateBackupButtonClicked(object sender, RoutedEventArgs e)
        {
            ApplicationViewModel.Instance.BackupManager.CreateBackup();
            DialogFactory.ShowDialog("Backup created", "Backup created successfully.");
        }

        private void PrintCurrentSheetButtonClicked(object sender, RoutedEventArgs e)
        {
            //var printDialog = new PrintDialog();

            DialogFactory.ShowDialog("Under construction", "Not quite ready :)");
            // TODO print cells without black background
            //printDialog.PrintVisual(ApplicationViewModel.Instance.ActiveSheetView, $"Print {ApplicationViewModel.Instance.SheetViewModel?.SheetName}");
            //printDialog.ShowDialog();
        }

        private void DefaultCellFormatEditorButtonClicked(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            if (ApplicationViewModel.Instance.SheetViewModel == null) return;
            var styleCell = ApplicationViewModel.Instance.ApplicationSettings.DefaultCellStyleCellModel;
            var cellFormatEditorWindowViewModel = new CellFormatEditWindowViewModel([styleCell], ApplicationViewModel.Instance.CellTracker, ApplicationViewModel.Instance.PluginFunctionLoader);
            var editPanel = new CellFormatEditWindow(cellFormatEditorWindowViewModel);
            ApplicationViewModel.Instance.ShowToolWindow(editPanel);
        }

        private void DefaultRowColumnCellFormatEditorButtonClicked(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            if (ApplicationViewModel.Instance.SheetViewModel == null) return;
            var styleCell = ApplicationViewModel.Instance.ApplicationSettings.DefaultSpecialCellStyleCellModel;
            var cellFormatEditorWindowViewModel = new CellFormatEditWindowViewModel([styleCell], ApplicationViewModel.Instance.CellTracker, ApplicationViewModel.Instance.PluginFunctionLoader);
            var editPanel = new CellFormatEditWindow(cellFormatEditorWindowViewModel);
            ApplicationViewModel.Instance.ShowToolWindow(editPanel);
        }

        private void RestoreFromBackupButtonClicked(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            _viewModel.RestoreFromBackup();
        }

        public void HandleBeingClosed()
        {
        }

        public void HandleBeingShown()
        {
        }
    }
}
