using Cell.Core.Execution.Functions;
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
        private readonly FunctionManagerWindowViewModel _viewModel;
        /// <summary>
        /// Creates a new instance of the <see cref="FunctionManagerWindow"/>.
        /// </summary>
        /// <param name="viewModel">The view model for this view.</param>
        public FunctionManagerWindow(FunctionManagerWindowViewModel viewModel) : base(viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
        }

        private void DeleteFunctionButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CellFunctionViewModel function)
            {
                if (function.UsageCount != 0)
                {
                    ApplicationViewModel.Instance.DialogFactory.Show("Function in use", $"Cannot delete '{function.Name}' because it is being used by {function.UsageCount} cells.");
                    return;
                }

                ApplicationViewModel.Instance.DialogFactory.ShowYesNo($"Delete '{function.Name}'?", "Are you sure you want to delete this function?", () =>
                {
                    ApplicationViewModel.Instance.PluginFunctionLoader.DeleteCellFunction(function.Function);
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
                var testingContext = new TestingContext(ApplicationViewModel.Instance.CellTracker, ApplicationViewModel.Instance.UserCollectionLoader, new DialogFactory(), cell);
                var codeEditorWindowViewModel = new CodeEditorWindowViewModel(capturedFunction.Function, cell, collectionNameToDataTypeMap, testingContext);
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
                if (cell.TriggerFunctionName == selectedFunction.Name)
                {
                    cell.TriggerFunctionName = "";
                }
                else if (cell.PopulateFunctionName == selectedFunction.Name)
                {
                    cell.PopulateFunctionName = "";
                }
                _viewModel.SelectedFunction = null;
                _viewModel.SelectedFunction = selectedFunction;
            }
        }
    }
}
