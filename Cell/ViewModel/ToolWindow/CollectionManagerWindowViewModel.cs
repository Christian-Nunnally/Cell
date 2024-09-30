using Cell.Common;
using Cell.Data;
using Cell.Model.Plugin;
using Cell.Persistence;
using Cell.ViewModel.Application;
using System.Collections.Specialized;
using System.Text.Json;

namespace Cell.ViewModel.ToolWindow
{
    public class CollectionManagerWindowViewModel : ToolWindowViewModel
    {
        private readonly JsonSerializerOptions _jsonDeserializerOptions = new()
        {
            WriteIndented = true
        };
        private readonly UserCollectionLoader _userCollectionLoader;
        private string _collectionItemListBoxFilterText = string.Empty;
        private string _collectionListBoxFilterText = string.Empty;
        private bool _isEditJsonTextBoxVisible;
        private bool _isSaveItemJsonButtonVisible;
        private UserCollection? _selectedCollection;
        private PluginModel? _selectedItem;
        private string _selectedItemSerialized = string.Empty;
        public CollectionManagerWindowViewModel(UserCollectionLoader userCollectionLoader)
        {
            _userCollectionLoader = userCollectionLoader;
        }

        /// <summary>
        /// Provides a list of commands to display in the title bar of the tool window.
        /// </summary>
        public override List<CommandViewModel> ToolBarCommands => 
        [
            new CommandViewModel("New Collection", OpenCreateCollectionWindow) { ToolTip = "Opens the 'Create new collection' tool window." },
        ];

        public override double MinimumHeight => 150;

        public override double MinimumWidth => 300;

        public override double DefaultHeight => 300;

        public override double DefaultWidth => 600;

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

        public IEnumerable<UserCollection> FilteredCollections => _userCollectionLoader.ObservableCollections.Where(x => x.Name.Contains(CollectionListBoxFilterText));

        public IEnumerable<PluginModel> FilteredItemsInSelectedCollection => _selectedCollection?.Items.Where(x => x.ToString().Contains(CollectionItemListBoxFilterText)) ?? [];

        public bool IsEditJsonTextBoxVisible
        {
            get => _isEditJsonTextBoxVisible; set
            {
                if (_isEditJsonTextBoxVisible == value) return;
                _isEditJsonTextBoxVisible = value;
                NotifyPropertyChanged(nameof(IsEditJsonTextBoxVisible));
            }
        }

        public bool IsSaveItemJsonButtonVisible
        {
            get => _isSaveItemJsonButtonVisible; set
            {
                if (_isSaveItemJsonButtonVisible == value) return;
                _isSaveItemJsonButtonVisible = value;
                NotifyPropertyChanged(nameof(IsSaveItemJsonButtonVisible));
            }
        }

        public string NewCollectionBaseName { get; set; } = string.Empty;

        public string NewCollectionName { get; set; } = string.Empty;

        public UserCollection? SelectedCollection
        {
            get => _selectedCollection; set
            {
                if (_selectedCollection == value) return;
                if (_selectedCollection != null)
                {
                    _selectedCollection.ItemAdded -= SelectedCollectionChanged;
                    _selectedCollection.ItemRemoved -= SelectedCollectionChanged;
                    _selectedCollection.ItemPropertyChanged -= SelectedCollectionChanged;
                    _selectedCollection.OrderChanged -= SelectedCollectionOrderChanged;
                }

                _selectedCollection = value;
                SelectedItem = null;

                if (_selectedCollection != null)
                {
                    _selectedCollection.ItemAdded += SelectedCollectionChanged;
                    _selectedCollection.ItemRemoved += SelectedCollectionChanged;
                    _selectedCollection.ItemPropertyChanged += SelectedCollectionChanged;
                    _selectedCollection.OrderChanged += SelectedCollectionOrderChanged;
                }
                NotifyPropertyChanged(nameof(FilteredItemsInSelectedCollection));
                NotifyPropertyChanged(nameof(SelectedCollection));
                NotifyPropertyChanged(nameof(IsEditJsonTextBoxVisible));
            }
        }

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
                        item.CopyPublicProperties(_selectedItem, ["ID"]);
                        IsSaveItemJsonButtonVisible = false;
                        _selectedItemSerialized = value.StartsWith('{') ? value[1..^1].Trim().Replace("\n  ", "\n") : value;
                        NotifyPropertyChanged(nameof(SelectedItemSerialized));
                    }
                }
                catch (JsonException)
                {
                    DialogFactory.ShowDialog("Did not save", "Invalid json for item");
                }
            }
        }

        /// <summary>
        /// Gets the string displayed in top bar of this tool window.
        /// </summary>
        public override string ToolWindowTitle => "Collection Manager";

        public void DeleteCollection(UserCollection collection)
        {
            _userCollectionLoader.DeleteCollection(collection);
        }

        public override void HandleBeingClosed()
        {
            _userCollectionLoader.ObservableCollections.CollectionChanged -= GlobalCollectionsCollectionChanged;
        }

        public override void HandleBeingShown()
        {
            _userCollectionLoader.ObservableCollections.CollectionChanged += GlobalCollectionsCollectionChanged;
        }

        public void OpenCreateCollectionWindow()
        {
            var createCollectionViewModel = new CreateCollectionWindowViewModel(_userCollectionLoader);
            ApplicationViewModel.Instance.ShowToolWindow(createCollectionViewModel);
        }

        public void RemoveItemFromSelectedCollection(PluginModel item)
        {
            SelectedCollection?.Remove(item);
        }

        internal bool CanDeleteCollection(UserCollection collection, out string reason)
        {
            reason = string.Empty;
            var conflictingBase = _userCollectionLoader.ObservableCollections.FirstOrDefault(x => x.Model.BasedOnCollectionName == collection.Name);
            if (conflictingBase != null) reason = $"Cannot delete '{collection.Model.Name}' because it acting as the base for '{conflictingBase.Name}'.";
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
