using Cell.Common;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;

namespace Cell.View.ToolWindow
{
    public partial class LogWindow : ResizableToolWindow
    {
        private LogWindowViewModel LogWindowViewModel => (LogWindowViewModel)ToolViewModel;
        public LogWindow(LogWindowViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }

        public override List<CommandViewModel> ToolBarCommands => [
            new("Clear", new RelayCommand(x => LogWindowViewModel.ClearBuffer()))
        ];
    }
}
