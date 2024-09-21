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

        public Action? RequestClose { get; set; }

        public double GetMinimumHeight() => 200;

        public string GetTitle() => "Example";

        public List<CommandViewModel> GetToolBarCommands()
        {
            return
            [
                new("Example", () => {})
            ];
        }

        public double GetMinimumWidth() => 200;

        public bool HandleCloseRequested()
        {
            return true;
        }

        public void HandleBeingClosed()
        {
        }

        public void HandleBeingShown()
        {
        }
    }
}
