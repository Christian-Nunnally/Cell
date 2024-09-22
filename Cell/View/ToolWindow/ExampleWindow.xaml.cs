using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows.Controls;

namespace Cell.View.ToolWindow
{
    public partial class ExampleWindow : UserControl, IResizableToolWindow
    {
        private readonly ExampleWindowViewModel _viewModel;
        public ExampleWindow(ExampleWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = _viewModel;
            InitializeComponent();
        }

        public double MinimumHeight => 200;

        public double MinimumWidth => 200;

        public Action? RequestClose { get; set; }

        public List<CommandViewModel> ToolBarCommands => [
            new("Example", () => {})
        ];

        public string ToolWindowTitle => "Example";

        public ToolWindowViewModel ToolViewModel => _viewModel;

        public void HandleBeingClosed()
        {
            _viewModel.HandleBeingShown();
        }

        public void HandleBeingShown()
        {
            _viewModel.HandleBeingClosed();
        }

        public bool HandleCloseRequested()
        {
            return true;
        }
    }
}
