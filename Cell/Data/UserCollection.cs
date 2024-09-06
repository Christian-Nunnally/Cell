using Cell.Common;
using Cell.Execution;
using Cell.Model;
using Cell.Model.Plugin;
using Cell.Persistence;
using Cell.View.ToolWindow;
using System.ComponentModel;

namespace Cell.Data
{
    public class UserCollection : PropertyChangedBase
    {
        private readonly Dictionary<string, int?> _cachedSortFilterResult = [];
        private readonly Dictionary<string, PluginModel> _items = [];
        private readonly List<PluginModel> _sortedItems = [];
        private UserCollection? _baseCollection;

        public UserCollection(UserCollectionModel model, UserCollectionLoader userCollectionLoader, PluginFunctionLoader pluginFunctionLoader, CellTracker cellTracker)
        {
            Model = model;
            _userCollectionLoader = userCollectionLoader;
            _pluginFunctionLoader = pluginFunctionLoader;
            _cellTracker = cellTracker;
        }

        public event Action<UserCollection, PluginModel>? ItemAdded;

        public event Action<UserCollection, PluginModel>? ItemPropertyChanged;

        public event Action<UserCollection, PluginModel>? ItemRemoved;

        public event Action<UserCollection>? OrderChanged;

        public bool IsFilteredView => !string.IsNullOrEmpty(Model.BasedOnCollectionName);

        public List<PluginModel> Items => _sortedItems;

        public UserCollectionModel Model { get; private set; }

        private readonly UserCollectionLoader _userCollectionLoader;
        private readonly PluginFunctionLoader _pluginFunctionLoader;
        private readonly CellTracker _cellTracker;

        public string Name
        {
            set
            {
                if (Model.Name == value) return;
                var oldName = Model.Name;
                var newName = value;
                DialogWindow.ShowYesNoConfirmationDialog("Change Collection Name", $"Do you want to change the collection name from '{oldName}' to '{newName}'?", () =>
                {
                    _userCollectionLoader.ProcessCollectionRename(oldName, newName);
                    Model.Name = newName;
                    NotifyPropertyChanged(nameof(Name));
                });
            }

            get
            {
                return Model.Name;
            }
        }

        public string Type { get; internal set; } = string.Empty;

        public int UsageCount => _pluginFunctionLoader.ObservableFunctions.SelectMany(x => x.CollectionDependencies).Count(x => x == Name) + _userCollectionLoader.ObservableCollections.Count(x => x.Model.BasedOnCollectionName == Name);

        public void Add(PluginModel item)
        {
            InsertItemWithSortAndFilter(item);
        }

        public void Remove(PluginModel item) => Remove(item.ID);

        public void Remove(string id)
        {
            if (!_items.TryGetValue(id, out var item)) return;
            _items.Remove(item.ID);
            _sortedItems.Remove(item);
            _cachedSortFilterResult.Remove(item.ID);
            item.PropertyChanged -= PropertyChangedOnItemInCollection;
            ItemRemoved?.Invoke(this, item);
            _baseCollection?.Remove(item);
        }

        public void BecomeViewIntoCollection(UserCollection baseCollection)
        {
            _baseCollection = baseCollection;
            baseCollection.ItemAdded += BaseCollectionItemAdded;
            baseCollection.ItemRemoved += BaseCollectionItemRemoved;
            baseCollection.ItemPropertyChanged += PropertyChangedOnItemInBaseCollection;
            baseCollection.Items.ForEach(InsertItemWithSortAndFilter);
        }

