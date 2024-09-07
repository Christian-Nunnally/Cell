using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows.Controls;

namespace Cell.View.ToolWindow
{
    public partial class CreateCollectionWindow : UserControl, IResizableToolWindow
    {
        private readonly CreateCollectionWindowViewModel _viewModel;
        private double _height = 100;
        private double _width = 350;
        public CreateCollectionWindow(CreateCollectionWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel;
            _viewModel.UserSetWidth = GetWidth();
            _viewModel.UserSetHeight = GetHeight();
            InitializeComponent();
        }

        public Action? RequestClose { get; set; }

        public double GetHeight() => _height;

        public string GetTitle() => "Creating new collection";

        public List<CommandViewModel> GetToolBarCommands()
        {
            return
            [
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

        private void AddCollectionButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            _viewModel.AddCurrentCollection();
            RequestClose?.Invoke();
        }
    }
}
