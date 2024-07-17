using Cell.Persistence;
using Cell.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cell.View
{
    public partial class MainWindow : Window
    {
        public SheetView? SheetView;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            DataContext = ApplicationViewModel.GetOrCreateInstance(this);
            base.OnInitialized(e);
            PersistenceManager.LoadAll();
            ApplicationViewModel.Instance.SheetViewModel.LoadCellViewModels();
        }

        private void TitleBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;
            if (e.ClickCount == 2) AdjustWindowSize();
            else Application.Current.MainWindow.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            AdjustWindowSize();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void AdjustWindowSize()
        {
            if (WindowState == WindowState.Maximized) WindowState = WindowState.Normal;
            else WindowState = WindowState.Maximized;

        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Maximized) BorderThickness = new Thickness(8);
            else BorderThickness = new Thickness(0);
        }

        private void WindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            ApplicationViewModel.Instance.SheetViewModel.LastKeyPressed = e.Key.ToString();
            if (e.IsDown && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.C)
                {
                    ApplicationViewModel.Instance.CopySelectedCells();
                }
                else if (e.Key == Key.V)
                {
                    ApplicationViewModel.Instance.PasteCopiedCells();
                }
            }
        }

        private void OnCodeEditorLoaded(object sender, RoutedEventArgs e)
        {
            CodeEditorViewModel.SetCodeEditorView((CodeEditor)sender);
        }

        private void OnSheetViewLoaded(object sender, RoutedEventArgs e)
        {
            SheetView = (SheetView)sender;
        }

        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textbox) textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }
    }
}