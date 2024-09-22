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

        public override double MinimumHeight => 200;

        public override double MinimumWidth => 200;

        public override List<CommandViewModel> ToolBarCommands => [
            new("Example", () => {})
        ];
    }
}
