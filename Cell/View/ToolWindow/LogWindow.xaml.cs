using Cell.Common;
using Cell.Persistence;
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
            return ApplicationSettings.Instance.FunctionManagerWindowHeight;
        }

        public string GetTitle() => "Logs";

        public List<CommandViewModel> GetToolBarCommands()
        {
            return new List<CommandViewModel>()
            {
                new CommandViewModel("Clear", new RelayCommand(x => _viewModel.ClearBuffer()))
            };
        }

        public double GetWidth()
        {
            return ApplicationSettings.Instance.FunctionManagerWindowWidth;
        }

        public void HandleBeingClosed()
        {
        }

        public void SetHeight(double height)
        {
            ApplicationSettings.Instance.FunctionManagerWindowHeight = height;
            _viewModel.UserSetHeight = height;
        }

        public void SetWidth(double width)
        {
            ApplicationSettings.Instance.FunctionManagerWindowWidth = width;
            _viewModel.UserSetWidth = width;
        }
    }
}
