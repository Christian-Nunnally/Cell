using Cell.ViewModel.ToolWindow;
using System.Windows;

namespace Cell.View.ToolWindow
{
    /// <summary>
    /// The interaction handling for cell content editing.
    /// </summary>
    public partial class CellContentEditWindow : ResizableToolWindow
    {
        private readonly CellContentEditWindowViewModel _viewModel;
        /// <summary>
        /// Creates a new instance of the <see cref="CellContentEditWindow"/> class.
        /// </summary>
        /// <param name="viewModel">The view model containing the data for this view.</param>
        public CellContentEditWindow(CellContentEditWindowViewModel viewModel) : base(viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
        }

        private void EditPopulateFunctionButtonClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.EditPopulateFunction();
        }

        private void EditTriggerFunctionButtonClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.EditTriggerFunction();
        }

        private void MultiUseUserInputTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            _viewModel.SubmitMultiUseUserInputText();
        }

        private void AutoIndexButtonClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.IndexSelectedCells();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
