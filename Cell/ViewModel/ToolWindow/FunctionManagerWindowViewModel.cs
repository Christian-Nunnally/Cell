using Cell.Core.Data;
using Cell.Core.Data.Tracker;
using Cell.Core.Execution.Functions;
using Cell.ViewModel.Application;
using Cell.ViewModel.Execution;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Cell.ViewModel.ToolWindow
{
    /// <summary>
    /// A tool window view model for managing functions.
    /// </summary>
    public class FunctionManagerWindowViewModel : ToolWindowViewModel
    {
        private readonly CellSelector _cellSelector;
        private readonly FunctionTracker _functionTracker;
        private string _filterCollection = string.Empty;
        private string _filterSheet = "All";
        private string _filterString = string.Empty;
        private FunctionUsersWindowViewModel _functionUsersWindowViewModel = new FunctionUsersWindowViewModel();
        private FunctionDependenciesWindowViewModel _functionDependenciesWindowViewModel = new FunctionDependenciesWindowViewModel();
        private bool _includePopulateFunctions = true;
        private bool _includeTriggerFunctions = true;
        private CellFunctionViewModel? _selectedFunction;
        /// <summary>
        /// Creates a new instance of the <see cref="FunctionManagerWindowViewModel"/>.
        /// </summary>
        /// <param name="functionTracker">The object to get the functions from.</param>
        public FunctionManagerWindowViewModel(FunctionTracker functionTracker, CellSelector cellSelector)
        {
            _cellSelector = cellSelector;
            _functionTracker = functionTracker;

            var compileAllFunctionsAsync = new AsyncCommand(CompileAllFunctionsAsync);
            ToolBarCommands.Add(new CommandViewModel("Compile All", compileAllFunctionsAsync) { ToolTip = "Create a new function that returns a value" });
            ToolBarCommands.Add(new CommandViewModel("New Populate", () => CreateNewPopulateFunction()) { ToolTip = "Create a new function that returns a value" });
            ToolBarCommands.Add(new CommandViewModel("New Trigger", () => CreateNewTriggerFunction()) { ToolTip = "Create a new function that does not return a value" });
        }

        /// <summary>
        /// Gets the default height of this tool window when it is shown.
        /// </summary>
        public override double DefaultHeight => 400;

        /// <summary>
        /// Gets the default width of this tool window when it is shown.
        /// </summary>
        public override double DefaultWidth => 650;

        /// <summary>
        /// Gets or sets the string that the user has entered to filter the collection of functions to functions that depend on a given collection name.
        /// </summary>
        public string FilterCollection
        {
            get => _filterCollection; set
            {
                if (_filterCollection == value) return;
                _filterCollection = value;
                NotifyPropertyChanged(nameof(FilterCollection));
                NotifyPropertyChanged(nameof(FilteredFunctions));
            }
        }

        /// <summary>
        /// Gets the list of collection names for the collection filter dropdown.
        /// </summary>
        public ObservableCollection<string> FilterCollectionOptions { get; set; } = [];

        /// <summary>
        /// Gets the list of functions after the filter has been applied from the user.
        /// </summary>
        public IEnumerable<CellFunctionViewModel> FilteredFunctions => _functionTracker.Functions.Select(x => new CellFunctionViewModel(x)).Where(IsFunctionIncludedInFilter);

        /// <summary>
        /// Gets or sets the string that the user has entered to filter the collection of functions to a given sheet name.
        /// </summary>
        public string FilterSheet
        {
            get => _filterSheet; set
            {
                if (_filterSheet == value) return;
                _filterSheet = value;
                NotifyPropertyChanged(nameof(FilterSheet));
                NotifyPropertyChanged(nameof(FilteredFunctions));
            }
        }

        /// <summary>
        /// Gets the list of sheets for the sheet filter dropdown.
        /// </summary>
        public ObservableCollection<string> FilterSheetOptions { get; set; } = [];

        /// <summary>
        /// Gets or sets the string that the user has entered to filter the functions.
        /// </summary>
        public string FilterString
        {
            get => _filterString; set
            {
                if (_filterString == value) return;
                _filterString = value;
                NotifyPropertyChanged(nameof(FilterString));
                NotifyPropertyChanged(nameof(FilteredFunctions));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether populate functions should be included in the list of functions.
        /// </summary>
        public bool IncludePopulateFunctions
        {
            get => _includePopulateFunctions; set
            {
                if (_includePopulateFunctions == value) return;
                _includePopulateFunctions = value;
                if (!_includeTriggerFunctions && !_includePopulateFunctions) IncludeTriggerFunctions = true;
                NotifyPropertyChanged(nameof(IncludePopulateFunctions));
                NotifyPropertyChanged(nameof(FilteredFunctions));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether trigger functions should be included in the list of functions.
        /// </summary>
        public bool IncludeTriggerFunctions
        {
            get => _includeTriggerFunctions; set
            {
                if (_includeTriggerFunctions == value) return;
                _includeTriggerFunctions = value;
                if (!_includeTriggerFunctions && !_includePopulateFunctions) IncludePopulateFunctions = true;
                NotifyPropertyChanged(nameof(IncludeTriggerFunctions));
                NotifyPropertyChanged(nameof(FilteredFunctions));
            }
        }

        /// <summary>
        /// Gets the minimum height this tool window is allowed to be reized to.
        /// </summary>
        public override double MinimumHeight => 200;

        /// <summary>
        /// Gets the minimum width this tool window is allowed to be reized to.
        /// </summary>
        public override double MinimumWidth => 200;

        /// <summary>
        /// Gets or sets the function that is currently selected in the list box.
        /// </summary>
        public CellFunctionViewModel? SelectedFunction
        {
            get => _selectedFunction; set
            {
                if (_selectedFunction == value) return;
                _selectedFunction = value;
                if (_functionUsersWindowViewModel is not null) _functionUsersWindowViewModel.SelectedFunction = value;
                if (_functionDependenciesWindowViewModel is not null) _functionDependenciesWindowViewModel.SelectedFunction = value;

                _cellSelector.UnselectAllCells();
                _cellSelector.SelectCells(_selectedFunction?.CellsThatUseFunction ?? []);

                NotifyPropertyChanged(nameof(SelectedFunction));
            }
        }

        /// <summary>
        /// Gets the string displayed in top bar of this tool window.
        /// </summary>
        public override string ToolWindowTitle => "Function Manager";

        /// <summary>
        /// Creates a copy of the selected function.
        /// </summary>
        /// <param name="function">The function to copy.</param>
        public void CreateCopyOfFunction(CellFunctionViewModel function)
        {
            if (SelectedFunction is null) return;
            var copy = function.ReturnType == "void"
                ? CreateNewTriggerFunction()
                : CreateNewPopulateFunction();
        }

        /// <summary>
        /// Creates a new populate function with a default name.
        /// </summary>
        public CellFunction CreateNewPopulateFunction(string name = "PopulateFunction")
        {
            var newName = GetNewFunctionName(name);
            return _functionTracker.CreateCellFunction("object", newName, "return \"Hi\";");
        }

        /// <summary>
        /// Creates a new trigger function with a default name.
        /// </summary>
        public CellFunction CreateNewTriggerFunction(string name = "TriggerFunction")
        {
            var newName = GetNewFunctionName(name);
            return _functionTracker.CreateCellFunction("void", newName, string.Empty);
        }

        /// <summary>
        /// Occurs when the tool window is really being closed.
        /// </summary>
        public override void HandleBeingClosed()
        {
            _functionTracker.Functions.CollectionChanged -= FunctionsCollectionChanged;
            FilterSheetOptions.Clear();
            FilterCollectionOptions.Clear();
        }

        /// <summary>
        /// Occurs when the tool window is being shown.
        /// </summary>
        public override void HandleBeingShown()
        {
            _functionTracker.Functions.CollectionChanged += FunctionsCollectionChanged;
            FilterSheetOptions.Add("All");
            foreach (var sheet in ApplicationViewModel.Instance.SheetTracker?.Sheets ?? [])
            {
                FilterSheetOptions.Add(sheet.Name);
            }
            FilterCollectionOptions.Add("All");
            foreach (var collectionName in ApplicationViewModel.Instance.UserCollectionTracker?.CollectionNames ?? [])
            {
                FilterCollectionOptions.Add(collectionName);
            }
        }

        internal void OpenDependenciesWindowForFunction(CellFunctionViewModel function)
        {
            _functionDependenciesWindowViewModel.SelectedFunction = function;
            ApplicationViewModel.Instance.ShowToolWindow(_functionDependenciesWindowViewModel);
        }

        internal void OpenUsersWindowForFunction(CellFunctionViewModel function)
        {
            _functionUsersWindowViewModel.SelectedFunction = function;
            ApplicationViewModel.Instance.ShowToolWindow(_functionUsersWindowViewModel);
        }

        internal void PromptUserToDeleteFunctionFromProject(CellFunctionViewModel functionViewModel)
        {
            if (functionViewModel.UsageCount != 0)
            {
                ApplicationViewModel.Instance.DialogFactory?.Show("Function in use", $"Cannot delete '{functionViewModel.Name}' because it is being used by {functionViewModel.UsageCount} cells.");
                return;
            }

            ApplicationViewModel.Instance.DialogFactory?.ShowYesNo($"Delete '{functionViewModel.Name}'?", "Are you sure you want to delete this function?", () =>
            {
                _functionTracker.StopTrackingFunction(functionViewModel.Function);
            });
        }

        private async Task CompileAllFunctionsAsync()
        {
            foreach (var function in _functionTracker.Functions)
            {
                function.Compile();
                await Task.Delay(1);
            }
        }

        private void FunctionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(FilteredFunctions));
        }

        private string GetNewFunctionName(string name)
        {
            var index = 0;
            var newName = $"{name}";
            var existingNames = _functionTracker.Functions.Select(x => x.Model.Name).ToList();
            while (existingNames.Any(x => x == (newName = $"{name}{index++}"))) ;
            return newName;
        }

        private bool IsFunctionIncludedInFilter(CellFunctionViewModel function)
        {
            if (!function.Name.Contains(_filterString, StringComparison.CurrentCultureIgnoreCase)) return false;
            if (_filterSheet != "All" && !function.CellsThatUseFunction.Any(x => x.Location.SheetName == _filterSheet)) return false;
            if (function.ReturnType == "void") return IncludeTriggerFunctions;
            if (function.ReturnType == "object") return IncludePopulateFunctions;
            return true;
        }
    }
}
