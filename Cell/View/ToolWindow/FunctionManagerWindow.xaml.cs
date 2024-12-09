using Cell.Core.Common;
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
            if (ApplicationViewModel.Instance.FunctionTracker is null) throw new CellError("Unable to delete functions cells without FunctionLoader, which is null");
            if (sender is Button button && button.DataContext is CellFunctionViewModel functionViewModel)
            {
                _viewModel.PromptUserToDeleteFunctionFromProject(functionViewModel);
            }
        }

        private void EditFunctionFromCellsContextButtonClick(object sender, RoutedEventArgs e)
        {
            if (ApplicationViewModel.Instance.UserCollectionTracker is null) throw new CellError("Unable to edit cells without UserCollectionTracker, which is null");
            if (ApplicationViewModel.Instance.CellTracker is null) throw new CellError("Unable to edit cells without CellTracker, which is null");
            if (ApplicationViewModel.Instance.FunctionTracker is null) throw new CellError("Unable to edit cells without FunctionTracker, which is null");
            if (sender is Button btn && btn.DataContext is CellModel cell)
            {
                if (_viewModel.SelectedFunction is null) return;
                var capturedFunction = _viewModel.SelectedFunction;
                var collectionNameToPropertyNameMap = ApplicationViewModel.Instance.UserCollectionTracker.GeneratePropertyNamesForCollectionMap();
                var testingContext = new TestingContext(ApplicationViewModel.Instance.CellTracker, ApplicationViewModel.Instance.UserCollectionTracker, cell, ApplicationViewModel.Instance.FunctionTracker, ApplicationViewModel.Instance.Logger);
                var codeEditorWindowViewModel = new CodeEditorWindowViewModel(capturedFunction.Function, cell, collectionNameToPropertyNameMap, testingContext, ApplicationViewModel.Instance.Logger);
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
                if (selectedFunction is null) return;
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
        private void ShowFunctionUsersButtonClicked(object sender, RoutedEventArgs e)
        {

        }

        private void ShowFunctionDependenciesButtonClicked(object sender, RoutedEventArgs e)
        {

        }

        private void CopyFunctionButtonClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.CreateCopyOfSelectedFunction();
        }
    }
}
