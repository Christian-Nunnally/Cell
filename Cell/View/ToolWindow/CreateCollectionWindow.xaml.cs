using Cell.View.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows;

namespace Cell.View.ToolWindow
{
    public partial class CreateCollectionWindow : ResizableToolWindow
    {
        private readonly CreateCollectionWindowViewModel _viewModel;
        /// <summary>
        /// Creates a new instance of the <see cref="CreateCollectionWindow"/>.
        /// </summary>
        /// <param name="viewModel">The view model for this view.</param>
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
