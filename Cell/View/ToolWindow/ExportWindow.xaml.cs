using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows;

namespace Cell.View.ToolWindow
{
    public partial class ExportWindow : ResizableToolWindow
    {
        private ExportWindowViewModel _viewModel;
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
