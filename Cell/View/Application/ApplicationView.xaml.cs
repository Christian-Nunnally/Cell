using Cell.View.Cells;
using Cell.View.ToolWindow;
using Cell.ViewModel.Application;
using Cell.ViewModel.Cells;
using Cell.ViewModel.ToolWindow;
using ICSharpCode.AvalonEdit.Editing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Cell.View.Application
{
    public partial class ApplicationView : Window
    {
        private readonly Dictionary<SheetViewModel, SheetView> _sheetViews = [];
        private readonly ApplicationViewModel _viewModel;

        /// <summary>
        /// Creates a new instance of the application view.
        /// </summary>
        public ApplicationView(ApplicationViewModel viewModel)
        {
            DataContext = viewModel;
            _viewModel = viewModel;
            viewModel.PropertyChanged += ApplicationViewModelPropertyChanged;
            InitializeComponent();
            _titleBarSheetNavigationView.DataContext = new TitleBarSheetNavigationViewModel(_viewModel.SheetTracker);
            viewModel.AttachToView(this);
        }

        /// <summary>
        /// Gets the active sheet view.
        /// </summary>
        public SheetView? ActiveSheetView { get; set; }

        /// <summary>
        /// Displays the given sheet view model in the main content control.
        /// </summary>
        /// <param name="sheetViewModel">The sheet to open.</param>
        public void ShowSheetView(SheetViewModel sheetViewModel)
        {
            if (sheetViewModel == null) return;
            if (!_sheetViews.TryGetValue(sheetViewModel, out var sheetView))
            {
                sheetView = new SheetView(sheetViewModel);
                _sheetViews.Add(sheetViewModel, sheetView);
            }
            _sheetViewContentControl.Content = sheetView;
            ActiveSheetView = sheetView;
        }

        /// <summary>
        /// Opens a tool window with the specified view model.
        /// </summary>
        /// <param name="viewModel">The tool window view model to open.</param>
        /// <param name="allowDuplicates">Whether or not to open the new window if one already exists of the same type.</param>
        public void ShowToolWindow(ToolWindowViewModel viewModel, bool allowDuplicates = false)
        {
            var window = ToolWindowViewFactory.Create(viewModel);
            if (window is null) return;
            ShowToolWindow(window, allowDuplicates);
        }

        /// <summary>
        /// Opens a tool window with the specified view model.
        /// </summary>
        /// <param name="viewModel">The tool window view model to open.</param>
        /// <param name="dock">The side to dock to.</param>
        /// <param name="allowDuplicates">Whether or not to open the new window if one already exists of the same type.</param>
        public void DockToolWindow(ToolWindowViewModel viewModel, Dock dock, bool allowDuplicates = false)
        {
            var window = ToolWindowViewFactory.Create(viewModel);
            if (window is null) return;
            DockToolWindow(window, dock, allowDuplicates);
        }

        private void AdjustWindowSize()
        {
            if (WindowState == WindowState.Maximized) WindowState = WindowState.Normal;
            else WindowState = WindowState.Maximized;
        }

        private void ApplicationViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ApplicationViewModel.ApplicationWindowWidth) || e.PropertyName == nameof(ApplicationViewModel.ApplicationWindowHeight))
            {
                UpdateToolWindowLocation();
            }
        }

        private void LoadProjectButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not Button loadButton) return;
            loadButton.Content = "Loading...";
            var loadProgress = ApplicationViewModel.Instance.LoadWithProgress();
            while (!loadProgress.IsComplete)
            {
                loadButton.Content = loadProgress.Message;
                App.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
                loadProgress = loadProgress.Continue();
            }
            loadButton.Content = loadProgress.Message;
        }

        private void MaximizeButtonClick(object sender, RoutedEventArgs e)
        {
            AdjustWindowSize();
        }

        private void MinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void OnCloseButtonClicked(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void OpenTextEditPanelButtonClick(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            var cellContentEditWindowViewModel = new CellContentEditWindowViewModel(_viewModel.CellSelector.SelectedCells);
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) DockToolWindow(cellContentEditWindowViewModel, Dock.Top);
            else ShowToolWindow(cellContentEditWindowViewModel);
        }

        private void ShowCollectionManagerButtonClick(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            var collectionManagerViewModel = new CollectionManagerWindowViewModel(_viewModel.UserCollectionLoader, _viewModel.PluginFunctionLoader);
            ShowToolWindow(collectionManagerViewModel);
        }

        private void ShowFunctionManagerButtonClick(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            var functionLoader = _viewModel.PluginFunctionLoader;
            var functionManagerViewModel = new FunctionManagerWindowViewModel(functionLoader);
            ShowToolWindow(functionManagerViewModel);
        }

        private void ShowSettingsWindowButtonClick(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            var settingsWindowViewModel = new SettingsWindowViewModel();
            ShowToolWindow(settingsWindowViewModel);
        }

        private void ShowSheetManagerButtonClick(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            var sheetManagerViewModel = new SheetManagerWindowViewModel(_viewModel.SheetTracker, ApplicationViewModel.Instance.DialogFactory);
            ShowToolWindow(sheetManagerViewModel);
        }

        private void ShowToolWindow(ResizableToolWindow resizableToolWindow, bool allowDuplicates = false)
        {
            if (!allowDuplicates)
            {
                foreach (var floatingToolWindow in _toolWindowCanvas.Children.Cast<FloatingToolWindow>())
                {
                    if (floatingToolWindow.ContentHost.Content.GetType() == resizableToolWindow.GetType())
                    {
                        return;
                    }
                }
            }

            var toolbox = new FloatingToolWindow(_toolWindowCanvas);
            toolbox.SetContent(resizableToolWindow);

            Canvas.SetLeft(toolbox, (_toolWindowCanvas.ActualWidth / 2) - (toolbox.ContentWidth / 2));
            Canvas.SetTop(toolbox, (_toolWindowCanvas.ActualHeight / 2) - (toolbox.ContentHeight / 2));

            _toolWindowCanvas.Children.Add(toolbox);
            resizableToolWindow.ToolViewModel.HandleBeingShown();
        }

        private void DockToolWindow(ResizableToolWindow resizableToolWindow, Dock dockSide, bool allowDuplicates = false)
        {
            if (!allowDuplicates)
            {
                foreach (var floatingToolWindow in _toolWindowCanvas.Children.Cast<FloatingToolWindow>())
                {
                    if (floatingToolWindow.ContentHost.Content.GetType() == resizableToolWindow.GetType())
                    {
                        return;
                    }
                }
            }


            DockPanel.SetDock(resizableToolWindow, dockSide);

            _toolWindowDockPanel.Children.Add(resizableToolWindow);
            resizableToolWindow.ToolViewModel.HandleBeingShown();
        }

        private void ToggleEditPanelButtonClick(object sender, RoutedEventArgs e)
        {
            if (_viewModel?.SheetViewModel == null) return;
            var viewModel = new CellFormatEditWindowViewModel(_viewModel.SheetViewModel.CellSelector.SelectedCells, _viewModel.CellTracker, _viewModel.PluginFunctionLoader);
            ShowToolWindow(viewModel);
        }

        private void UpdateToolWindowLocation()
        {
            foreach (var toolWindow in _toolWindowCanvas.Children.Cast<FloatingToolWindow>())
            {
                toolWindow.HandleOwningCanvasSizeChanged();
            }
        }

        private void WindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_viewModel == null) return;
            if (Mouse.DirectlyOver is TextArea || Mouse.DirectlyOver is TextBox || Keyboard.FocusedElement is TextArea || Keyboard.FocusedElement is TextBox) return; // Disable keyboard shortcuts when typing in a textbox
            if (e.IsDown && (Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                if (e.Key == Key.C)
                {
                    _viewModel.CopySelectedCells((Keyboard.Modifiers & ModifierKeys.Shift) == 0);
                    e.Handled = true;
                }
                else if (e.Key == Key.V)
                {
                    _viewModel.PasteCopiedCells();
                    e.Handled = true;
                }
                else if (e.Key == Key.Z)
                {
                    _viewModel.UndoRedoManager.Undo();
                    e.Handled = true;
                }
                else if (e.Key == Key.Y)
                {
                    _viewModel.UndoRedoManager.Redo();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Tab)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift) _viewModel.CellSelector.MoveSelectionLeft();
                else _viewModel.CellSelector.MoveSelectionRight();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift) _viewModel.CellSelector.MoveSelectionUp();
                else _viewModel.CellSelector.MoveSelectionDown();
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                _viewModel.CellSelector.MoveSelectionUp();
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                _viewModel.CellSelector.MoveSelectionDown();
                e.Handled = true;
            }
            else if (e.Key == Key.Left)
            {
                _viewModel.CellSelector.MoveSelectionLeft();
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                _viewModel.CellSelector.MoveSelectionRight();
                e.Handled = true;
            }
            else if (e.Key == Key.Delete)
            {
                var selectedCells = _viewModel.CellSelector.SelectedCells;
                _viewModel.UndoRedoManager.StartRecordingUndoState();
                foreach (var cell in selectedCells)
                {
                    _viewModel.UndoRedoManager.RecordStateIfRecording(cell);
                    cell.Text = "";
                }
                _viewModel.UndoRedoManager.FinishRecordingUndoState();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                _viewModel.SheetViewModel?.CellSelector.UnselectAllCells();
                e.Handled = true;
            }
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Maximized) BorderThickness = new Thickness(7.5);
            else BorderThickness = new Thickness(0);
        }
    }
}
