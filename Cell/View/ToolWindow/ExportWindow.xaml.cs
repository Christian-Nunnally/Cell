using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;

namespace Cell.View.ToolWindow
{
    public partial class ExportWindow : ResizableToolWindow
    {
        private ExportWindowViewModel ExportWindowViewModel => (ExportWindowViewModel)ToolViewModel;
        public ExportWindow(ExportWindowViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }

        public override double MinimumHeight => 200;

        public override double MinimumWidth => 200;

        private void ExportSheetButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            var sheetName = ExportWindowViewModel.SheetNameToExport;
            ApplicationViewModel.Instance.SheetTracker.ExportSheetTemplate(sheetName);
            DialogFactory.ShowDialog("Sheet exported", $"The sheet has been exported to the default export location as a template.");
        }
    }
}
