using Cell.ViewModel.Application;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cell.View.Application
{
    /// <summary>
    /// Interaction logic for TitleBarSheetNavigation.xaml
    /// </summary>
    public partial class TitleBarSheetNavigation : UserControl
    {
        private bool isRenameingSheet = false;
        private string sheetBeingRenamed = string.Empty;
        public TitleBarSheetNavigation()
        {
            InitializeComponent();
        }

        private void AddNewSheetButtonClicked(object sender, RoutedEventArgs e)
        {
            if (isRenameingSheet)
            {
                isRenameingSheet = false;
                ApplicationViewModel.Instance.IsAddingSheet = false;
                ApplicationViewModel.RenameSheet(sheetBeingRenamed, ApplicationViewModel.Instance.NewSheetName);
                return;
            }
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

        private void GoToSheetButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.Content is not Label label) return;
            if (label.Content is not string sheetName) return;
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                isRenameingSheet = true;
                sheetBeingRenamed = sheetName;
                ApplicationViewModel.Instance.IsAddingSheet = true;
                ApplicationViewModel.Instance.NewSheetName = sheetName;
            }
            else
            {
                ApplicationViewModel.Instance.GoToSheet(sheetName);
            }
        }
    }
}
