using Cell.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cell.View
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

        private void AddNewSheetButtonClicked(object sender, RoutedEventArgs e)
        {
            if (isRenameingSheet)
            {
                isRenameingSheet = false;
                ApplicationViewModel.Instance.IsAddingSheet = false;
                ApplicationViewModel.Instance.RenameSheet(sheetBeingRenamed, ApplicationViewModel.Instance.NewSheetName);
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
    }
}
