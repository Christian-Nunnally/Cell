using Cell.Model;
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

        private ApplicationViewModel? application;

        public CodeEditor? CodeEditor;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            application = ApplicationViewModel.GetOrCreateInstance(this);
            DataContext = application;
            base.OnInitialized(e);
            PersistenceManager.LoadAll();
            application.SheetViewModel.LoadCellViewModels();
        }

        public void ChangeSheet(string sheetName)
        {
            application?.GoToSheet(sheetName);
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
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

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Maximized) BorderThickness = new System.Windows.Thickness(8);
            else BorderThickness = new System.Windows.Thickness(0);
        }

        private void CreateNewColumnToTheLeftButtonClicked(object sender, RoutedEventArgs e)
        {
            if (Utilities.TryGetSendersDataContext<ColumnCellViewModel>(sender, out var cell))
            {
                cell.AddColumnToTheLeft();
            }
        }

        private void CreateNewColumnToTheRightButtonClicked(object sender, RoutedEventArgs e)
        {
            if (Utilities.TryGetSendersDataContext<ColumnCellViewModel>(sender, out var cell))
            {
                cell.AddColumnToTheRight();
            }
        }

        private void CreateNewRowAboveButtonClicked(object sender, RoutedEventArgs e)
        {
            if (Utilities.TryGetSendersDataContext<RowCellViewModel>(sender, out var cell))
            {
                cell.AddRowAbove();
            }
        }

        private void CreateNewRowBelowButtonClicked(object sender, RoutedEventArgs e)
        {
            if (Utilities.TryGetSendersDataContext<RowCellViewModel>(sender, out var cell))
            {
                cell.AddRowBelow();
            }
        }

        private void ChangeCellTypeButtonPressed(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Content is string buttonContent && Enum.TryParse(typeof(CellType), buttonContent, out var newType) && newType is CellType cellType)
            {
                application?.ChangeSelectedCellsType(cellType);
            }
        }

        private void DeleteRowButtonClicked(object sender, RoutedEventArgs e)
        {
            if (SheetView is null) return;
            foreach (var cell in SheetView.SheetViewModel.SelectedCellViewModels.OfType<RowCellViewModel>().ToList())
            {
                cell.DeleteRow();
            }
        }

        private void DeleteColumnButtonClicked(object sender, RoutedEventArgs e)
        {
            if (SheetView is null) return;
            foreach (var cell in SheetView.SheetViewModel.SelectedCellViewModels.OfType<ColumnCellViewModel>().ToList())
            {
                cell.DeleteColumn();
            }
        }

        private void EditGetTextFunctionButtonClicked(object sender, RoutedEventArgs e)
        {
            if (Utilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell))
            {
                if (string.IsNullOrEmpty(cell.GetTextFunctionName)) cell.GetTextFunctionName = "Untitled";
                CodeEditor?.Show(cell.GetTextFunctionCode, x => cell.GetTextFunctionCode = x);
            }
        }

        private void EditOnEditFunctionButtonClicked(object sender, RoutedEventArgs e)
        {
            if (Utilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell))
            {
                if (string.IsNullOrEmpty(cell.OnEditFunctionName)) cell.OnEditFunctionName = "Untitled";
                CodeEditor?.Show(cell.OnEditFunctionCode, x => cell.OnEditFunctionCode = x);
            }
        }

        private void OnCodeEditorLoaded(object sender, RoutedEventArgs e)
        {
            CodeEditor = (CodeEditor)sender;
        }

        private void CreateBackupButtonClicked(object sender, RoutedEventArgs e)
        {
            PersistenceManager.CreateBackup();
        }

        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textbox)
            {
                textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void WindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            ApplicationViewModel.Instance.SheetViewModel.LastKeyPressed = e.Key.ToString();
            if (e.IsDown && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.C)
                {
                    application?.CopySelectedCells();
                }
                else if (e.Key == Key.V)
                {
                    application?.PasteCopiedCells();
                }
            }
        }

        private void OnSheetViewLoaded(object sender, RoutedEventArgs e)
        {
            SheetView = (SheetView)sender;
        }
    }
}