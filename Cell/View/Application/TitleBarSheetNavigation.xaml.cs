using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
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
        /// <summary>
        /// Creates a new instance of the <see cref="TitleBarSheetNavigation"/>.
        /// </summary>
        public TitleBarSheetNavigation()
        {
            InitializeComponent();
        }

        private void AddNewSheetButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ApplicationViewModel.Instance.SheetTracker is null) return;
            if (ApplicationViewModel.Instance.DialogFactory is null) return;
            var createSheetWindowViewModel = new CreateSheetWindowViewModel(ApplicationViewModel.Instance.SheetTracker, ApplicationViewModel.Instance.DialogFactory);
            ApplicationViewModel.Instance.ShowToolWindow(createSheetWindowViewModel);
        }

        private async void GoToSheetButtonClickedAsync(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.Content is not Label label) return;
            if (label.Content is not string sheetName) return;
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                var sheetToolWindowViewModel = new SheetToolWindowViewModel();
                ApplicationViewModel.Instance.ShowToolWindow(sheetToolWindowViewModel);
                sheetToolWindowViewModel.SheetViewModel = ApplicationViewModel.Instance.SheetViewModel;
            }
            else
            {
                await ApplicationViewModel.Instance.GoToSheetAsync(sheetName);
            }
        }
    }
}
