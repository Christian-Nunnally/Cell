using Cell.Persistence;
using Cell.View.Cells;
using Cell.View.ToolWindow;
using Cell.ViewModel.Application;
using Cell.ViewModel.Cells;
using Cell.ViewModel.ToolWindow;
using ICSharpCode.AvalonEdit.Editing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Cell.View.Application
{
    public partial class ApplicationView : Window
    {
        private readonly Dictionary<SheetViewModel, SheetView> _sheetViews = [];

        public ApplicationView()
        {
            InitializeComponent();

            ShowSheetView(ApplicationViewModel.Instance.SheetViewModel);
        }

        public SheetView? ActiveSheetView { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Binding")]
        public ApplicationSettings ApplicationSettings => ApplicationSettings.Instance;

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
            DataContext = ApplicationViewModel.GetOrCreateInstance(this);
            base.OnInitialized(e);
            PersistenceManager.LoadAll();
            ApplicationViewModel.Instance.SheetViewModel.LoadCellViewModels();
            ApplicationViewModel.Instance.PropertyChanged += ApplicationViewModelPropertyChanged;
        }

        private void ApplicationViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ApplicationViewModel.ApplicationWindowWidth) || e.PropertyName == nameof(ApplicationViewModel.ApplicationWindowHeight))
            {
                UpdateToolWindowLocation();
            }
        }

        private void UpdateToolWindowLocation()
        {
            foreach (var toolWindow in _toolWindowCanvas.Children.Cast<FloatingToolWindow>())
            {
                toolWindow.UpdateSizeAndPositionRespectingBounds();
            }
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
            editPanel.SetBinding(DataContextProperty, new Binding("SheetViewModel.SelectedCellViewModel") { Source = ApplicationViewModel.Instance });
            ShowToolWindow(editPanel);
        }

        private void OpenTextEditPanelButtonClick(object sender, RoutedEventArgs e)
        {
            var editPanel = new CellContentEditWindow();
            editPanel.SetBinding(DataContextProperty, new Binding("SheetViewModel.SelectedCellViewModel") { Source = ApplicationViewModel.Instance });
            ShowToolWindow(editPanel);
        }

        private void ShowCollectionManagerButtonClick(object sender, RoutedEventArgs e)
        {
            var collectionManagerViewModel = new CollectionManagerWindowViewModel(UserCollectionLoader.ObservableCollections);
            var collectionManager = new CollectionManagerWindow(collectionManagerViewModel);
            ShowToolWindow(collectionManager);
        }

        private void ShowFunctionManagerButtonClick(object sender, RoutedEventArgs e)
        {
            var functionManagerViewModel = new FunctionManagerWindowViewModel(PluginFunctionLoader.ObservableFunctions);
            var functionManager = new FunctionManagerWindow(functionManagerViewModel);
            ShowToolWindow(functionManager);
        }

        private void ShowSheetManagerButtonClick(object sender, RoutedEventArgs e)
        {
            var sheetManagerViewModel = new SheetManagerWindowViewModel();
            var sheetManager = new SheetManagerWindow(sheetManagerViewModel);
            ShowToolWindow(sheetManager);
        }

        private void ShowSettingsWindowButtonClick(object sender, RoutedEventArgs e)
        {
            var settingsWindowViewModel = new SettingsWindowViewModel();
            var settingsWindow = new SettingsWindow(settingsWindowViewModel);
            ShowToolWindow(settingsWindow);
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
            var editPanel = new CellFormatEditWindow();
            editPanel.SetBinding(DataContextProperty, new Binding("SheetViewModel.SelectedCellViewModel") { Source = ApplicationViewModel.Instance });
            ShowToolWindow(editPanel);
        }

        private void WindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            ApplicationViewModel.Instance.SheetViewModel.LastKeyPressed = e.Key.ToString();
            if (Mouse.DirectlyOver is TextArea || Mouse.DirectlyOver is TextBox || Keyboard.FocusedElement is TextArea || Keyboard.FocusedElement is TextBox) return; // Disable keyboard shortcuts when typing in a textbox
            if (e.IsDown && (Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                if (e.Key == Key.C)
                {
                    ApplicationViewModel.Instance.CopySelectedCells((Keyboard.Modifiers & ModifierKeys.Shift) == 0);
                    e.Handled = true;
                }
                else if (e.Key == Key.V)
                {
                    ApplicationViewModel.Instance.PasteCopiedCells();
                    e.Handled = true;
                }
                else if (e.Key == Key.Z)
                {
                    UndoRedoManager.Undo();
                    e.Handled = true;
                }
                else if (e.Key == Key.Y)
                {
                    UndoRedoManager.Redo();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Tab)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift) ApplicationViewModel.Instance.SheetViewModel.MoveSelectionLeft();
                else ApplicationViewModel.Instance.SheetViewModel.MoveSelectionRight();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift) ApplicationViewModel.Instance.SheetViewModel.MoveSelectionUp();
                else ApplicationViewModel.Instance.SheetViewModel.MoveSelectionDown();
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                ApplicationViewModel.Instance.SheetViewModel.MoveSelectionUp();
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                ApplicationViewModel.Instance.SheetViewModel.MoveSelectionDown();
                e.Handled = true;
            }
            else if (e.Key == Key.Left)
            {
                ApplicationViewModel.Instance.SheetViewModel.MoveSelectionLeft();
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                ApplicationViewModel.Instance.SheetViewModel.MoveSelectionRight();
                e.Handled = true;
            }
            else if (e.Key == Key.Delete)
            {
                if (ApplicationViewModel.Instance.SheetViewModel.SelectedCellViewModel == null) return;
                ApplicationViewModel.Instance.SheetViewModel.SelectedCellViewModel.Text = "";
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                ApplicationViewModel.Instance.SheetViewModel.UnselectAllCells();
                e.Handled = true;
            }
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Maximized) BorderThickness = new Thickness(8);
            else BorderThickness = new Thickness(0);
        }
    }
}
