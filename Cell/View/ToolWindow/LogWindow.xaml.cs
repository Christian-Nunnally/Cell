using Cell.Common;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows.Controls;

namespace Cell.View.ToolWindow
{
    public partial class LogWindow : UserControl, IResizableToolWindow
    {
        private readonly LogWindowViewModel _viewModel;
        public LogWindow(LogWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel;
            InitializeComponent();
        }

        public double MinimumHeight => 400;

        public double MinimumWidth => 400;

        public Action? RequestClose { get; set; }

        public List<CommandViewModel> ToolBarCommands => [
            new("Clear", new RelayCommand(x => _viewModel.ClearBuffer()))
        ];

        public string ToolWindowTitle => "Logs";

        public ToolWindowViewModel ToolViewModel => _viewModel;

        public void HandleBeingClosed() => _viewModel.HandleBeingShown();

        public void HandleBeingShown() => _viewModel.HandleBeingClosed();

        public bool HandleCloseRequested() => true;
    }
}
