using Cell.View.Cells;
using Cell.View.Converters;
using Cell.View.ToolWindow;
using Cell.ViewModel.Application;
using Cell.ViewModel.Cells;
using Cell.ViewModel.ToolWindow;
using ICSharpCode.AvalonEdit.Editing;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            CreateWindowDockPanelView();
        }

        private void CreateWindowDockPanelView()
        {
            if (_viewModel.WindowDockPanelViewModel is null) return;
            _windowDockPanel.Content = new WindowDockPanel(_viewModel.WindowDockPanelViewModel, _toolWindowCanvas);
        }

        private void AdjustWindowSize()
        {
            if (WindowState == WindowState.Maximized) WindowState = WindowState.Normal;
            else WindowState = WindowState.Maximized;
        }

        private void ApplicationViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ApplicationViewModel.ApplicationWindowWidth) || e.PropertyName == nameof(ApplicationViewModel.ApplicationWindowHeight))
            {
                Width = _viewModel.ApplicationWindowWidth;
                Height = _viewModel.ApplicationWindowHeight;
                (_windowDockPanel.Content as WindowDockPanel)?.UpdateToolWindowLocation(ActualWidth, ActualHeight);
            }
            else if (e.PropertyName == nameof(ApplicationViewModel.SheetViewModel))
            {
                ShowSheetView(_viewModel.SheetViewModel);
            }
            else if (e.PropertyName == nameof(ApplicationViewModel.WindowDockPanelViewModel))
            {
                CreateWindowDockPanelView();
            }
        }

        private void CloseAllSheetViews()
        {
            _sheetViews.Clear();
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
            _viewModel.ShutdownApplicationGracefully();
        }

        private void OpenTextEditPanelButtonClick(object sender, RoutedEventArgs e)
        {
            if (_viewModel.CellSelector is null) return;
            if (_viewModel.FunctionTracker is null) return;
            var cellContentEditWindowViewModel = new CellContentEditWindowViewModel(_viewModel.CellSelector.SelectedCells, _viewModel.FunctionTracker, _viewModel.Logger);
            if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) _viewModel.DockToolWindow(cellContentEditWindowViewModel, Dock.Top);
            else _viewModel.ShowToolWindow(cellContentEditWindowViewModel);
        }

        private void ShowCollectionManagerButtonClick(object sender, RoutedEventArgs e)
        {
            if (_viewModel.UserCollectionTracker is null) return;
            if (_viewModel.FunctionTracker is null) return;
            var collectionManagerViewModel = new CollectionManagerWindowViewModel(_viewModel.UserCollectionTracker, _viewModel.FunctionTracker);
            _viewModel.ShowToolWindow(collectionManagerViewModel);
        }

        private void ShowFunctionManagerButtonClick(object sender, RoutedEventArgs e)
        {
            var functionLoader = _viewModel.FunctionTracker;
            if (functionLoader is null) return;
            var functionManagerViewModel = new FunctionManagerWindowViewModel(functionLoader);
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) _viewModel.DockToolWindow(functionManagerViewModel, Dock.Right);
            else _viewModel.ShowToolWindow(functionManagerViewModel);
        }

        private void ShowSettingsWindowButtonClick(object sender, RoutedEventArgs e)
        {
            var settingsWindowViewModel = new SettingsWindowViewModel();
            _viewModel.ShowToolWindow(settingsWindowViewModel);
        }

        private void ShowSheetManagerButtonClick(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SheetTracker is null) return;
            if (_viewModel.DialogFactory is null) return;
            var sheetManagerViewModel = new SheetManagerWindowViewModel(_viewModel.SheetTracker, _viewModel.DialogFactory);
            _viewModel.ShowToolWindow(sheetManagerViewModel);
        }

        private void ShowSheetView(SheetViewModel? sheetViewModel)
        {
            if (sheetViewModel is null) return;
            if (!_sheetViews.TryGetValue(sheetViewModel, out var sheetView))
            {
                sheetView = new SheetView(sheetViewModel);
                _sheetViews.Add(sheetViewModel, sheetView);
            }
            _viewModel.WindowDockPanelViewModel.MainContent = sheetView;
        }

        private void ToggleEditPanelButtonClick(object sender, RoutedEventArgs e)
        {
            if (_viewModel?.SheetViewModel is null) return;
            if (_viewModel.CellTracker is null) return;
            if (_viewModel.FunctionTracker is null) return;
            var viewModel = new CellFormatEditWindowViewModel(_viewModel.SheetViewModel.CellSelector.SelectedCells, _viewModel.CellTracker, _viewModel.FunctionTracker, _viewModel.UndoRedoManager);
            _viewModel.ShowToolWindow(viewModel);
        }

        private void WindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (IsUserTypingIntoTextInputBox()) return;
            if (IsControlPressed())
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
                    _viewModel.UndoRedoManager?.Undo();
                    e.Handled = true;
                }
                else if (e.Key == Key.Y)
                {
                    _viewModel.UndoRedoManager?.Redo();
                    e.Handled = true;
                }
                else if (e.Key == Key.T)
                {
                    if (ApplicationViewModel.Instance.SheetTracker is null) return;
                    if (ApplicationViewModel.Instance.DialogFactory is null) return;
                    var createSheetWindowViewModel = new CreateSheetWindowViewModel(ApplicationViewModel.Instance.SheetTracker, ApplicationViewModel.Instance.DialogFactory);
                    ApplicationViewModel.Instance.ShowToolWindow(createSheetWindowViewModel);
                }
                else if (e.Key == Key.S)
                {
                    ThemeColorConverter.IsDarkMode = !ThemeColorConverter.IsDarkMode;
                    var activeSheet = _viewModel.SheetViewModel;
                    CloseAllSheetViews();
                    ShowSheetView(activeSheet);
                }
                else if (e.Key == Key.Tab)
                {
                    if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0) _viewModel.GoToPreviousSheet();
                    else _viewModel.GoToNextSheet();
                }
            }
            else if (e.Key == Key.Tab)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0) _viewModel.CellSelector?.MoveSelectionLeft();
                else _viewModel.CellSelector?.MoveSelectionRight();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0) _viewModel.CellSelector?.MoveSelectionUp();
                else _viewModel.CellSelector?.MoveSelectionDown();
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0) _viewModel.CellSelector?.AddToSelectionUp();
                else _viewModel.CellSelector?.MoveSelectionUp();
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0) _viewModel.CellSelector?.AddToSelectionDown();
                else _viewModel.CellSelector?.MoveSelectionDown();
                e.Handled = true;
            }
            else if (e.Key == Key.Left)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0) _viewModel.CellSelector?.AddToSelectionLeft();
                else _viewModel.CellSelector?.MoveSelectionLeft();
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0) _viewModel.CellSelector?.AddToSelectionRight();
                else _viewModel.CellSelector?.MoveSelectionRight();
                e.Handled = true;
            }
            else if (e.Key == Key.Delete)
            {
                var selectedCells = _viewModel.CellSelector?.SelectedCells ?? [];
                _viewModel.UndoRedoManager?.StartRecordingUndoState();
                foreach (var cell in selectedCells)
                {
                    _viewModel.UndoRedoManager?.RecordStateIfRecording(cell);
                    cell.Text = "";
                }
                _viewModel.UndoRedoManager?.FinishRecordingUndoState();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                _viewModel.SheetViewModel?.CellSelector.UnselectAllCells();
                e.Handled = true;
            }

            static bool IsUserTypingIntoTextInputBox()
            {
                return Mouse.DirectlyOver is TextArea 
                    || Mouse.DirectlyOver is TextBox 
                    || Keyboard.FocusedElement is TextArea 
                    || Keyboard.FocusedElement is TextBox;
            }

            static bool IsControlPressed() => (Keyboard.Modifiers & ModifierKeys.Control) != 0;
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Maximized) BorderThickness = new Thickness(7.5);
            else BorderThickness = new Thickness(0);
            (_windowDockPanel.Content as WindowDockPanel)?.UpdateToolWindowLocation(ActualWidth, ActualHeight);
        }
    }
}
