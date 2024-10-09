using Cell.ViewModel.ToolWindow;
using System.Windows;

namespace Cell.View.ToolWindow
{
    public partial class CellSettingsEditWindow : ResizableToolWindow
    {
        private readonly CellSettingsEditWindowViewModel _viewModel;

        /// <summary>
        /// Creates a new instance of the <see cref="CellSettingsEditWindow"/> class.
        /// </summary>
        /// <param name="viewModel">The view model for this view.</param>
        public CellSettingsEditWindow(CellSettingsEditWindowViewModel viewModel) : base(viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
        }

        private void EditTriggerFunctionButtonClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.EditTriggerFunction();
        }
    }
}
