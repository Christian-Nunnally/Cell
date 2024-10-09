using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows;

namespace Cell.View.ToolWindow
{
    public partial class ExportWindow : ResizableToolWindow
    {
        private readonly ExportWindowViewModel _viewModel;

        /// <summary>
        /// Creates a new instance of the <see cref="ExportWindow"/>.
        /// </summary>
        /// <param name="viewModel">The view model for this view.</param>
        public ExportWindow(ExportWindowViewModel viewModel) : base(viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
        }

        private void ExportSheetButtonClicked(object sender, RoutedEventArgs e)
        {
            var sheetName = _viewModel.SheetNameToExport;
            ApplicationViewModel.Instance.SheetTracker.ExportSheetTemplate(sheetName);
            DialogFactory.ShowDialog("Sheet exported", $"The sheet has been exported to the default export location as a template.");
        }
    }
}
