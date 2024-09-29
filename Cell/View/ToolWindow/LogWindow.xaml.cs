using Cell.Common;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;

namespace Cell.View.ToolWindow
{
    public partial class LogWindow : ResizableToolWindow
    {
        public LogWindow(LogWindowViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }
    }
}
