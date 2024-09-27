using Cell.ViewModel.ToolWindow;
using System.Windows;

namespace Cell.View.ToolWindow
{
    public partial class CreateSheetWindow : ResizableToolWindow
    {
        private CreateSheetWindowViewModel CreateSheetWindowViewModel => (CreateSheetWindowViewModel)ToolViewModel;
        public CreateSheetWindow(CreateSheetWindowViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }

        private void AddNewSheetButtonClicked(object sender, RoutedEventArgs e)
        {
            CreateSheetWindowViewModel.AddNewSheet();
            RequestClose?.Invoke();
        }
    }
}
