using Cell.ViewModel.ToolWindow;

namespace Cell.View.ToolWindow
{
    public partial class CellSettingsEditWindow : ResizableToolWindow
    {
        public CellSettingsEditWindow(CellSettingsEditWindowViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }

        public override double MinimumHeight => 200;

        public override double MinimumWidth => 200;
    }
}
