using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows.Controls;

namespace Cell.View.ToolWindow
{
    public partial class ExportWindow : UserControl, IResizableToolWindow
    {
        private readonly ExportWindowViewModel _viewModel;
        public ExportWindow(ExportWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel;
            InitializeComponent();
        }

        public Action? RequestClose { get; set; }

        public double GetMinimumHeight() => 200;

        public double GetMinimumWidth() => 200;

        public string GetTitle() => "Export";

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

        private void ExportSheetButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            var sheetName = _viewModel.SheetNameToExport;
            ApplicationViewModel.Instance.SheetTracker.ExportSheetTemplate(sheetName);
            DialogFactory.ShowDialog("Sheet exported", $"The sheet has been exported to the default export location as a template.");
        }
    }
}
