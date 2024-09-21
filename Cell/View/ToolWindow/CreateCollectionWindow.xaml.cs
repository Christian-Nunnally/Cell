using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows.Controls;

namespace Cell.View.ToolWindow
{
    public partial class CreateCollectionWindow : UserControl, IResizableToolWindow
    {
        private readonly CreateCollectionWindowViewModel _viewModel;
        public CreateCollectionWindow(CreateCollectionWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel;
            InitializeComponent();
        }

        public Action? RequestClose { get; set; }

        public double GetMinimumHeight() => 100;

        public double GetMinimumWidth() => 350;

        public string GetTitle() => "Creating new collection";

        public List<CommandViewModel> GetToolBarCommands()
        {
            return
            [
            ];
        }

        public void HandleBeingClosed()
        {
        }

        public void HandleBeingShown()
        {
        }

        public bool HandleCloseRequested()
        {
            return true;
        }

        private void AddCollectionButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            _viewModel.AddCurrentCollection();
            RequestClose?.Invoke();
        }
    }
}
