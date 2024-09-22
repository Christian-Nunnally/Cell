using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows;
using System.Windows.Controls;

namespace Cell.View.ToolWindow
{
    public partial class CreateSheetWindow : UserControl, IResizableToolWindow
    {
        private readonly CreateSheetWindowViewModel _viewModel;
        public CreateSheetWindow(CreateSheetWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel;
            InitializeComponent();
        }

        public double MinimumHeight => 150;

        public double MinimumWidth => 300;

        public Action? RequestClose { get; set; }

        public List<CommandViewModel> ToolBarCommands => [];

        public string ToolWindowTitle => "Create New Sheet";

        public ToolWindowViewModel ToolViewModel => _viewModel;

        public void HandleBeingClosed()
        {
            _viewModel.HandleBeingShown();
        }

        public void HandleBeingShown()
        {
            _viewModel.HandleBeingClosed();
        }

        public bool HandleCloseRequested() => true;

        private void AddNewSheetButtonClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.AddNewSheet();
            RequestClose?.Invoke();
        }
    }
}
