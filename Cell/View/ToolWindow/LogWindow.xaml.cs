using Cell.Common;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows.Controls;

namespace Cell.View.ToolWindow
{
    public partial class LogWindow : UserControl, IResizableToolWindow
    {
        private readonly LogWindowViewModel _viewModel;
        public LogWindow(LogWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel;
            _viewModel.UserSetWidth = GetWidth();
            _viewModel.UserSetHeight = GetHeight();
            InitializeComponent();
        }

        public Action? RequestClose { get; set; }

        public double GetHeight()
        {
            return ApplicationViewModel.Instance.ApplicationSettings.FunctionManagerWindowHeight;
        }

        public string GetTitle() => "Logs";

        public List<CommandViewModel> GetToolBarCommands()
        {
            return
            [
                new("Clear", new RelayCommand(x => _viewModel.ClearBuffer()))
            ];
        }

        public double GetWidth()
        {
            return ApplicationViewModel.Instance.ApplicationSettings.FunctionManagerWindowWidth;
        }

        public bool HandleBeingClosed()
        {
            return true;
        }

        public void SetHeight(double height)
        {
            ApplicationViewModel.Instance.ApplicationSettings.FunctionManagerWindowHeight = height;
            _viewModel.UserSetHeight = height;
        }

        public void SetWidth(double width)
        {
            ApplicationViewModel.Instance.ApplicationSettings.FunctionManagerWindowWidth = width;
            _viewModel.UserSetWidth = width;
        }
    }
}
