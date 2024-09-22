using Cell.Model;
using Cell.ViewModel.Application;
using Cell.ViewModel.Execution;
using Cell.ViewModel.ToolWindow;
using System.Windows.Controls;

namespace Cell.View.ToolWindow
{
    public partial class FunctionManagerWindow : ResizableToolWindow
    {
        private FunctionManagerWindowViewModel FunctionManagerWindowViewModel => (FunctionManagerWindowViewModel)ToolViewModel;
        public FunctionManagerWindow(FunctionManagerWindowViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }

        public override double MinimumHeight => 300;

        public override double MinimumWidth => 300;

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
                if (FunctionManagerWindowViewModel.SelectedFunction == null) return;
                var capturedFunction = FunctionManagerWindowViewModel.SelectedFunction;
                var collectionNameToDataTypeMap = ApplicationViewModel.Instance.UserCollectionLoader.GenerateDataTypeForCollectionMap();
                var codeEditorWindowViewModel = new CodeEditorWindowViewModel(capturedFunction, cell, collectionNameToDataTypeMap);
                ApplicationViewModel.Instance.ShowToolWindow(codeEditorWindowViewModel, true);
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
                var selectedFunction = FunctionManagerWindowViewModel.SelectedFunction;
                if (selectedFunction == null) return;
                if (cell.TriggerFunctionName == selectedFunction.Model.Name)
                {
                    cell.TriggerFunctionName = "";
                }
                else if (cell.PopulateFunctionName == selectedFunction.Model.Name)
                {
                    cell.PopulateFunctionName = "";
                }
                FunctionManagerWindowViewModel.SelectedFunction = null;
                FunctionManagerWindowViewModel.SelectedFunction = selectedFunction;
            }
        }
    }
}
