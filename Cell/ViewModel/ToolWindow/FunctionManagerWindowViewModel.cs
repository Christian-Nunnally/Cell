using Cell.Model;
using Cell.Persistence;
using Cell.ViewModel.Application;
using Cell.ViewModel.Execution;
using System.Collections.ObjectModel;

namespace Cell.ViewModel.ToolWindow
{
    public class FunctionManagerWindowViewModel : ResizeableToolWindowViewModel
    {
        private readonly ObservableCollection<FunctionViewModel> _functions;
        private readonly PluginFunctionLoader _pluginFunctionLoader;
        private string _filterSheet = "All";
        private string _filterString = string.Empty;
        private bool _includePopulateFunctions = true;
        private bool _includeTriggerFunctions = true;
        private FunctionViewModel? _selectedFunction;
        private string _filterCollection = string.Empty;

        public FunctionManagerWindowViewModel(PluginFunctionLoader pluginFunctionLoader)
        {
            _pluginFunctionLoader = pluginFunctionLoader;
            _functions = _pluginFunctionLoader.ObservableFunctions;
            _functions.CollectionChanged += FunctionsCollectionChanged;
            foreach (var function in _functions)
            {
                Functions.Add(function);
            }
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

        public string FilterSheet
        {
            get => _filterSheet; set
            {
                if (_filterSheet == value) return;
                _filterSheet = value;
                NotifyPropertyChanged(nameof(FilterSheet));
                FilterVisibleFunctions();
            }
        }

        public string FilterString
        {
            get => _filterString; set
            {
                if (_filterString == value) return;
                _filterString = value;
                NotifyPropertyChanged(nameof(FilterString));
                FilterVisibleFunctions();
            }
        }

        public string FilterCollection
        {
            get => _filterCollection; set
            {
                if (_filterCollection == value) return;
                _filterCollection = value;
                NotifyPropertyChanged(nameof(FilterCollection));
                FilterVisibleFunctions();
            }
        }

        public ObservableCollection<FunctionViewModel> Functions { get; set; } = [];

        public bool IncludePopulateFunctions
        {
            get => _includePopulateFunctions; set
            {
                if (_includePopulateFunctions == value) return;
                _includePopulateFunctions = value;
                if (!_includeTriggerFunctions && !_includePopulateFunctions) IncludeTriggerFunctions = true;
                NotifyPropertyChanged(nameof(IncludePopulateFunctions));
                FilterVisibleFunctions();
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
                FilterVisibleFunctions();
            }
        }

        public ObservableCollection<string> ReferencedCollectionsByTheSelectedFunction { get; set; } = [];

        public FunctionViewModel? SelectedFunction
        {
            get => _selectedFunction; set
            {
                if (_selectedFunction == value) return;
                _selectedFunction = value;
                NotifyPropertyChanged(nameof(SelectedFunction));
                UsersOfTheSelectedFunction.Clear();
                if (_selectedFunction == null) return;
                foreach (var cell in _selectedFunction.CellsThatUseFunction)
                {
                    UsersOfTheSelectedFunction.Add(cell);
                }

                ReferencedCollectionsByTheSelectedFunction.Clear();
                foreach (var collection in _selectedFunction.CollectionDependencies)
                {
                    ReferencedCollectionsByTheSelectedFunction.Add(collection);
                }
            }
        }

        public ObservableCollection<string> FilterSheetOptions { get; set; } = [];

        public ObservableCollection<string> FilterCollectionOptions { get; set; } = [];

        public ObservableCollection<CellModel> UsersOfTheSelectedFunction { get; set; } = [];

        private void FilterVisibleFunctions()
        {
            for (int i = Functions.Count - 1; i >= 0; i--)
            {
                if (IsFunctionIncludedInFilter(Functions[i])) continue;
                Functions.RemoveAt(i);
            }

            foreach (var function in _functions)
            {
                if (Functions.Contains(function)) continue;
                if (!IsFunctionIncludedInFilter(function)) continue;
                Functions.Add(function);
            }
        }

        private void FunctionsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Functions.Clear();
            FilterVisibleFunctions();
        }

        private bool IsFunctionIncludedInFilter(FunctionViewModel function)
        {
            if (!function.Model.Name.Contains(_filterString, StringComparison.CurrentCultureIgnoreCase)) return false;
            if (_filterSheet != "All" && !function.CellsThatUseFunction.Any(x => x.SheetName == _filterSheet)) return false;
            if (_filterCollection != "All" && !function.CollectionDependencies.Contains(_filterCollection)) return false;
            if (function.Model.ReturnType == "void") return IncludeTriggerFunctions;
            if (function.Model.ReturnType == "object") return IncludePopulateFunctions;
            return true;
        }
    }
}
