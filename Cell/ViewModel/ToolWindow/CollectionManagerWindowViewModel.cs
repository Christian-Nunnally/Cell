using Cell.Core.Common;
using Cell.Core.Data;
using Cell.Model.Plugin;
using Cell.ViewModel.Application;
using System.Collections.Specialized;
using System.Text.Json;
using Cell.ViewModel.Data;
using Cell.Core.Data.Tracker;

namespace Cell.ViewModel.ToolWindow
{
    /// <summary>
    /// A tool window that allows the user to manage collections.
    /// </summary>
    public class CollectionManagerWindowViewModel : ToolWindowViewModel
    {
        private readonly JsonSerializerOptions _jsonDeserializerOptions = new()
        {
            WriteIndented = true
        };
        private readonly UserCollectionTracker _userCollectionTracker;
        private string _collectionItemListBoxFilterText = string.Empty;
        private string _collectionListBoxFilterText = string.Empty;
        private bool _isEditJsonTextBoxVisible;
        private bool _isSaveItemJsonButtonVisible;
        private UserCollectionListItemViewModel? _selectedCollection;
        private PluginModel? _selectedItem;
        private string _selectedItemSerialized = string.Empty;
        private readonly FunctionTracker _functionTracker;

        /// <summary>
        /// Creates a new instance of the <see cref="CollectionManagerWindowViewModel"/>.
        /// </summary>
        /// <param name="userCollectionTracker">Source of the collections.</param>
        /// <param name="functionTracker">Used to determine what functions are using what collections and display it to the user.</param>
        public CollectionManagerWindowViewModel(UserCollectionTracker userCollectionTracker, FunctionTracker functionTracker)
        {
            _functionTracker = functionTracker;
            _userCollectionTracker = userCollectionTracker;
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
                NotifyPropertyChanged(nameof(FilteredCollections));
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
        /// Gets the collections that are currently being displayed in the list box to the user, filtered based on the users filter criteria.
        /// </summary>
        public IEnumerable<UserCollectionListItemViewModel> FilteredCollections => _userCollectionTracker.UserCollections.Where(x => x.Model.Name.Contains(CollectionListBoxFilterText)).Select(x => new UserCollectionListItemViewModel(x, _functionTracker, _userCollectionTracker));

        /// <summary>
        /// Gets the items in the selected collection, filtered based on the users filter criteria.
        /// </summary>
        public IEnumerable<PluginModel> FilteredItemsInSelectedCollection => _selectedCollection?.Collection.Items.Where(x => x.ToString().Contains(CollectionItemListBoxFilterText)) ?? [];

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
        /// The collection the user has selected in the collections list box.
        /// </summary>
        public UserCollectionListItemViewModel? SelectedCollection
        {
            get => _selectedCollection; set
            {
                if (_selectedCollection == value) return;
                if (_selectedCollection != null)
                {
                    _selectedCollection.Collection.ItemAdded -= SelectedCollectionChanged;
                    _selectedCollection.Collection.ItemRemoved -= SelectedCollectionChanged;
                    _selectedCollection.Collection.ItemPropertyChanged -= SelectedCollectionChanged;
                    _selectedCollection.Collection.OrderChanged -= SelectedCollectionOrderChanged;
                }

                _selectedCollection = value;
                SelectedItem = null;

                if (_selectedCollection != null)
                {
                    _selectedCollection.Collection.ItemAdded += SelectedCollectionChanged;
                    _selectedCollection.Collection.ItemRemoved += SelectedCollectionChanged;
                    _selectedCollection.Collection.ItemPropertyChanged += SelectedCollectionChanged;
                    _selectedCollection.Collection.OrderChanged += SelectedCollectionOrderChanged;
                }
                NotifyPropertyChanged(nameof(FilteredItemsInSelectedCollection));
                NotifyPropertyChanged(nameof(SelectedCollection));
                NotifyPropertyChanged(nameof(IsEditJsonTextBoxVisible));
            }
        }

        /// <summary>
        /// The item the user has selected in the items list box.
        /// </summary>
        public PluginModel? SelectedItem
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
                    var item = JsonSerializer.Deserialize<PluginModel>(stringToDeserialize);
                    if (item != null && _selectedItem != null)
                    {
                        item.CopyPublicProperties(_selectedItem, [nameof(PluginModel.ID)]);
                        IsSaveItemJsonButtonVisible = false;
                        _selectedItemSerialized = value.StartsWith('{') ? value[1..^1].Trim().Replace("\n  ", "\n") : value;
                        NotifyPropertyChanged(nameof(SelectedItemSerialized));
                    }
                }
                catch (JsonException)
                {
                    ApplicationViewModel.Instance.DialogFactory.Show("Did not save", "Invalid json for item");
                }
            }
        }

        /// <summary>
        /// Provides a list of commands to display in the title bar of the tool window.
        /// </summary>
        public override List<CommandViewModel> ToolBarCommands =>
        [
            new CommandViewModel("New Collection", OpenCreateCollectionWindow) { ToolTip = "Opens the 'Create new collection' tool window." },
        ];

        /// <summary>
        /// Gets the string displayed in top bar of this tool window.
        /// </summary>
        public override string ToolWindowTitle => "Collection Manager";

        /// <summary>
        /// Deletes the given collection.
        /// </summary>
        /// <param name="collection">The collection to delete entirely.</param>
        public void DeleteCollection(UserCollection collection)
        {
            _userCollectionTracker.DeleteCollection(collection);
        }

        /// <summary>
        /// Occurs when the tool window is really being closed.
        /// </summary>
        public override void HandleBeingClosed()
        {
            _userCollectionTracker.UserCollections.CollectionChanged -= GlobalCollectionsCollectionChanged;
        }

        /// <summary>
        /// Occurs when the tool window is being shown.
        /// </summary>
        public override void HandleBeingShown()
        {
            _userCollectionTracker.UserCollections.CollectionChanged += GlobalCollectionsCollectionChanged;
        }

        /// <summary>
        /// Opens the create collection tool window.
        /// </summary>
        public void OpenCreateCollectionWindow()
        {
            var createCollectionViewModel = new CreateCollectionWindowViewModel(_userCollectionTracker);
            ApplicationViewModel.Instance.ShowToolWindow(createCollectionViewModel);
        }

        /// <summary>
        /// Deleted the given item from the selected collection.
        /// </summary>
        /// <param name="item">The item to delete from the selected collection.</param>
        public void RemoveItemFromSelectedCollection(PluginModel item)
        {
            SelectedCollection?.Collection.Remove(item);
        }

        internal bool CanDeleteCollection(UserCollection collection, out string reason)
        {
            reason = string.Empty;
            var conflictingBase = _userCollectionTracker.UserCollections.FirstOrDefault(x => x.Model.BasedOnCollectionName == collection.Model.Name);
            if (conflictingBase != null) reason = $"Cannot delete '{collection.Model.Name}' because it acting as the base for '{conflictingBase.Model.Name}'.";
            return reason == string.Empty;
        }

        private void GlobalCollectionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(FilteredCollections));
        }

        private void ItemsInSelectedCollectionCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(FilteredItemsInSelectedCollection));
        }

        private void SelectedCollectionChanged(UserCollection collection, PluginModel model)
        {
            NotifyPropertyChanged(nameof(FilteredItemsInSelectedCollection));
        }

        private void SelectedCollectionOrderChanged(UserCollection collection)
        {
            NotifyPropertyChanged(nameof(FilteredItemsInSelectedCollection));
        }
    }
}
