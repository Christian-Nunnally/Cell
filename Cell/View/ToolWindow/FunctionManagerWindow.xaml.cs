using Cell.Model;
using Cell.ViewModel.Application;
using Cell.ViewModel.Execution;
using Cell.ViewModel.ToolWindow;
using System.Windows.Controls;

namespace Cell.View.ToolWindow
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class FunctionManagerWindow : UserControl, IResizableToolWindow
    {
        private readonly FunctionManagerWindowViewModel _viewModel;
        public FunctionManagerWindow(FunctionManagerWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel;
            _viewModel.UserSetWidth = GetWidth();
            _viewModel.UserSetHeight = GetHeight();
            InitializeComponent();
        }

        public Action? RequestClose { get; set; }

        public double GetHeight()
        {
            return ApplicationViewModel.Instance.ApplicationSettings.FunctionManagerWindowHeight;
        }

        public string GetTitle() => "Function Manager";

        public List<CommandViewModel> GetToolBarCommands() => [];

        public double GetWidth()
        {
            return ApplicationViewModel.Instance.ApplicationSettings.FunctionManagerWindowWidth;
        }

        public bool HandleBeingClosed()
        {
            return true;
        }

        public void SetHeight(double height)
        {
            ApplicationViewModel.Instance.ApplicationSettings.FunctionManagerWindowHeight = height;
            _viewModel.UserSetHeight = height;
        }

        public void SetWidth(double width)
        {
            ApplicationViewModel.Instance.ApplicationSettings.FunctionManagerWindowWidth = width;
            _viewModel.UserSetWidth = width;
        }

        private void DeleteFunctionButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is FunctionViewModel function)
            {
                if (function.UsageCount != 0)
                {
                    DialogWindow.ShowDialog("Function in use", $"Cannot delete '{function.Model.Name}' because it is being used by {function.UsageCount} cells.");
                    return;
                }

                DialogWindow.ShowYesNoConfirmationDialog($"Delete '{function.Model.Name}'?", "Are you sure you want to delete this function?", () =>
                {
                    ApplicationViewModel.Instance.PluginFunctionLoader.DeleteFunction(function);
                });
            }
        }

        private void EditFunctionFromCellsContextButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is CellModel cell)
            {
                if (_viewModel.SelectedFunction == null) return;
                var capturedFunction = _viewModel.SelectedFunction;
                var editor = new CodeEditorWindow(_viewModel.SelectedFunction, x =>
                {
                    capturedFunction.SetUserFriendlyCode(x, cell);
                }, cell);
                ApplicationViewModel.Instance.ApplicationView.ShowToolWindow(editor, true);
            }
        }

        private void GoToCellButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is CellModel cell)
            {
                ApplicationViewModel.Instance.GoToCell(cell);
            }
        }

        private void RemoveFunctionReferenceFromCellButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is CellModel cell)
            {
                var selectedFunction = _viewModel.SelectedFunction;
                if (selectedFunction == null) return;
                if (cell.TriggerFunctionName == selectedFunction.Model.Name)
                {
                    cell.TriggerFunctionName = "";
                }
                else if (cell.PopulateFunctionName == selectedFunction.Model.Name)
                {
                    cell.PopulateFunctionName = "";
                }
                _viewModel.SelectedFunction = null;
                _viewModel.SelectedFunction = selectedFunction;
            }
        }
    }
}
