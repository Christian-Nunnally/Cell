using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;

namespace Cell.View.ToolWindow
{
    public partial class ExampleWindow : ResizableToolWindow
    {
        public ExampleWindow(ExampleWindowViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }

        public override List<CommandViewModel> ToolBarCommands => [
            new("Example", () => {})
        ];
    }
}
