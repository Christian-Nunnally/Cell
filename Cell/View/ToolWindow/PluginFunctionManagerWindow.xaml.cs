using Cell.Model;
using Cell.Persistence;
using Cell.ViewModel;
using System.ComponentModel;
using System.Windows.Controls;

namespace Cell.View.ToolWindow
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class PluginFunctionManagerWindow : UserControl, IResizableToolWindow, INotifyPropertyChanged
    {
        private readonly PluginFunctionToolWindowViewModel _viewModel;

        public PluginFunctionManagerWindow(PluginFunctionToolWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel;
            _viewModel.UserSetWidth = GetWidth();
            _viewModel.UserSetHeight = GetHeight();
            InitializeComponent();
        }

        public Action? RequestClose { get; set; }

        public double GetWidth()
        {
            return ApplicationSettings.Instance.FunctionManagerWindowWidth;
        }

        public double GetHeight()
        {
            return ApplicationSettings.Instance.FunctionManagerWindowHeight;
        }

        public void SetWidth(double width)
        {
            ApplicationSettings.Instance.FunctionManagerWindowWidth = width;
            _viewModel.UserSetWidth = width;
        }

        public void SetHeight(double height)
        {
            ApplicationSettings.Instance.FunctionManagerWindowHeight = height;
            _viewModel.UserSetHeight = height;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void HandleBeingClosed()
        {
        }

        public string GetTitle() => "Function Manager";

        public List<CommandViewModel> GetToolBarCommands() => [];

        private void DeleteFunctionButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is PluginFunctionViewModel function)
            {
                if (function.UsageCount != 0)
                {
                    DialogWindow.ShowDialog("Function in use", $"Cannot delete '{function.Model.Name}' because it is being used by {function.UsageCount} cells.");
                    return;
                }

                DialogWindow.ShowYesNoConfirmationDialog($"Delete '{function.Model.Name}'?", "Are you sure you want to delete this function?", () =>
                {
                    PluginFunctionLoader.DeleteFunction(function);
                });
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

        private void GoToCellButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is CellModel cell)
            {
                ApplicationViewModel.Instance.GoToCell(cell);
            }
        }

        private void EditFunctionFromCellsContextButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is CellModel cell)
            {
                if (_viewModel.SelectedFunction == null) return;
                var capturedFunction = _viewModel.SelectedFunction;
                var editor = new CodeEditor(_viewModel.SelectedFunction, x =>
                {
                    capturedFunction.SetUserFriendlyCode(x, cell);
                }, cell);
                ApplicationViewModel.Instance.MainWindow.ShowToolWindow(editor);
            }
        }
    }
}
