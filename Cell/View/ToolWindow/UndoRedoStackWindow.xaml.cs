using Cell.Common;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;

namespace Cell.View.ToolWindow
{
    public partial class UndoRedoStackWindow : ResizableToolWindow
    {
        public UndoRedoStackWindow(UndoRedoStackWindowViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }

        public override List<CommandViewModel> ToolBarCommands =>
        [
            new("Undo", new RelayCommand(x => ApplicationViewModel.GetUndoRedoManager()?.Undo())),
            new("Redo", new RelayCommand(x => ApplicationViewModel.GetUndoRedoManager()?.Redo()))
        ];
    }
}
