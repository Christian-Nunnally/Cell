using Cell.ViewModel.ToolWindow;
using System.Windows;

namespace Cell.View.ToolWindow
{
    public partial class CreateCollectionWindow : ResizableToolWindow
    {
        private CreateCollectionWindowViewModel _viewModel;
        public CreateCollectionWindow(CreateCollectionWindowViewModel viewModel) : base(viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
        }

        private void AddCollectionButtonClick(object sender, RoutedEventArgs e)
        {
            _viewModel.AddCurrentCollection();
        }
    }
}
