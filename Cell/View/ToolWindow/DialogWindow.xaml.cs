using Cell.View.Application;
using Cell.ViewModel.ToolWindow;

namespace Cell.View.ToolWindow
{
    public partial class DialogWindow : ResizableToolWindow
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DialogWindow"/>.
        /// </summary>
        /// <param name="viewModel">The view model for this view.</param>
        public DialogWindow(DialogWindowViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }
    }
}
