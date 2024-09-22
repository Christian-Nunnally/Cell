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

        public double MinimumHeight => 200;

        public double MinimumWidth => 200;

        public Action? RequestClose { get; set; }

        public List<CommandViewModel> ToolBarCommands => [];

        public string ToolWindowTitle => "Export";

        public ToolWindowViewModel ToolViewModel => _viewModel;

        public void HandleBeingClosed()
        {
            _viewModel.HandleBeingShown();
        }

        public void HandleBeingShown()
        {
            _viewModel.HandleBeingClosed();
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
