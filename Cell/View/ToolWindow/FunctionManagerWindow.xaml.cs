using Cell.Model;
using Cell.ViewModel.Application;
using Cell.ViewModel.Execution;
using Cell.ViewModel.ToolWindow;
using System.Windows;
using System.Windows.Controls;

namespace Cell.View.ToolWindow
{
    public partial class FunctionManagerWindow : ResizableToolWindow
    {
        private FunctionManagerWindowViewModel _viewModel;
        public FunctionManagerWindow(FunctionManagerWindowViewModel viewModel) : base(viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
        }

        private void DeleteFunctionButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CellFunction function)
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

        private void EditFunctionFromCellsContextButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is CellModel cell)
            {
                if (_viewModel.SelectedFunction == null) return;
                var capturedFunction = _viewModel.SelectedFunction;
                var collectionNameToDataTypeMap = ApplicationViewModel.Instance.UserCollectionLoader.GenerateDataTypeForCollectionMap();
                var codeEditorWindowViewModel = new CodeEditorWindowViewModel(capturedFunction, cell, collectionNameToDataTypeMap);
                ApplicationViewModel.Instance.ShowToolWindow(codeEditorWindowViewModel, true);
            }
        }

        private void GoToCellButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is CellModel cell)
            {
                ApplicationViewModel.Instance.GoToCell(cell);
            }
        }

        private void RemoveFunctionReferenceFromCellButtonClick(object sender, RoutedEventArgs e)
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
