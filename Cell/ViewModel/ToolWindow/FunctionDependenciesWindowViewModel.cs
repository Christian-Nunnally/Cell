using Cell.Model;
using Cell.ViewModel.Application;
using Cell.ViewModel.Execution;
using System.Collections.ObjectModel;

namespace Cell.ViewModel.ToolWindow
{
    /// <summary>
    /// View model for the function dependencies window.
    /// </summary>
    public class FunctionDependenciesWindowViewModel : ToolWindowViewModel
    {
        private readonly CellModel _cellContextToResolveDependencies;
        private string _dependencciesListBoxFilterText = string.Empty;
        private string _filterCollection = string.Empty;
        private string _filterSheet = "All";
        private CellFunctionViewModel? _selectedFunction;
        private string _usersListBoxFilterText = string.Empty;
        /// <summary>
        /// Creates a new instance of the <see cref="FunctionManagerWindowViewModel"/>.
        /// </summary>
        /// <param name="function">The function to get dependencies from.</param>
        /// <param name="cellContextToResolveDependencies"></param>
        public FunctionDependenciesWindowViewModel(CellFunctionViewModel function, CellModel cellContextToResolveDependencies)
        {
            SelectedFunction = function;
            _cellContextToResolveDependencies = cellContextToResolveDependencies;
        }

        /// <summary>
        /// Gets the default height of this tool window when it is shown.
        /// </summary>
        public override double DefaultHeight => 300;

        /// <summary>
        /// Gets the default width of this tool window when it is shown.
        /// </summary>
        public override double DefaultWidth => 300;

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
                if (_cellContextToResolveDependencies is null)
                {
                    return SelectedFunction?.Dependencies
                        .Select(x => x.ResolveUserFriendlyCellAgnosticName())
                        .Where(x => x.Contains(DependenciesListBoxFilterText)) ?? [];
                }
                else
                {
                    return SelectedFunction?.Dependencies
                        .Select(x => x.ResolveUserFriendlyNameForCell(_cellContextToResolveDependencies))
                        .Where(x => x.Contains(DependenciesListBoxFilterText)) ?? [];
                }
            }
        }

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
            }
        }

        /// <summary>
        /// Gets the list of sheets for the sheet filter dropdown.
        /// </summary>
        public ObservableCollection<string> FilterSheetOptions { get; set; } = [];

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
                NotifyPropertyChanged(nameof(FilteredUsersOfTheSelectedFunction));
                NotifyPropertyChanged(nameof(FilteredDependenciesOfTheSelectedFunction));
            }
        }

        /// <summary>
        /// Gets the string displayed in top bar of this tool window.
        /// </summary>
        public override string ToolWindowTitle => $"Depenencies of '{SelectedFunction?.Name ?? "No function selected"}'";

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
        /// Occurs when the tool window is really being closed.
        /// </summary>
        public override void HandleBeingClosed()
        {
            FilterSheetOptions.Clear();
            FilterCollectionOptions.Clear();
        }

        /// <summary>
        /// Occurs when the tool window is being shown.
        /// </summary>
        public override void HandleBeingShown()
        {
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
    }
}
