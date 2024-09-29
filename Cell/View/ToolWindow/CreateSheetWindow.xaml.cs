using Cell.ViewModel.ToolWindow;
using System.Windows;

namespace Cell.View.ToolWindow
{
    public partial class CreateSheetWindow : ResizableToolWindow
    {
        private CreateSheetWindowViewModel _viewModel;
        public CreateSheetWindow(CreateSheetWindowViewModel viewModel) : base(viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
        }

        private void AddNewSheetButtonClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.AddNewSheet();
        }
    }
}
