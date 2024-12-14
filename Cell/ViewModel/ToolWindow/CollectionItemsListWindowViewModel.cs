using Cell.Core.Common;
using Cell.Core.Data;
using Cell.Model;
using Cell.ViewModel.Application;
using System.Text.Json;
using Cell.Core.Data.Tracker;
using Cell.Core.Execution.Functions;

namespace Cell.ViewModel.ToolWindow
{
    /// <summary>
    /// A tool window that allows the user to manage collections.
    /// </summary>
    public class CollectionItemsListWindowViewModel : ToolWindowViewModel
    {
        private readonly JsonSerializerOptions _jsonDeserializerOptions = new()
        {
            WriteIndented = true
        };
        private readonly UserCollection _userCollection;
        private string _collectionItemListBoxFilterText = string.Empty;
        private string _collectionListBoxFilterText = string.Empty;
        private bool _isEditJsonTextBoxVisible;
        private bool _isSaveItemJsonButtonVisible;
        private UserItem? _selectedItem;
        private string _selectedItemSerialized = string.Empty;
        private readonly FunctionTracker _functionTracker;

        /// <summary>
        /// Creates a new instance of the <see cref="CollectionItemsListWindowViewModel"/>.
        /// </summary>
        /// <param name="userCollection">The collection to view the items of.</param>
        /// <param name="functionTracker">Used to determine what functions are using what collections and display it to the user.</param>
        public CollectionItemsListWindowViewModel(UserCollection userCollection, FunctionTracker functionTracker)
        {
            _functionTracker = functionTracker;
            _userCollection = userCollection;
        }

        /// <summary>
        /// Gets or sets the text used to filter the items in the selected collection.
        /// </summary>
        public string CollectionItemListBoxFilterText
        {
            get => _collectionItemListBoxFilterText; set
            {
                if (_collectionItemListBoxFilterText == value) return;
                _collectionItemListBoxFilterText = value;
                NotifyPropertyChanged(nameof(CollectionItemListBoxFilterText));
                NotifyPropertyChanged(nameof(FilteredItemsInSelectedCollection));
            }
        }

        /// <summary>
        /// Gets or sets the text used to filter the collections in the collections list box.
        /// </summary>
        public string CollectionListBoxFilterText
        {
            get => _collectionListBoxFilterText; set
            {
                if (_collectionListBoxFilterText == value) return;
                _collectionListBoxFilterText = value;
                NotifyPropertyChanged(nameof(CollectionListBoxFilterText));
            }
        }

        /// <summary>
        /// Gets the default height of this tool window when it is shown.
        /// </summary>
        public override double DefaultHeight => 300;

        /// <summary>
        /// Gets the default width of this tool window when it is shown.
        /// </summary>
        public override double DefaultWidth => 600;

        /// <summary>
        /// Gets the items in the selected collection, filtered based on the users filter criteria.
        /// </summary>
        public IEnumerable<UserItem> FilteredItemsInSelectedCollection => _userCollection.Items.Where(x => x.ToString().Contains(CollectionItemListBoxFilterText)) ?? [];

        /// <summary>
        /// Gets whether the json text box for editing the selected item is visible.
        /// </summary>
        public bool IsEditJsonTextBoxVisible
        {
            get => _isEditJsonTextBoxVisible; set
            {
                if (_isEditJsonTextBoxVisible == value) return;
                _isEditJsonTextBoxVisible = value;
                NotifyPropertyChanged(nameof(IsEditJsonTextBoxVisible));
            }
        }

        /// <summary>
        /// Gets whether the save item json button is visible.
        /// </summary>
        public bool IsSaveItemJsonButtonVisible
        {
            get => _isSaveItemJsonButtonVisible; set
            {
                if (_isSaveItemJsonButtonVisible == value) return;
                _isSaveItemJsonButtonVisible = value;
                NotifyPropertyChanged(nameof(IsSaveItemJsonButtonVisible));
            }
        }

        /// <summary>
        /// Gets the minimum height this tool window is allowed to be reized to.
        /// </summary>
        public override double MinimumHeight => 150;

        /// <summary>
        /// Gets the minimum width this tool window is allowed to be reized to.
        /// </summary>
        public override double MinimumWidth => 300;

        /// <summary>
        /// The item the user has selected in the items list box.
        /// </summary>
        public UserItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem == value) return;
                _selectedItem = value;
                _selectedItemSerialized = _selectedItem != null ? JsonSerializer.Serialize(_selectedItem, _jsonDeserializerOptions)[1..^1].Trim().Replace("\n  ", "\n") : string.Empty;
                IsSaveItemJsonButtonVisible = false;
                IsEditJsonTextBoxVisible = _selectedItem is not null;
                NotifyPropertyChanged(nameof(SelectedItemSerialized));
            }
        }

        /// <summary>
        /// A serialized version of the selected item.
        /// </summary>
        public string SelectedItemSerialized
        {
            get => _selectedItemSerialized;
            set
            {
                if (_selectedItemSerialized == value) return;

                try
                {
                    var stringToDeserialize = value.StartsWith('{') ? value : $"{{\n{value}\n}}";
                    var item = JsonSerializer.Deserialize<UserItem>(stringToDeserialize);
                    if (item != null && _selectedItem != null)
                    {
                        item.CopyPublicProperties(_selectedItem, [nameof(UserItem.ID)]);
                        IsSaveItemJsonButtonVisible = false;
                        _selectedItemSerialized = value.StartsWith('{') ? value[1..^1].Trim().Replace("\n  ", "\n") : value;
                        NotifyPropertyChanged(nameof(SelectedItemSerialized));
                    }
                }
                catch (JsonException)
                {
                    ApplicationViewModel.Instance.DialogFactory?.Show("Did not save", "Invalid json for item");
                }
            }
        }

        /// <summary>
        /// Provides a list of commands to display in the title bar of the tool window.
        /// </summary>
        public override List<CommandViewModel> ToolBarCommands =>
        [
        ];

        /// <summary>
        /// Gets the string displayed in top bar of this tool window.
        /// </summary>
        public override string ToolWindowTitle => $"'{_userCollection.Model.Name}' User Collection";

        /// <summary>
        /// Occurs when the tool window is really being closed.
        /// </summary>
        public override void HandleBeingClosed()
        {
        }

        /// <summary>
        /// Occurs when the tool window is being shown.
        /// </summary>
        public override void HandleBeingShown()
        {
        }

        /// <summary>
        /// Deletes the given item from the current collection.
        /// </summary>
        /// <param name="item">The item to delete from the selected collection.</param>
        public void RemoveItemFromSelectedCollection(UserItem item)
        {
            _userCollection?.Remove(item);
        }

        internal void EditSortAndFilterFunctionForCollection()
        {
            var functionName = _userCollection.Model.SortAndFilterFunctionName;
            if (string.IsNullOrEmpty(functionName)) return;
            var function = _functionTracker.GetOrCreateFunction("object", functionName);
            var propertyNamesForCollectionMap = ApplicationViewModel.Instance.UserCollectionTracker.GeneratePropertyNamesForCollectionMap();
            var testingContext = new TestingContext(ApplicationViewModel.Instance.CellTracker, ApplicationViewModel.Instance.UserCollectionTracker, CellModel.Null, _functionTracker, ApplicationViewModel.Instance.Logger);
            var codeEditorWindowViewModel = new CodeEditorWindowViewModel(function, null, propertyNamesForCollectionMap, testingContext, ApplicationViewModel.Instance.Logger);
            ApplicationViewModel.Instance.ShowToolWindow(codeEditorWindowViewModel, true);
        }
    }
}
