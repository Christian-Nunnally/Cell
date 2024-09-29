using Cell.ViewModel.ToolWindow;

namespace Cell.View.ToolWindow
{
    public partial class UndoRedoStackWindow : ResizableToolWindow
    {
        public UndoRedoStackWindow(UndoRedoStackWindowViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }
    }
}
