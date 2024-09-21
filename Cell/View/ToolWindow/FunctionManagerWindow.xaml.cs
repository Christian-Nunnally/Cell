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
            InitializeComponent();
        }

        public Action? RequestClose { get; set; }

        public double GetMinimumHeight() => 300;

        public double GetMinimumWidth() => 300;

        public string GetTitle() => "Function Manager";

        public List<CommandViewModel> GetToolBarCommands() => [];

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

        private void DeleteFunctionButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is PluginFunction function)
            {
                if (function.UsageCount != 0)
                {
                    DialogFactory.ShowDialog("Function in use", $"Cannot delete '{function.Model.Name}' because it is being used by {function.UsageCount} cells.");
                    return;
                }

                DialogFactory.ShowYesNoConfirmationDialog($"Delete '{function.Model.Name}'?", "Are you sure you want to delete this function?", () =>
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
                var codeEditorWindowViewModel = new CodeEditorWindowViewModel();
                var editor = new CodeEditorWindow(codeEditorWindowViewModel, _viewModel.SelectedFunction, x =>
                {
                    capturedFunction.SetUserFriendlyCode(x, cell, ApplicationViewModel.Instance.UserCollectionLoader.GetDataTypeStringForCollection, ApplicationViewModel.Instance.UserCollectionLoader.CollectionNames);
                }, cell);
                ApplicationViewModel.Instance.ShowToolWindow(editor, true);
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
