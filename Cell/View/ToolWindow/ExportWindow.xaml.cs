using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows.Controls;

namespace Cell.View.ToolWindow
{
    public partial class ExportWindow : UserControl, IResizableToolWindow
    {
        private readonly ExportWindowViewModel _viewModel;
        private double _height = 200;
        private double _width = 200;
        public ExportWindow(ExportWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel;
            _viewModel.UserSetWidth = GetWidth();
            _viewModel.UserSetHeight = GetHeight();
            InitializeComponent();
        }

        public Action? RequestClose { get; set; }

        public double GetHeight() => _height;

        public string GetTitle() => "Export";

        public List<CommandViewModel> GetToolBarCommands()
        {
            return
            [
            ];
        }

        public double GetWidth() => _width;

        public bool HandleBeingClosed()
        {
            return true;
        }

        public void SetHeight(double height)
        {
            _height = height;
            _viewModel.UserSetHeight = height;
        }

        public void SetWidth(double width)
        {
            _width = width;
            _viewModel.UserSetWidth = width;
        }

        private void ExportSheetButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            var sheetName = _viewModel.SheetNameToExport;
            ApplicationViewModel.Instance.CellLoader.ExportSheetTemplate(sheetName);
            DialogWindow.ShowDialog("Sheet exported", $"The sheet has been exported to the default export location as a template. ({ApplicationViewModel.Instance.PersistenceManager.CurrentTemplatePath})");
        }
    }
}
