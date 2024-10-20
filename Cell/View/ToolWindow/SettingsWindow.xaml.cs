using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows;
using System.Windows.Controls;

namespace Cell.View.ToolWindow
{
    public partial class SettingsWindow : ResizableToolWindow
    {
        private readonly SettingsWindowViewModel _viewModel;
        /// <summary>
        /// Creates a new instance of <see cref="SettingsWindow"/>.
        /// </summary>
        /// <param name="viewModel">The view model for this view.</param>
        public SettingsWindow(SettingsWindowViewModel viewModel) : base(viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
        }

        private void CreateBackupButtonClicked(object sender, RoutedEventArgs e)
        {
            ApplicationViewModel.Instance.BackupManager.CreateBackup();
            ApplicationViewModel.Instance.DialogFactory.Show("Backup created", "Backup created successfully.");
        }

        private void DefaultCellFormatEditorButtonClicked(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            _viewModel.OpenEditorForDefaultCellFormat();
        }

        private void DefaultRowColumnCellFormatEditorButtonClicked(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            _viewModel.OpenEditorForDefaultRowAndColumnCellFormat();
        }

        private void OpenSaveLocationButtonClicked(object sender, RoutedEventArgs e)
        {
            ApplicationViewModel.Instance.PersistedProject.GetRootPath();
        }

        private void PrintCurrentSheetButtonClicked(object sender, RoutedEventArgs e)
        {
            //var printDialog = new PrintDialog();
            ApplicationViewModel.Instance.DialogFactory.Show("Under construction", "Not quite ready :)");
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
    }
}
