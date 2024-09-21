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

        public Action? RequestClose { get; set; }

        public double GetMinimumHeight() => 150;

        public double GetMinimumWidth() => 300;

        public string GetTitle() => "Create New Sheet";

        public List<CommandViewModel> GetToolBarCommands()
        {
            return
            [
            ];
        }

        public void HandleBeingClosed()
        {
        }

        public void HandleBeingShown()
        {
        }

        public bool HandleCloseRequested()
        {
            return true;
        }

        private void AddNewSheetButtonClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.AddNewSheet();
            RequestClose?.Invoke();
        }
    }
}
