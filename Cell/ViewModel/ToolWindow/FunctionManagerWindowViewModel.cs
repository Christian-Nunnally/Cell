using Cell.Model;
using Cell.ViewModel.Application;
using Cell.ViewModel.Execution;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Cell.Core.Data.Tracker;

namespace Cell.ViewModel.ToolWindow
{
    /// <summary>
    /// A tool window view model for managing functions.
    /// </summary>
    public class FunctionManagerWindowViewModel : ToolWindowViewModel
    {
        private readonly FunctionTracker _functionTracker;
        private string _dependencciesListBoxFilterText = string.Empty;
        private string _filterCollection = string.Empty;
        private string _filterSheet = "All";
        private string _filterString = string.Empty;
        private bool _includePopulateFunctions = true;
        private bool _includeTriggerFunctions = true;
        private CellFunctionViewModel? _selectedFunction;
        private CellModel? _selectedUserOfTheSelectedFunction;
        private string _usersListBoxFilterText = string.Empty;
        /// <summary>
        /// Creates a new instance of the <see cref="FunctionManagerWindowViewModel"/>.
        /// </summary>
        /// <param name="functionTracker">The object to get the functions from.</param>
        public FunctionManagerWindowViewModel(FunctionTracker functionTracker)
        {
            _functionTracker = functionTracker;
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
        /// Gets the string that the user has entered to filter the selected functions dependencies list box.
        /// </summary>
        public string DependenciesListBoxFilterText
        {
            get => _dependencciesListBoxFilterText; set
            {
                if (_dependencciesListBoxFilterText == value) return;
                _dependencciesListBoxFilterText = value;
                NotifyPropertyChanged(nameof(DependenciesListBoxFilterText));
                NotifyPropertyChanged(nameof(FilteredDependenciesOfTheSelectedFunction));
            }
        }

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
        /// Gets the list of the selected function dependencies after the filter has been applied from the user.
        /// </summary>
        public IEnumerable<string> FilteredDependenciesOfTheSelectedFunction
        {
            get
            {
                if (SelectedUserOfTheSelectedFunction is null)
                {
                    return SelectedFunction?.Dependencies
                        .Select(x => x.ResolveUserFriendlyCellAgnosticName())
                        .Where(x => x.Contains(DependenciesListBoxFilterText)) ?? [];
                }
                else
                {
                    return SelectedFunction?.Dependencies
                        .Select(x => x.ResolveUserFriendlyNameForCell(SelectedUserOfTheSelectedFunction))
                        .Where(x => x.Contains(DependenciesListBoxFilterText)) ?? [];
                }
            }
        }

        /// <summary>
        /// Gets the list of functions after the filter has been applied from the user.
        /// </summary>
        public IEnumerable<CellFunctionViewModel> FilteredFunctions => _functionTracker.Functions.Select(x => new CellFunctionViewModel(x)).Where(IsFunctionIncludedInFilter);

        /// <summary>
        /// Gets the list of users of the selected function after the filter has been applied from the user.
        /// </summary>
        public IEnumerable<CellModel> FilteredUsersOfTheSelectedFunction => SelectedFunction?.CellsThatUseFunction.Where(x => x.Location.ToString().Contains(UsersListBoxFilterText)) ?? [];

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
                NotifyPropertyChanged(nameof(SelectedFunction));
                NotifyPropertyChanged(nameof(SelectedFunctionTitleString));
                NotifyPropertyChanged(nameof(FilteredUsersOfTheSelectedFunction));
                NotifyPropertyChanged(nameof(FilteredDependenciesOfTheSelectedFunction));
            }
        }

        /// <summary>
        /// Gets the string that is displayed in the title bar of the tool window to inform the user what function is selected.
        /// </summary>
        public string SelectedFunctionTitleString => SelectedFunction is null ? "No function selected" : SelectedFunction.Name;

        /// <summary>
        /// Gets or sets the user selected cell from the function users list.
        /// </summary>
        public CellModel? SelectedUserOfTheSelectedFunction
        {
            get => _selectedUserOfTheSelectedFunction; set
            {
                if (_selectedUserOfTheSelectedFunction == value) return;
                _selectedUserOfTheSelectedFunction = value;
                NotifyPropertyChanged(nameof(SelectedUserOfTheSelectedFunction));
                NotifyPropertyChanged(nameof(FilteredDependenciesOfTheSelectedFunction));
            }
        }

        /// <summary>
        /// Provides a list of commands to display in the title bar of the tool window.
        /// </summary>
        public override List<CommandViewModel> ToolBarCommands =>
        [
            new CommandViewModel("New Populate", () => CreateNewPopulateFunction()) { ToolTip = "Create a new function that returns a value" },
            new CommandViewModel("New Trigger", () => CreateNewTriggerFunction()) { ToolTip = "Create a new function that does not return a value" },
        ];

        /// <summary>
        /// Gets the string displayed in top bar of this tool window.
        /// </summary>
        public override string ToolWindowTitle => "Function Manager";

        /// <summary>
        /// Gets or sets the string that the user has entered to filter the users of the selected function.
        /// </summary>
        public string UsersListBoxFilterText
        {
            get => _usersListBoxFilterText; set
            {
                if (_usersListBoxFilterText == value) return;
                _usersListBoxFilterText = value;
                NotifyPropertyChanged(nameof(UsersListBoxFilterText));
                NotifyPropertyChanged(nameof(FilteredUsersOfTheSelectedFunction));
            }
        }

        /// <summary>
        /// Creates a new populate function with a default name.
        /// </summary>
        public void CreateNewPopulateFunction(string name = "PopulateFunction")
        {
            var newName = GetNewFunctionName(name);
            _functionTracker.CreateCellFunction("object", newName, "return \"Hi\";");
        }

        private string GetNewFunctionName(string name)
        {
            var index = 0;
            var newName = $"{name}";
            var existingNames = _functionTracker.Functions.Select(x => x.Model.Name).ToList();
            while (existingNames.Any(x => x == (newName = $"{name}{index++}"))) ;
            return newName;
        }

        /// <summary>
        /// Creates a new trigger function with a default name.
        /// </summary>
        public void CreateNewTriggerFunction(string name = "TriggerFunction")
        {
            var newName = GetNewFunctionName(name);
            _functionTracker.CreateCellFunction("void", newName, string.Empty);
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
            foreach (var sheet in ApplicationViewModel.Instance.SheetTracker.Sheets)
            {
                FilterSheetOptions.Add(sheet.Name);
            }
            FilterCollectionOptions.Add("All");
            foreach (var collectionName in ApplicationViewModel.Instance.UserCollectionTracker.CollectionNames)
            {
                FilterCollectionOptions.Add(collectionName);
            }
        }

        /// <summary>
        /// Creates a copy of the selected function.
        /// </summary>
        public void CreateCopyOfSelectedFunction()
        {
            if (SelectedFunction is null) return;
            if (SelectedFunction.ReturnType == "void") CreateNewTriggerFunction();
            else CreateNewPopulateFunction();
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

        private void FunctionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(FilteredFunctions));
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
