using Cell.Model;
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
                foreach (var child in _toolWindowCanvas.Children.Cast<UIElement>())
                {
                    if (child is FloatingToolWindow floatingToolWindow && floatingToolWindow.ContentHost.Content.GetType() == content.GetType())
                    {
                        return;
                    }
                }
            }

            var toolbox = new FloatingToolWindow(_toolWindowCanvas);
            toolbox.SetContent(content);

            Canvas.SetLeft(toolbox, 100);
            Canvas.SetTop(toolbox, 100 + _toolWindowCanvas.Children.Cast<UIElement>().Count() * 200);

            //foreach (var child in _toolWindowCanvas.Children.Cast<UIElement>())
            //{
            //    var currentSize = child.DesiredSize;
            //    var x = Canvas.GetLeft(child);
            //    var y = Canvas.GetTop(child);
            //}

            _toolWindowCanvas.Children.Add(toolbox);
        }

        protected override void OnInitialized(EventArgs e)
        {
            DataContext = ApplicationViewModel.GetOrCreateInstance(this);
            base.OnInitialized(e);
            PersistenceManager.LoadAll();
            ApplicationViewModel.Instance.SheetViewModel.LoadCellViewModels();
        }

        private void AddNewSheetButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ApplicationViewModel.Instance.IsAddingSheet)
            {
                if (!string.IsNullOrEmpty(ApplicationViewModel.Instance.NewSheetName))
                {
                    ApplicationViewModel.Instance.GoToSheet(ApplicationViewModel.Instance.NewSheetName);
                }
                ApplicationViewModel.Instance.NewSheetName = string.Empty;
                ApplicationViewModel.Instance.IsAddingSheet = false;
            }
            else
            {
                ApplicationViewModel.Instance.IsAddingSheet = true;
                ApplicationViewModel.Instance.NewSheetName = "Untitled";
            }
        }

        private void AdjustWindowSize()
        {
            if (WindowState == WindowState.Maximized) WindowState = WindowState.Normal;
            else WindowState = WindowState.Maximized;
        }

        private void GoToSheetButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.Content is not Label label) return;
            if (label.Content is not string sheetName) return;
            ApplicationViewModel.Instance.GoToSheet(sheetName);
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

        private void ShowHelpButtonClick(object sender, RoutedEventArgs e)
        {
            var helpWindow = new HelpWindow();
            helpWindow.SetBinding(DataContextProperty, new Binding("SheetViewModel.SelectedCellViewModel") { Source = ApplicationViewModel.Instance });
            ShowToolWindow(helpWindow);
        }

        private void ShowLogWindowButtonClick(object sender, RoutedEventArgs e)
        {
            var logWindowViewModel = new LogWindowViewModel();
            var logWindow = new LogWindow(logWindowViewModel);
            ShowToolWindow(logWindow);
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
            ApplicationViewModel.Instance.ToggleEditingPanels();
            var editPanel = new CellFormatEditWindow();
            editPanel.SetBinding(DataContextProperty, new Binding("SheetViewModel.SelectedCellViewModel") { Source = ApplicationViewModel.Instance });
            ShowToolWindow(editPanel);
        }

        private void TogglePanLockButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (ActiveSheetView is null) return;
            if (ActiveSheetView.PanAndZoomCanvas is null) return;
            ActiveSheetView.PanAndZoomCanvas.PanCanvasTo(CellModelFactory.DefaultCellWidth, CellModelFactory.DefaultCellHeight);
            ActiveSheetView.PanAndZoomCanvas.ZoomCanvasTo(new Point(0, 0), 1);
            ActiveSheetView.PanAndZoomCanvas.IsPanningEnabled = !ActiveSheetView.PanAndZoomCanvas.IsPanningEnabled;
            button.Content = ActiveSheetView.PanAndZoomCanvas.IsPanningEnabled ? "🔓" : "🔒";
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

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
    }
}
