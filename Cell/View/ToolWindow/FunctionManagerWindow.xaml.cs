using Cell.Core.Common;
using Cell.View.Application;
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

        private void ShowFunctionUsersButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<CellFunctionViewModel>(sender, out var function)) return;
            _viewModel.OpenUsersWindowForFunction(function);
        }

        private void ShowFunctionDependenciesButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<CellFunctionViewModel>(sender, out var function)) return;
            _viewModel.OpenDependenciesWindowForFunction(function);
        }

        private void CopyFunctionButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<CellFunctionViewModel>(sender, out var function)) return;
            _viewModel.CreateCopyOfFunction(function);
        }
    }
}
