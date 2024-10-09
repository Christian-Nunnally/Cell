using Cell.ViewModel.ToolWindow;

namespace Cell.View.ToolWindow
{
    public partial class LogWindow : ResizableToolWindow
    {
        /// <summary>
        /// Creates a new instance of the <see cref="LogWindow"/>.
        /// </summary>
        /// <param name="viewModel">The view model for this view.</param>
        public LogWindow(LogWindowViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }
    }
}
