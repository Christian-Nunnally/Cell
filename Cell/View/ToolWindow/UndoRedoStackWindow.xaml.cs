using Cell.Common;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows.Controls;

namespace Cell.View.ToolWindow
{
    public partial class UndoRedoStackWindow : UserControl, IResizableToolWindow
    {
        private readonly UndoRedoStackWindowViewModel _viewModel;
        private double _height = 200;
        private double _width = 200;
        public UndoRedoStackWindow(UndoRedoStackWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel;
            _viewModel.UserSetWidth = GetWidth();
            _viewModel.UserSetHeight = GetHeight();
            InitializeComponent();
        }

        public Action? RequestClose { get; set; }

        public double GetHeight() => _height;

        public string GetTitle() => "Undo/Redo Stack";

        public List<CommandViewModel> GetToolBarCommands()
        {
            return
            [
                new("Undo", new RelayCommand(x => ApplicationViewModel.GetUndoRedoManager()?.Undo())),
                new("Redo", new RelayCommand(x => ApplicationViewModel.GetUndoRedoManager()?.Redo()))
            ];
        }

        public double GetWidth() => _width;

        public bool HandleBeingClosed()
        {
            return true;
        }

        public void SetHeight(double height)
        {
            _height = height;
            _viewModel.UserSetHeight = height;
        }

        public void SetWidth(double width)
        {
            _width = width;
            _viewModel.UserSetWidth = width;
        }
    }
}
