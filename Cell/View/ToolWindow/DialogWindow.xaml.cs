using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows;

namespace Cell.View.ToolWindow
{
    public partial class DialogWindow : ResizableToolWindow
    {
        public DialogWindow(DialogWindowViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }
    }
}