        public void RefreshSortAndFilter()
        {
            if (string.IsNullOrEmpty(Model.BasedOnCollectionName))
            {
                var itemsToSortResultMap = new Dictionary<PluginModel, int?>();
                int i = 0;
                foreach (var item in _sortedItems)
                {
                    var sortFilterResult = DynamicCellPluginExecutor.RunSortFilter(_pluginFunctionLoader, new PluginContext(_cellTracker, _userCollectionLoader, i), Model.SortAndFilterFunctionName);
                    itemsToSortResultMap.Add(item, sortFilterResult);
                    i++;
                }

                i = 0;
                foreach (var item in itemsToSortResultMap.OrderBy(x => x.Value))
                {
                    _sortedItems[i] = item.Key;
                    i++;
                }
                NotifyPropertyChanged(nameof(Items));
            }
            else
            {
                // TODO make this clear and notify once.
                for (int i = Items.Count - 1; i >= 0; i--)
                {
                    Remove(Items[i]);
                }
            
                _baseCollection?.Items.ForEach(InsertItemWithSortAndFilter);
            }
        }

        public void StopBeingViewIntoCollection(UserCollection baseCollection)
        {
            _baseCollection = null;
            baseCollection.ItemAdded -= BaseCollectionItemAdded;
            baseCollection.ItemRemoved -= BaseCollectionItemRemoved;
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                Remove(Items[i]);
            }
        }

        private void BaseCollectionItemAdded(UserCollection collection, PluginModel model)
        {
            InsertItemWithSortAndFilter(model);
        }

        private void BaseCollectionItemRemoved(UserCollection collection, PluginModel model)
        {
            Remove(model);
        }

        private void InsertItemWithSortAndFilter(PluginModel model)
        {
            if (_items.ContainsKey(model.ID)) return;

            // Not sorted yet, so just add it to the end.
            _sortedItems.Add(model);
            var sortFilterResult = DynamicCellPluginExecutor.RunSortFilter(_pluginFunctionLoader, new PluginContext(_cellTracker, _userCollectionLoader, _sortedItems.Count - 1), Model.SortAndFilterFunctionName);
            _sortedItems.RemoveAt(_sortedItems.Count - 1);
            if (sortFilterResult != null || string.IsNullOrEmpty(Model.BasedOnCollectionName))
            {
                var inserter = new SortedListInserter<PluginModel>(i => DynamicCellPluginExecutor.RunSortFilter(_pluginFunctionLoader, new PluginContext(_cellTracker, _userCollectionLoader, i), Model.SortAndFilterFunctionName) ?? 0);
                inserter.InsertSorted(_sortedItems, model, sortFilterResult ?? 0);
                _cachedSortFilterResult.Add(model.ID, sortFilterResult);
                _items.Add(model.ID, model);
                model.PropertyChanged += PropertyChangedOnItemInCollection;
                ItemAdded?.Invoke(this, model);
            }
        }

        private void PropertyChangedOnItemInBaseCollection(UserCollection collection, PluginModel model)
        {
            if (Model.SortAndFilterFunctionName is not null)
            {
                bool willBeHandledByBase = _items.ContainsKey(model.ID);
                if (willBeHandledByBase) return;

                InsertItemWithSortAndFilter(model);
            }
        }

        private void PropertyChangedOnItemInCollection(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is PluginModel model && Model.SortAndFilterFunctionName is not null)
            {
                ItemPropertyChanged?.Invoke(this, model);
                var currentIndex = _sortedItems.IndexOf(model);
                var sortFilterResult = DynamicCellPluginExecutor.RunSortFilter(_pluginFunctionLoader, new PluginContext(_cellTracker, _userCollectionLoader, currentIndex), Model.SortAndFilterFunctionName);

                var cachedResult = _cachedSortFilterResult[model.ID];
                if (cachedResult == sortFilterResult) return;
                else if (sortFilterResult == null)
                {
                    Remove(model.ID);
                    return;
                }
                else
                {
                    _cachedSortFilterResult[model.ID] = (int)sortFilterResult;
                    _sortedItems.RemoveAt(currentIndex);
                    var inserter = new SortedListInserter<PluginModel>(i => DynamicCellPluginExecutor.RunSortFilter(_pluginFunctionLoader, new PluginContext(_cellTracker, _userCollectionLoader, i), Model.SortAndFilterFunctionName) ?? 0);
                    inserter.InsertSorted(_sortedItems, model, sortFilterResult ?? 0);
                    OrderChanged?.Invoke(this);
                }
            }
        }
    }
}
