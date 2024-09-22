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

        public double MinimumHeight => 200;

        public double MinimumWidth => 200;

        public Action? RequestClose { get; set; }

        public List<CommandViewModel> ToolBarCommands =>
        [
            new("Undo", new RelayCommand(x => ApplicationViewModel.GetUndoRedoManager()?.Undo())),
            new("Redo", new RelayCommand(x => ApplicationViewModel.GetUndoRedoManager()?.Redo()))
        ];

        public string ToolWindowTitle => "Undo/Redo Stack";

        public ToolWindowViewModel ToolViewModel => _viewModel;

        public void HandleBeingClosed() => _viewModel.HandleBeingShown();

        public void HandleBeingShown() => _viewModel.HandleBeingClosed();

        public bool HandleCloseRequested() => true;
    }
}
