using Cell.Persistence;
using Cell.ViewModel;
using ICSharpCode.AvalonEdit.Editing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cell.View
{
    public partial class ApplicationView : Window
    {
        public SheetView? SheetView;

        public ApplicationView()
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

        private void OnCloseButtonClicked(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MaximizeButtonClick(object sender, RoutedEventArgs e)
        {
            AdjustWindowSize();
        }

        private void MinimizeButtonClick(object sender, RoutedEventArgs e)
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
            if (Mouse.DirectlyOver is TextArea || Mouse.DirectlyOver is TextBox || Keyboard.FocusedElement is TextArea || Keyboard.FocusedElement is TextBox) return; // Disable keyboard shortcuts when typing in a textbox
            if (e.IsDown && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.C)
                {
                    ApplicationViewModel.Instance.CopySelectedCells();
                    e.Handled = true;
                }
                else if (e.Key == Key.V)
                {
                    ApplicationViewModel.Instance.PasteCopiedCells();
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
        }

        private void ToggleEditPanelButtonClick(object sender, RoutedEventArgs e)
        {
            ApplicationViewModel.Instance.ToggleEditingPanels();
        }

        private void OnCodeEditorLoaded(object sender, RoutedEventArgs e)
        {
            CodeEditorViewModel.SetCodeEditorView((CodeEditor)sender);
        }

        private void OnSheetViewLoaded(object sender, RoutedEventArgs e)
        {
            SheetView = (SheetView)sender;
        }

        private void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
            {
                if (e.Key == Key.Enter && sender is TextBox textbox) textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = true;
            }
        }
    }
}