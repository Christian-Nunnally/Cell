using Cell.Model;
using Cell.Persistence;
using Cell.ViewModel.Application;
using Cell.ViewModel.Execution;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Cell.ViewModel.ToolWindow
{
    public class FunctionManagerWindowViewModel : ToolWindowViewModel
    {
        private readonly PluginFunctionLoader _pluginFunctionLoader;
        private string _dependencciesListBoxFilterText = string.Empty;
        private string _filterCollection = string.Empty;
        private string _filterSheet = "All";
        private string _filterString = string.Empty;
        private bool _includePopulateFunctions = true;
        private bool _includeTriggerFunctions = true;
        private CellFunction? _selectedFunction;
        private CellModel? _selectedUserOfTheSelectedFunction;
        private string _usersListBoxFilterText = string.Empty;
        public FunctionManagerWindowViewModel(PluginFunctionLoader pluginFunctionLoader)
        {
            _pluginFunctionLoader = pluginFunctionLoader;
        }

        /// <summary>
        /// Gets the default height of this tool window when it is shown.
        /// </summary>
        public override double DefaultHeight => 400;

        /// <summary>
        /// Gets the default width of this tool window when it is shown.
        /// </summary>
        public override double DefaultWidth => 650;

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

        public ObservableCollection<string> FilterCollectionOptions { get; set; } = [];

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

        public IEnumerable<CellFunction> FilteredFunctions => _pluginFunctionLoader.ObservableFunctions.Where(IsFunctionIncludedInFilter);

        public IEnumerable<CellModel> FilteredUsersOfTheSelectedFunction => SelectedFunction?.CellsThatUseFunction.Where(x => x.UserFriendlyCellName.Contains(UsersListBoxFilterText)) ?? [];

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

        public ObservableCollection<string> FilterSheetOptions { get; set; } = [];

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

        public override double MinimumHeight => 200;

        public override double MinimumWidth => 200;

        public CellFunction? SelectedFunction
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

        public string SelectedFunctionTitleString => SelectedFunction == null ? "No function selected" : SelectedFunction.Model.Name;

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
            new CommandViewModel("New Populate", CreateNewPopulateFunction) { ToolTip = "Create a new function that returns a value" },
            new CommandViewModel("New Trigger", CreateNewTriggerFunction) { ToolTip = "Create a new function that does not return a value" },
        ];

        /// <summary>
        /// Gets the string displayed in top bar of this tool window.
        /// </summary>
        public override string ToolWindowTitle => "Function Manager";

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

        public void CreateNewPopulateFunction()
        {
            var index = 0;
            var newPopulateFunctionName = "NewPopulateFunction";
            var existingNames = _pluginFunctionLoader.ObservableFunctions.Select(x => x.Model.Name).ToList();
            while (existingNames.Any(x => x == newPopulateFunctionName))
            {
                index += 1;
                newPopulateFunctionName += $"NewPopulateFunction{index}";
            }
            _pluginFunctionLoader.CreateFunction("object", newPopulateFunctionName, "return \"Hello world\";");
        }

        public void CreateNewTriggerFunction()
        {
            var index = 0;
            var newTriggerFunctionName = "NewTriggerFunction";
            var existingNames = _pluginFunctionLoader.ObservableFunctions.Select(x => x.Model.Name).ToList();
            while (existingNames.Any(x => x == newTriggerFunctionName))
            {
                index += 1;
                newTriggerFunctionName += $"NewTriggerFunction{index}";
            }
            _pluginFunctionLoader.CreateFunction("void", newTriggerFunctionName, string.Empty);
        }

        public override void HandleBeingClosed()
        {
            _pluginFunctionLoader.ObservableFunctions.CollectionChanged -= FunctionsCollectionChanged;
            FilterSheetOptions.Clear();
            FilterCollectionOptions.Clear();
        }

        public override void HandleBeingShown()
        {
            _pluginFunctionLoader.ObservableFunctions.CollectionChanged += FunctionsCollectionChanged;
            FilterSheetOptions.Add("All");
            foreach (var sheet in ApplicationViewModel.Instance.SheetTracker.Sheets)
            {
                FilterSheetOptions.Add(sheet.Name);
            }
            FilterCollectionOptions.Add("All");
            foreach (var collectionName in ApplicationViewModel.Instance.UserCollectionLoader.CollectionNames)
            {
                FilterCollectionOptions.Add(collectionName);
            }
        }

        private void FunctionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(FilteredFunctions));
        }

        private bool IsFunctionIncludedInFilter(CellFunction function)
        {
            if (!function.Model.Name.Contains(_filterString, StringComparison.CurrentCultureIgnoreCase)) return false;
            if (_filterSheet != "All" && !function.CellsThatUseFunction.Any(x => x.SheetName == _filterSheet)) return false;
            if (function.Model.ReturnType == "void") return IncludeTriggerFunctions;
            if (function.Model.ReturnType == "object") return IncludePopulateFunctions;
            return true;
        }
    }
}
