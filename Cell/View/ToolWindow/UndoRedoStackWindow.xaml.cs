using Cell.Common;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows.Controls;

namespace Cell.View.ToolWindow
{
    public partial class UndoRedoStackWindow : UserControl, IResizableToolWindow
    {
        private readonly UndoRedoStackWindowViewModel _viewModel;
        public UndoRedoStackWindow(UndoRedoStackWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = _viewModel;
            InitializeComponent();
        }

        public Action? RequestClose { get; set; }

        public double GetMinimumHeight() => 200;

        public string GetTitle() => "Undo/Redo Stack";

        public List<CommandViewModel> GetToolBarCommands()
        {
            return
            [
                new("Undo", new RelayCommand(x => ApplicationViewModel.GetUndoRedoManager()?.Undo())),
                new("Redo", new RelayCommand(x => ApplicationViewModel.GetUndoRedoManager()?.Redo()))
            ];
        }

        public double GetMinimumWidth() => 200;

        public bool HandleCloseRequested() => true;

        public void HandleBeingClosed()
        {
        }

        public void HandleBeingShown()
        {
        }
    }
}
