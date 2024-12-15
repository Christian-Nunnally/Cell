using Cell.ViewModel.ToolWindow;
using System.Windows;

namespace Cell.View.ToolWindow
{
    public partial class CreateSheetWindow : ResizableToolWindow
    {
        private readonly CreateSheetWindowViewModel _viewModel;
        /// <summary>
        /// Creates a new instance of the <see cref="CreateSheetWindow"/>.
        /// </summary>
        /// <param name="viewModel">The view model for this view.</param>
        public CreateSheetWindow(CreateSheetWindowViewModel viewModel) : base(viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
        }

        private void AddNewSheetButtonClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.AddNewSheet();
        }
    }
}
