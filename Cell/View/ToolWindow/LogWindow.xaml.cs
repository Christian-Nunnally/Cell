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

        public Action? RequestClose { get; set; }

        public double GetMinimumHeight() => 400;

        public double GetMinimumWidth() => 400;

        public string GetTitle() => "Logs";

        public List<CommandViewModel> GetToolBarCommands()
        {
            return
            [
                new("Clear", new RelayCommand(x => _viewModel.ClearBuffer()))
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
    }
}
