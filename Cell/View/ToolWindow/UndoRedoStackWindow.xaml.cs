using Cell.ViewModel.ToolWindow;

namespace Cell.View.ToolWindow
{
    public partial class UndoRedoStackWindow : ResizableToolWindow
    {
        /// <summary>
        /// Creates a new instance of the <see cref="UndoRedoStackWindow"/> class.
        /// </summary>
        /// <param name="viewModel">The view model for the view.</param>
        public UndoRedoStackWindow(UndoRedoStackWindowViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }
    }
}
