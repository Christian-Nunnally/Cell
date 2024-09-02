using Cell.Model;
using Cell.Persistence;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Cell.View.ToolWindow
{
    public partial class SettingsWindow : UserControl, IResizableToolWindow
    {
        private double _width = 300;
        private double _height = 300;
        private readonly SettingsWindowViewModel _viewModel;
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

        private void TogglePanLockButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            var activeSheetView = ApplicationViewModel.Instance.ApplicationView.ActiveSheetView;
            if (activeSheetView is null) return;
            if (activeSheetView.PanAndZoomCanvas is null) return;
            activeSheetView.PanAndZoomCanvas.PanCanvasTo(CellModelFactory.DefaultCellWidth, CellModelFactory.DefaultCellHeight);
            activeSheetView.PanAndZoomCanvas.ZoomCanvasTo(new Point(0, 0), 1);
            activeSheetView.PanAndZoomCanvas.IsPanningEnabled = !activeSheetView.PanAndZoomCanvas.IsPanningEnabled;
        }

        private void TogglePopulateCellDependencyButtonClick(object sender, RoutedEventArgs e)
        {
            ApplicationSettings.Instance.HighlightPopulateCellDependencies = !ApplicationSettings.Instance.HighlightPopulateCellDependencies;
        }

        private void TogglePopulateCollectionDependencyButtonClick(object sender, RoutedEventArgs e)
        {
            ApplicationSettings.Instance.HighlightPopulateCollectionDependencies = !ApplicationSettings.Instance.HighlightPopulateCollectionDependencies;
        }

        private void ToggleTriggerCellDependencyButtonClick(object sender, RoutedEventArgs e)
        {
            ApplicationSettings.Instance.HighlightTriggerCellDependencies = !ApplicationSettings.Instance.HighlightTriggerCellDependencies;
        }

        private void ToggleTriggerCollectionDependencyButtonClick(object sender, RoutedEventArgs e)
        {
            ApplicationSettings.Instance.HighlightTriggerCollectionDependencies = !ApplicationSettings.Instance.HighlightTriggerCollectionDependencies;
        }

        private void ShowHelpButtonClick(object sender, RoutedEventArgs e)
        {
            var helpWindow = new HelpWindow();
            helpWindow.SetBinding(DataContextProperty, new Binding("SheetViewModel.SelectedCellViewModel") { Source = ApplicationViewModel.Instance });
            ApplicationViewModel.Instance.ApplicationView.ShowToolWindow(helpWindow);
        }

        private void ShowLogWindowButtonClick(object sender, RoutedEventArgs e)
        {
            var logWindowViewModel = new LogWindowViewModel();
            var logWindow = new LogWindow(logWindowViewModel);
            ApplicationViewModel.Instance.ApplicationView.ShowToolWindow(logWindow);
        }

        private void ShowUndoRedoStackWindowButtonClick(object sender, RoutedEventArgs e)
        {
            var undoRedoStackWindowViewModel = new UndoRedoStackWindowViewModel();
            var undoRedoStackWindow = new UndoRedoStackWindow(undoRedoStackWindowViewModel);
            ApplicationViewModel.Instance.ApplicationView.ShowToolWindow(undoRedoStackWindow);
        }

        private void OpenSaveLocationButtonClicked(object sender, RoutedEventArgs e)
        {
            ApplicationViewModel.Instance.PersistenceManager.OpenRootDirectoryInExplorer();
        }
    }
}
