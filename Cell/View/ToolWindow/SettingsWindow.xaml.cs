using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Cell.View.ToolWindow
{
    public partial class SettingsWindow : UserControl, IResizableToolWindow
    {
        private readonly SettingsWindowViewModel _viewModel;
        private double _height = 350;
        private double _width = 350;
        public SettingsWindow(SettingsWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel;
            _viewModel.UserSetWidth = GetWidth();
            _viewModel.UserSetHeight = GetHeight();
            InitializeComponent();
        }

        public Action? RequestClose { get; set; }

        public double GetHeight() => _height;

        public string GetTitle() => "Settings";

        public List<CommandViewModel> GetToolBarCommands()
        {
            return
            [
            ];
        }

        public double GetWidth() => _width;

        public bool HandleBeingClosed()
        {
            return true;
        }

        public void SetHeight(double height)
        {
            _height = height;
            _viewModel.UserSetHeight = height;
        }

        public void SetWidth(double width)
        {
            _width = width;
            _viewModel.UserSetWidth = width;
        }

        private void OpenSaveLocationButtonClicked(object sender, RoutedEventArgs e)
        {
            ApplicationViewModel.Instance.PersistenceManager.OpenRootDirectoryInExplorer();
        }

        private void ShowHelpButtonClick(object sender, RoutedEventArgs e)
        {
            var helpWindow = new HelpWindow();
            helpWindow.SetBinding(DataContextProperty, new Binding("SheetViewModel.SelectedCellViewModel") { Source = ApplicationViewModel.Instance });
            ApplicationViewModel.Instance.ShowToolWindow(helpWindow);
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
            var cellFormatEditorWindowViewModel = new CellFormatEditWindowViewModel([styleCell], ApplicationViewModel.Instance.CellTracker);
            var editPanel = new CellFormatEditWindow(cellFormatEditorWindowViewModel);
            ApplicationViewModel.Instance.ShowToolWindow(editPanel);
        }

        private void DefaultRowColumnCellFormatEditorButtonClicked(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            if (ApplicationViewModel.Instance.SheetViewModel == null) return;
            var styleCell = ApplicationViewModel.Instance.ApplicationSettings.DefaultSpecialCellStyleCellModel;
            var cellFormatEditorWindowViewModel = new CellFormatEditWindowViewModel([styleCell], ApplicationViewModel.Instance.CellTracker);
            var editPanel = new CellFormatEditWindow(cellFormatEditorWindowViewModel);
            ApplicationViewModel.Instance.ShowToolWindow(editPanel);
        }

        private void LoadFromBackupButtonPressed(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            _viewModel.LoadFromBackup();
        }
    }
}
