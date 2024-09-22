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
        private string _filterCollection = string.Empty;
        private string _filterSheet = "All";
        private string _filterString = string.Empty;
        private bool _includePopulateFunctions = true;
        private bool _includeTriggerFunctions = true;
        private PluginFunction? _selectedFunction;
        private string _usersListBoxFilterText = string.Empty;
        private string _dependencciesListBoxFilterText = string.Empty;
        private CellModel? _selectedUserOfTheSelectedFunction;

        public FunctionManagerWindowViewModel(PluginFunctionLoader pluginFunctionLoader)
        {
            _pluginFunctionLoader = pluginFunctionLoader;
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

        public string SelectedFunctionTitleString => SelectedFunction == null ? "No function selected" : SelectedFunction.Model.Name;

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

        public IEnumerable<PluginFunction> FilteredFunctions => _pluginFunctionLoader.ObservableFunctions.Where(IsFunctionIncludedInFilter);

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

        public PluginFunction? SelectedFunction
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

        public override string ToolWindowTitle => "Function Manager";

        public IEnumerable<CellModel> FilteredUsersOfTheSelectedFunction => SelectedFunction?.CellsThatUseFunction.Where(x => x.UserFriendlyCellName.Contains(UsersListBoxFilterText)) ?? [];

        public override void HandleBeingClosed()
        {
            _pluginFunctionLoader.ObservableFunctions.CollectionChanged -= FunctionsCollectionChanged;
            FilterSheetOptions.Clear();
            FilterCollectionOptions.Clear();
        }

        private void FunctionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(FilteredFunctions));
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

        private bool IsFunctionIncludedInFilter(PluginFunction function)
        {
            if (!function.Model.Name.Contains(_filterString, StringComparison.CurrentCultureIgnoreCase)) return false;
            if (_filterSheet != "All" && !function.CellsThatUseFunction.Any(x => x.SheetName == _filterSheet)) return false;
            if (function.Model.ReturnType == "void") return IncludeTriggerFunctions;
            if (function.Model.ReturnType == "object") return IncludePopulateFunctions;
            return true;
        }
    }
}
