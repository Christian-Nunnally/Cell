using Cell.Common;
using Cell.Data;
using Cell.Model;
using System.Collections.ObjectModel;

namespace Cell.ViewModel.ToolWindow
{
    public class FunctionManagerWindowViewModel : PropertyChangedBase
    {
        private readonly ObservableCollection<PluginFunctionViewModel> _functions;
        private string filterSheet = "All";
        private string filterString = string.Empty;
        private bool includePopulateFunctions = true;
        private bool includeTriggerFunctions = true;
        private PluginFunctionViewModel? selectedFunction;
        private double userSetHeight;
        private double userSetWidth;
        public FunctionManagerWindowViewModel(ObservableCollection<PluginFunctionViewModel> pluginFunctions)
        {
            _functions = pluginFunctions;
            _functions.CollectionChanged += FunctionsCollectionChanged;
            foreach (var function in _functions)
            {
                Functions.Add(function);
            }
            SheetNameOptions.Add("All");
            foreach (var sheet in Cells.Instance.SheetNames)
            {
                SheetNameOptions.Add(sheet);
            }
        }

        public string FilterSheet
        {
            get => filterSheet; set
            {
                if (filterSheet == value) return;
                filterSheet = value;
                NotifyPropertyChanged(nameof(FilterSheet));
                FilterVisibleFunctions();
            }
        }

        public string FilterString
        {
            get => filterString; set
            {
                if (filterString == value) return;
                filterString = value;
                NotifyPropertyChanged(nameof(FilterString));
                FilterVisibleFunctions();
            }
        }

        public ObservableCollection<PluginFunctionViewModel> Functions { get; set; } = [];

        public bool IncludePopulateFunctions
        {
            get => includePopulateFunctions; set
            {
                if (includePopulateFunctions == value) return;
                includePopulateFunctions = value;
                if (!includeTriggerFunctions && !includePopulateFunctions) IncludeTriggerFunctions = true;
                NotifyPropertyChanged(nameof(IncludePopulateFunctions));
                FilterVisibleFunctions();
            }
        }

        public bool IncludeTriggerFunctions
        {
            get => includeTriggerFunctions; set
            {
                if (includeTriggerFunctions == value) return;
                includeTriggerFunctions = value;
                if (!includeTriggerFunctions && !includePopulateFunctions) IncludePopulateFunctions = true;
                NotifyPropertyChanged(nameof(IncludeTriggerFunctions));
                FilterVisibleFunctions();
            }
        }

        public ObservableCollection<string> ReferencedCollectionsByTheSelectedFunction { get; set; } = [];

        public PluginFunctionViewModel? SelectedFunction
        {
            get => selectedFunction; set
            {
                if (selectedFunction == value) return;
                selectedFunction = value;
                NotifyPropertyChanged(nameof(SelectedFunction));
                UsersOfTheSelectedFunction.Clear();
                if (selectedFunction == null) return;
                foreach (var cell in selectedFunction.CellsThatUseFunction)
                {
                    UsersOfTheSelectedFunction.Add(cell);
                }

                ReferencedCollectionsByTheSelectedFunction.Clear();
                foreach (var collection in selectedFunction.CollectionDependencies)
                {
                    ReferencedCollectionsByTheSelectedFunction.Add(collection);
                }
            }
        }

        public ObservableCollection<string> SheetNameOptions { get; set; } = [];

        public double UserSetHeight
        {
            get => userSetHeight; set
            {
                if (userSetHeight == value) return;
                userSetHeight = value;
                NotifyPropertyChanged(nameof(UserSetHeight));
            }
        }

        public double UserSetWidth
        {
            get => userSetWidth; set
            {
                if (userSetWidth == value) return;
                userSetWidth = value;
                NotifyPropertyChanged(nameof(UserSetWidth));
            }
        }

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

        private bool IsFunctionIncludedInFilter(PluginFunctionViewModel function)
        {
            if (!function.Model.Name.Contains(filterString, StringComparison.CurrentCultureIgnoreCase)) return false;
            if (filterSheet != "All" && !function.CellsThatUseFunction.Any(x => x.SheetName == filterSheet)) return false;
            if (function.Model.ReturnType == "void") return IncludeTriggerFunctions;
            if (function.Model.ReturnType == "object") return IncludePopulateFunctions;
            return true;
        }
    }
}
