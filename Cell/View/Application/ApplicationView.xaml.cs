using Cell.Data;
using Cell.Execution;
using Cell.Persistence;
using Cell.Persistence.Migration;
using Cell.View.Cells;
using Cell.View.ToolWindow;
using Cell.ViewModel.Application;
using Cell.ViewModel.Cells;
using Cell.ViewModel.ToolWindow;
using ICSharpCode.AvalonEdit.Editing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace Cell.View.Application
{
    public partial class ApplicationView : Window
    {
        private readonly Dictionary<SheetViewModel, SheetView> _sheetViews = [];
        private ApplicationViewModel? _viewModel;
        public ApplicationView()
        {
            InitializeComponent();
            if (DataContext is ApplicationViewModel viewModel) viewModel.PropertyChanged += ApplicationViewModelPropertyChanged;
            DataContextChanged += ApplicationViewDataContextChanged;
        }

        private void ApplicationViewDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ApplicationViewModel oldViewModel) oldViewModel.PropertyChanged -= ApplicationViewModelPropertyChanged;
            if (e.NewValue is ApplicationViewModel newViewModel) newViewModel.PropertyChanged += ApplicationViewModelPropertyChanged;
        }

        public SheetView? ActiveSheetView { get; set; }

        public void ApplicationViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ApplicationViewModel.ApplicationWindowWidth) || e.PropertyName == nameof(ApplicationViewModel.ApplicationWindowHeight))
            {
                UpdateToolWindowLocation();
            }
        }

        public void ShowSheetView(SheetViewModel sheetViewModel)
        {
            if (sheetViewModel == null) return;
            if (_sheetViews.TryGetValue(sheetViewModel, out var sheetView))
            {
                _sheetViewContentControl.Content = sheetView;
                ActiveSheetView = sheetView;
            }
            else
            {
                sheetView = new SheetView
                {
                    DataContext = sheetViewModel
                };
                _sheetViewContentControl.Content = sheetView;
                ActiveSheetView = sheetView;
                _sheetViews.Add(sheetViewModel, sheetView);
            }
        }

        public void ShowToolWindow(UserControl content, bool allowDuplicates = false)
        {
            if (!allowDuplicates)
            {
                foreach (var floatingToolWindow in _toolWindowCanvas.Children.Cast<FloatingToolWindow>())
                {
                    if (floatingToolWindow.ContentHost.Content.GetType() == content.GetType())
                    {
                        return;
                    }
                }
            }

            var toolbox = new FloatingToolWindow(_toolWindowCanvas);
            toolbox.SetContent(content);

            Canvas.SetLeft(toolbox, (_toolWindowCanvas.ActualWidth / 2) - (toolbox.ContentWidth / 2));
            Canvas.SetTop(toolbox, (_toolWindowCanvas.ActualHeight / 2) - (toolbox.ContentHeight / 2));

            _toolWindowCanvas.Children.Add(toolbox);
        }

        protected override void OnInitialized(EventArgs e)
        {
            var appDataPath = Environment.SpecialFolder.ApplicationData;
            var appDataRoot = Environment.GetFolderPath(appDataPath);
            var savePath = Path.Combine(appDataRoot, "LGF", "Cell");
            var fileIo = new FileIO();
            var persistenceManager = new PersistenceManager(savePath, fileIo);

            var v0ToV1Migration = new V1ToV2Migrator();
            persistenceManager.RegisterMigrator("0.0.0", "1", v0ToV1Migration);

            var pluginFunctionLoader = new PluginFunctionLoader(persistenceManager);
            var cellLoader = new CellLoader(persistenceManager);
            var cellTracker = new CellTracker(cellLoader);
            var userCollectionLoader = new UserCollectionLoader(persistenceManager, pluginFunctionLoader, cellTracker);
            var cellPopulateManager = new CellPopulateManager(cellTracker, pluginFunctionLoader, userCollectionLoader);
            var cellTriggerManager = new CellTriggerManager(cellTracker, pluginFunctionLoader, userCollectionLoader);
            var sheetTracker = new SheetTracker(persistenceManager, cellLoader, cellTracker, pluginFunctionLoader, userCollectionLoader);
            var titleBarSheetNavigationViewModel = new TitleBarSheetNavigationViewModel(sheetTracker);
            var applicationSettings = ApplicationSettings.CreateInstance(persistenceManager);
            var undoRedoManager = new UndoRedoManager(cellTracker);
            var textClipboard = new TextClipboard();
            var cellClipboard = new CellClipboard(undoRedoManager, cellTracker, textClipboard);
            var backupManager = new BackupManager(persistenceManager, cellTracker, sheetTracker, userCollectionLoader, pluginFunctionLoader);

            _viewModel = new ApplicationViewModel(persistenceManager, pluginFunctionLoader, cellLoader, cellTracker, userCollectionLoader, cellPopulateManager, cellTriggerManager, sheetTracker, titleBarSheetNavigationViewModel, applicationSettings, undoRedoManager, cellClipboard, backupManager);
            ApplicationViewModel.Instance = _viewModel;
            _viewModel.AttachToView(this);
            base.OnInitialized(e);
        }

        private void AdjustWindowSize()
        {
            if (WindowState == WindowState.Maximized) WindowState = WindowState.Normal;
            else WindowState = WindowState.Maximized;
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

        private void OpenSpecialEditPanelButtonClick(object sender, RoutedEventArgs e)
        {
            var editPanel = new CellSettingsEditWindow();
            editPanel.SetBinding(DataContextProperty, new Binding("SheetViewModel.SelectedCellViewModel") { Source = _viewModel });
            ShowToolWindow(editPanel);
        }

        private void OpenTextEditPanelButtonClick(object sender, RoutedEventArgs e)
        {
            var editPanel = new CellContentEditWindow();
            editPanel.SetBinding(DataContextProperty, new Binding("SheetViewModel.SelectedCellViewModel") { Source = _viewModel });
            ShowToolWindow(editPanel);
        }

        private void ShowCollectionManagerButtonClick(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            var collectionManagerViewModel = new CollectionManagerWindowViewModel(_viewModel.UserCollectionLoader);
            var collectionManager = new CollectionManagerWindow(collectionManagerViewModel);
            ShowToolWindow(collectionManager);
        }

        private void ShowFunctionManagerButtonClick(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            var functionLoader = _viewModel.PluginFunctionLoader;
            var functionManagerViewModel = new FunctionManagerWindowViewModel(functionLoader);
            var functionManager = new FunctionManagerWindow(functionManagerViewModel);
            ShowToolWindow(functionManager);
        }

        private void ShowSettingsWindowButtonClick(object sender, RoutedEventArgs e)
        {
            var settingsWindowViewModel = new SettingsWindowViewModel();
            var settingsWindow = new SettingsWindow(settingsWindowViewModel);
            ShowToolWindow(settingsWindow);
        }

        private void ShowSheetManagerButtonClick(object sender, RoutedEventArgs e)
        {
            var sheetManagerViewModel = new SheetManagerWindowViewModel();
            var sheetManager = new SheetManagerWindow(sheetManagerViewModel);
            ShowToolWindow(sheetManager);
        }

        private void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
            {
                if (e.Key == Key.Enter && sender is TextBox textbox) textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = true;
            }
        }

        private void ToggleEditPanelButtonClick(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            var editPanel = new CellFormatEditWindow(_viewModel.CellTracker);
            editPanel.SetBinding(DataContextProperty, new Binding("SheetViewModel.SelectedCellViewModel") { Source = _viewModel });
            ShowToolWindow(editPanel);
        }

        private void UpdateToolWindowLocation()
        {
            foreach (var toolWindow in _toolWindowCanvas.Children.Cast<FloatingToolWindow>())
            {
                toolWindow.UpdateSizeAndPositionRespectingBounds();
            }
        }

        private void WindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Mouse.DirectlyOver is TextArea || Mouse.DirectlyOver is TextBox || Keyboard.FocusedElement is TextArea || Keyboard.FocusedElement is TextBox) return; // Disable keyboard shortcuts when typing in a textbox
            if (e.IsDown && (Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                if (e.Key == Key.C)
                {
                    _viewModel?.CopySelectedCells((Keyboard.Modifiers & ModifierKeys.Shift) == 0);
                    e.Handled = true;
                }
                else if (e.Key == Key.V)
                {
                    _viewModel?.PasteCopiedCells();
                    e.Handled = true;
                }
                else if (e.Key == Key.Z)
                {
                    _viewModel?.UndoRedoManager.Undo();
                    e.Handled = true;
                }
                else if (e.Key == Key.Y)
                {
                    _viewModel?.UndoRedoManager.Redo();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Tab)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift) _viewModel?.SheetViewModel?.MoveSelectionLeft();
                else _viewModel?.SheetViewModel?.MoveSelectionRight();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift) _viewModel?.SheetViewModel?.MoveSelectionUp();
                else _viewModel?.SheetViewModel?.MoveSelectionDown();
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                _viewModel?.SheetViewModel?.MoveSelectionUp();
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                _viewModel?.SheetViewModel?.MoveSelectionDown();
                e.Handled = true;
            }
            else if (e.Key == Key.Left)
            {
                _viewModel?.SheetViewModel?.MoveSelectionLeft();
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                _viewModel?.SheetViewModel?.MoveSelectionRight();
                e.Handled = true;
            }
            else if (e.Key == Key.Delete)
            {
                if (_viewModel?.SheetViewModel?.SelectedCellViewModel == null) return;
                _viewModel.SheetViewModel.SelectedCellViewModel.Text = "";
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                _viewModel?.SheetViewModel?.UnselectAllCells();
                e.Handled = true;
            }
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Maximized) BorderThickness = new Thickness(8);
            else BorderThickness = new Thickness(0);
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
    }
}
