using Cell.View.Cells;
using Cell.View.Converters;
using Cell.View.Skin;
using Cell.View.ToolWindow;
using Cell.ViewModel.Application;
using Cell.ViewModel.Cells;
using Cell.ViewModel.ToolWindow;
using ICSharpCode.AvalonEdit.Editing;
using System.Collections.Specialized;
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
            viewModel.OpenToolWindowViewModels.CollectionChanged += OpenToolWindowViewModelsCollectionChanged;
            viewModel.MoveToolWindowToTop += MoveToolWindowToTop;
            InitializeComponent();
        }

        private void MoveToolWindowToTop(ToolWindowViewModel toolWindow)
        {
            foreach (var floatingContainer in _toolWindowCanvas.Children.OfType<FloatingToolWindowContainer>())
            {
                if (floatingContainer.ToolWindowContent?.ToolViewModel == toolWindow)
                {
                    _toolWindowCanvas.Children.Remove(floatingContainer);
                    _toolWindowCanvas.Children.Add(floatingContainer);
                    return;
                }
            }
        }

        private void OpenToolWindowViewModelsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems is not null)
            {
                foreach (var toolWindowViewModel in e.NewItems.Cast<ToolWindowViewModel>())
                {
                    toolWindowViewModel.PropertyChanged += ToolWindowViewModelPropertyChanged;
                    AddToolWindowToView(toolWindowViewModel);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems is not null)
            {
                foreach (var toolWindowViewModel in e.OldItems.Cast<ToolWindowViewModel>())
                {
                    toolWindowViewModel.PropertyChanged -= ToolWindowViewModelPropertyChanged;
                    RemoveToolWindowFromView(toolWindowViewModel);
                }
            }
        }

        private void AddToolWindowToView(ToolWindowViewModel toolWindowViewModel)
        {
            if (toolWindowViewModel.IsDocked)
            {
                ShowToolWindowInDockedContainer(toolWindowViewModel, toolWindowViewModel.Dock);
            }
            else
            {
                ShowToolWindowInFloatingContainer(toolWindowViewModel);
            }
        }

        private void ToolWindowViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var toolWindowViewModel = (ToolWindowViewModel)sender!;
            if (e.PropertyName == nameof(ToolWindowViewModel.IsDocked))
            {
                RemoveToolWindowFromView(toolWindowViewModel);
                AddToolWindowToView(toolWindowViewModel);
            }
        }

        private void ShowDockSites()
        {
            var topDock = new Border();
            DockPanel.SetDock(topDock, Dock.Top);
            topDock.Height = 10;
            topDock.Background = ColorConstants.ForegroundColorConstantBrush;
            topDock.Tag = "[DockSite]";
            topDock.MouseDown += DockMouseDown;
            _toolWindowDockPanel.Children.Insert(_toolWindowDockPanel.Children.Count - 1, topDock);

            var bottomDock = new Border();
            DockPanel.SetDock(bottomDock, Dock.Bottom);
            bottomDock.Height = 10;
            bottomDock.Background = ColorConstants.ForegroundColorConstantBrush;
            bottomDock.Tag = "[DockSite]";
            bottomDock.MouseDown += DockMouseDown;
            _toolWindowDockPanel.Children.Insert(_toolWindowDockPanel.Children.Count - 1, bottomDock);

            var leftDock = new Border();
            DockPanel.SetDock(leftDock, Dock.Left);
            leftDock.Width = 10;
            leftDock.Background = ColorConstants.ForegroundColorConstantBrush;
            leftDock.Tag = "[DockSite]";
            leftDock.MouseDown += DockMouseDown;
            _toolWindowDockPanel.Children.Insert(_toolWindowDockPanel.Children.Count - 1, leftDock);

            var rightDock = new Border();
            DockPanel.SetDock(rightDock, Dock.Right);
            rightDock.Width = 10;
            rightDock.Background = ColorConstants.ForegroundColorConstantBrush;
            rightDock.Tag = "[DockSite]";
            rightDock.MouseDown += DockMouseDown;
            _toolWindowDockPanel.Children.Insert(_toolWindowDockPanel.Children.Count - 1, rightDock);
        }

        private void DockMouseDown(object sender, MouseButtonEventArgs e)
        {
            var dockSite = (Border)sender!;
            var dockSide = DockPanel.GetDock(dockSite);
            HideDockSites();
            var topItemInCanvas = _toolWindowCanvas.Children.OfType<FloatingToolWindowContainer>().LastOrDefault();
            if (topItemInCanvas?.ToolWindowContent is null) return;
            topItemInCanvas.ToolWindowContent.ToolViewModel.Dock = dockSide;
            topItemInCanvas.ToolWindowContent.ToolViewModel.IsDocked = true;
        }

        private void HideDockSites()
        {
            foreach (var dockSite in _toolWindowDockPanel.Children.OfType<Border>().Where(x => x.Tag as string == "[DockSite]").ToList())
            {
                _toolWindowDockPanel.Children.Remove(dockSite);
            }
        }

        private void RemoveToolWindowFromView(ToolWindowViewModel toolWindowViewModel)
        {
            foreach (var dockedContainer in _toolWindowDockPanel.Children.OfType<DockedToolWindowContainer>())
            {
                if (dockedContainer.ToolWindowContent?.ToolViewModel == toolWindowViewModel)
                {
                    _toolWindowDockPanel.Children.Remove(dockedContainer);
                    return;
                }
            }
            foreach (var floatingContainer in _toolWindowCanvas.Children.OfType<FloatingToolWindowContainer>())
            {
                if (floatingContainer.ToolWindowContent?.ToolViewModel == toolWindowViewModel)
                {
                    _toolWindowCanvas.Children.Remove(floatingContainer);
                    return;
                }
            }
        }

        /// <summary>
        /// Gets the active sheet view.
        /// </summary>
        public SheetView? ActiveSheetView { get; set; }

        private void ShowSheetView(SheetViewModel? sheetViewModel)
        {
            if (sheetViewModel is null) return;
            if (!_sheetViews.TryGetValue(sheetViewModel, out var sheetView))
            {
                sheetView = new SheetView(sheetViewModel);
                _sheetViews.Add(sheetViewModel, sheetView);
            }
            _sheetViewContentControl.Content = sheetView;
            ActiveSheetView = sheetView;
        }

        private void CloseAllSheetViews()
        {
            if (ActiveSheetView is null) return;
            ActiveSheetView = null;
            _sheetViews.Clear();
        }

        /// <summary>
        /// Opens a tool window with the specified view model.
        /// </summary>
        /// <param name="viewModel">The tool window view model to open.</param>
        /// <param name="allowDuplicates">Whether or not to open the new window if one already exists of the same type.</param>
        private void ShowToolWindowInFloatingContainer(ToolWindowViewModel viewModel, bool allowDuplicates = false)
        {
            var window = ToolWindowViewFactory.Create(viewModel);
            if (window is null) return;
            if (!allowDuplicates && IsWindowOfTypeOpen(window.GetType())) return;
            OpenToolWindowInFloatingContainer(window);
        }

        /// <summary>
        /// Opens a tool window with the specified view model.
        /// </summary>
        /// <param name="viewModel">The tool window view model to open.</param>
        /// <param name="dock">The side to dock to.</param>
        /// <param name="allowDuplicates">Whether or not to open the new window if one already exists of the same type.</param>
        public void ShowToolWindowInDockedContainer(ToolWindowViewModel viewModel, Dock dock, bool allowDuplicates = false)
        {
            var window = ToolWindowViewFactory.Create(viewModel);
            if (window is null) return;
            if (!allowDuplicates && IsWindowOfTypeOpen(window.GetType())) return;
            ShowToolWindowInDockedContainer(window, dock);
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
                UpdateToolWindowLocation();
            }
            else if (e.PropertyName == nameof(ApplicationViewModel.SheetViewModel))
            {
                ShowSheetView(_viewModel.SheetViewModel);
            }
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
            if (_viewModel is null) return;
            if (_viewModel.CellSelector is null) return;
            if (_viewModel.FunctionTracker is null) return;
            var cellContentEditWindowViewModel = new CellContentEditWindowViewModel(_viewModel.CellSelector.SelectedCells, _viewModel.FunctionTracker);
            if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) _viewModel.DockToolWindow(cellContentEditWindowViewModel, Dock.Top);
            else _viewModel.ShowToolWindow(cellContentEditWindowViewModel);
        }

        private void ShowCollectionManagerButtonClick(object sender, RoutedEventArgs e)
        {
            if (_viewModel is null) return;
            if (_viewModel.UserCollectionTracker is null) return;
            if (_viewModel.FunctionTracker is null) return;
            var collectionManagerViewModel = new CollectionManagerWindowViewModel(_viewModel.UserCollectionTracker, _viewModel.FunctionTracker);
            _viewModel.ShowToolWindow(collectionManagerViewModel);
        }

        private void ShowFunctionManagerButtonClick(object sender, RoutedEventArgs e)
        {
            if (_viewModel is null) return;
            var functionLoader = _viewModel.FunctionTracker;
            if (functionLoader is null) return;
            var functionManagerViewModel = new FunctionManagerWindowViewModel(functionLoader);
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) _viewModel.DockToolWindow(functionManagerViewModel, Dock.Right);
            else _viewModel.ShowToolWindow(functionManagerViewModel);
        }

        private void ShowSettingsWindowButtonClick(object sender, RoutedEventArgs e)
        {
            if (_viewModel is null) return;
            var settingsWindowViewModel = new SettingsWindowViewModel();
            _viewModel.ShowToolWindow(settingsWindowViewModel);
        }

        private void ShowSheetManagerButtonClick(object sender, RoutedEventArgs e)
        {
            if (_viewModel is null) return;
            if (_viewModel.SheetTracker is null) return;
            if (_viewModel.DialogFactory is null) return;
            var sheetManagerViewModel = new SheetManagerWindowViewModel(_viewModel.SheetTracker, _viewModel.DialogFactory);
            _viewModel.ShowToolWindow(sheetManagerViewModel);
        }

        private void OpenToolWindowInFloatingContainer(ResizableToolWindow resizableToolWindow)
        {
            var toolbox = new FloatingToolWindowContainer(_viewModel);
            if (resizableToolWindow.ToolViewModel.X < 0) resizableToolWindow.ToolViewModel.X = (_toolWindowCanvas.ActualWidth / 2) - (resizableToolWindow.ToolViewModel.DefaultWidth / 2);
            if (resizableToolWindow.ToolViewModel.Y < 0) resizableToolWindow.ToolViewModel.Y = (_toolWindowCanvas.ActualHeight / 2) - (resizableToolWindow.ToolViewModel.DefaultHeight / 2);
            toolbox.ToolWindowContent = resizableToolWindow;
            _toolWindowCanvas.Children.Add(toolbox);
        }

        private bool IsWindowOfTypeOpen(Type type)
        {
            foreach (var floatingToolWindow in _toolWindowCanvas.Children.OfType<FloatingToolWindowContainer>())
            {
                if (floatingToolWindow.ContentHost.Content.GetType() == type)
                {
                    return true;
                }
            }
            foreach (var dockedToolWindow in _toolWindowDockPanel.Children.OfType<DockedToolWindowContainer>())
            {
                if (dockedToolWindow.ContentHost.Content.GetType() == type)
                {
                    return true;
                }
            }
            return false;
        }

        private void ShowToolWindowInDockedContainer(ResizableToolWindow resizableToolWindow, Dock dockSide)
        {
            var toolbox = new DockedToolWindowContainer(_viewModel)
            {
                ToolWindowContent = resizableToolWindow
            };
            DockPanel.SetDock(toolbox, dockSide);
            _toolWindowDockPanel.Children.Insert(_toolWindowDockPanel.Children.Count - 1, toolbox);
        }

        private void ToggleEditPanelButtonClick(object sender, RoutedEventArgs e)
        {
            if (_viewModel?.SheetViewModel is null) return;
            if (_viewModel.CellTracker is null) return;
            if (_viewModel.FunctionTracker is null) return;
            var viewModel = new CellFormatEditWindowViewModel(_viewModel.SheetViewModel.CellSelector.SelectedCells, _viewModel.CellTracker, _viewModel.FunctionTracker);
            _viewModel.ShowToolWindow(viewModel);
        }

        private void UpdateToolWindowLocation()
        {
            foreach (var toolWindow in _toolWindowCanvas.Children.Cast<FloatingToolWindowContainer>())
            {
                toolWindow.HandleOwningCanvasSizeChanged();
            }
        }

        private void WindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_viewModel is null) return;
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
                    ThemeColorConverter.IsDarkMode = !ThemeColorConverter.IsDarkMode;
                    var activeSheet = _viewModel.SheetViewModel;
                    CloseAllSheetViews();
                    ShowSheetView(activeSheet);
                }
                else if (e.Key == Key.Q)
                {
                    ShowDockSites();
                }
                else if (e.Key == Key.W)
                {
                    HideDockSites();
                }
            }
            else if (e.Key == Key.Tab)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift) _viewModel.CellSelector?.MoveSelectionLeft();
                else _viewModel.CellSelector?.MoveSelectionRight();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift) _viewModel.CellSelector?.MoveSelectionUp();
                else _viewModel.CellSelector?.MoveSelectionDown();
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                _viewModel.CellSelector?.MoveSelectionUp();
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                _viewModel.CellSelector?.MoveSelectionDown();
                e.Handled = true;
            }
            else if (e.Key == Key.Left)
            {
                _viewModel.CellSelector?.MoveSelectionLeft();
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                _viewModel.CellSelector?.MoveSelectionRight();
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
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Maximized) BorderThickness = new Thickness(7.5);
            else BorderThickness = new Thickness(0);
        }
    }
}
