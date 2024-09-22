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

        public override double MinimumHeight => 150;

        public override double MinimumWidth => 300;

        private void AddNewSheetButtonClicked(object sender, RoutedEventArgs e)
        {
            CreateSheetWindowViewModel.AddNewSheet();
            RequestClose?.Invoke();
        }
    }
}
