using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows;
using System.Windows.Controls;

namespace Cell.View.ToolWindow
{
    public partial class SettingsWindow : ResizableToolWindow
    {
        private SettingsWindowViewModel _viewModel;
        public SettingsWindow(SettingsWindowViewModel viewModel) : base(viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
        }

        private void CreateBackupButtonClicked(object sender, RoutedEventArgs e)
        {
            ApplicationViewModel.Instance.BackupManager.CreateBackup();
            DialogFactory.ShowDialog("Backup created", "Backup created successfully.");
        }

        private void DefaultCellFormatEditorButtonClicked(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            if (ApplicationViewModel.Instance.SheetViewModel == null) return;
            var styleCell = ApplicationViewModel.Instance.ApplicationSettings.DefaultCellStyleCellModel;
            var cellFormatEditorWindowViewModel = new CellFormatEditWindowViewModel([styleCell], ApplicationViewModel.Instance.CellTracker, ApplicationViewModel.Instance.PluginFunctionLoader);
            ApplicationViewModel.Instance.ShowToolWindow(cellFormatEditorWindowViewModel);
        }

        private void DefaultRowColumnCellFormatEditorButtonClicked(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
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
            if (_viewModel == null) return;
            _viewModel.RestoreFromBackup();
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

        private void ToggleAbilityToSelectCells(object sender, RoutedEventArgs e)
        {
            if (sender is not Button) return;
            var cellSelector = ApplicationViewModel.Instance.CellSelector;
            var activeSheetViewModel = ApplicationViewModel.Instance.SheetViewModel;
            if (cellSelector is null) return;
            if (activeSheetViewModel is null) return;
            cellSelector.IsSelectingEnabled = !cellSelector.IsSelectingEnabled;
            activeSheetViewModel.IsCellHighlightOnMouseOverEnabled = cellSelector.IsSelectingEnabled;
        }

        private void ToggleCenterLockButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button) return;
            var activeSheetView = ApplicationViewModel.Instance.ActiveSheetView;
            if (activeSheetView is null) return;
            activeSheetView.IsLockedToCenter = !activeSheetView.IsLockedToCenter;
        }

        private void TogglePanLockButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button) return;
            var activeSheetView = ApplicationViewModel.Instance.ActiveSheetView;
            if (activeSheetView is null) return;
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
