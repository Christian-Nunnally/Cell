using Cell.Core.Common;
using Cell.Model;
using Cell.Model.Plugin;
using Cell.Core.Persistence;
using System.ComponentModel;
using Cell.Core.Execution.Functions;

namespace Cell.Core.Data
{
    /// <summary>
    /// A user collection is a user created collection of items that can be sorted and filtered.
    /// </summary>
    public class UserCollection : PropertyChangedBase
    {
        private readonly CellModel _cellModelToInjectIndexIntoSortFunction = new();
        private readonly Dictionary<string, int?> _cachedSortFilterResult = [];
        private readonly IContext _sortFunctionContext;
        private readonly Dictionary<string, PluginModel> _items = [];
        private readonly PluginFunctionLoader _pluginFunctionLoader;
        private readonly List<PluginModel> _sortedItems = [];
        private UserCollection? _baseCollection;
        /// <summary>
        /// Initializes a new instance of the <see cref="UserCollection"/>.
        /// </summary>
        /// <param name="model">The underlying collection model.</param>
        /// <param name="pluginFunctionLoader">The function loader to load the sort function from.</param>
        /// <param name="sortFunctionContext">The cell tracker to provide to the sort function context.</param>
        public UserCollection(UserCollectionModel model, PluginFunctionLoader pluginFunctionLoader, IContext sortFunctionContext)
        {
            sortFunctionContext.ContextCell = _cellModelToInjectIndexIntoSortFunction;
            Model = model;
            Model.PropertyChanged += UserCollectionModelPropertyChanged;
            _pluginFunctionLoader = pluginFunctionLoader;
            _sortFunctionContext = sortFunctionContext;
        }

        /// <summary>
        /// Occurs when an item is added to the collection.
        /// </summary>
        public event Action<UserCollection, PluginModel>? ItemAdded;

        /// <summary>
        /// Occurs when a property of an item in the collection changes.
        /// </summary>
        public event Action<UserCollection, PluginModel>? ItemPropertyChanged;

        /// <summary>
        /// Occurs when an item is removed from the collection.
        /// </summary>
        public event Action<UserCollection, PluginModel>? ItemRemoved;

        /// <summary>
        /// Occurs when the order of items in the collection changes.
        /// </summary>
        public event Action<UserCollection>? OrderChanged;

        /// <summary>
        /// Gets whether this collection is a filtered view of another collection.
        /// </summary>
        public bool IsFilteredView => !string.IsNullOrEmpty(Model.BasedOnCollectionName);

        /// <summary>
        /// Gets the items in this collection.
        /// </summary>
        public List<PluginModel> Items => _sortedItems;

        /// <summary>
        /// The underlying collection model.
        /// </summary>
        public UserCollectionModel Model { get; private set; }

        /// <summary>
        /// Gets the data type of items in this collection.
        /// </summary>
        public string Type { get; internal set; } = string.Empty;

        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(PluginModel item)
        {
            InsertItemWithSortAndFilter(item);
        }

        /// <summary>
        /// Starts viewing the base collection and adds all items from the base collection to this collection.
        /// 
        /// When an item is added to the base collection, it will be added to this collection if it passes the sort and filter function.
        /// </summary>
        /// <param name="baseCollection">The collection to start being a projection of.</param>
        public void BecomeViewIntoCollection(UserCollection baseCollection)
        {
            _baseCollection = baseCollection;
            baseCollection.ItemAdded += BaseCollectionItemAdded;
            baseCollection.ItemRemoved += BaseCollectionItemRemoved;
            baseCollection.ItemPropertyChanged += PropertyChangedOnItemInBaseCollection;
            baseCollection.Items.ForEach(InsertItemWithSortAndFilter);
        }

        /// <summary>
        /// Updates the sort and filter for all items in the collection.
        /// </summary>
        public void RefreshSortAndFilter()
        {
            if (string.IsNullOrEmpty(Model.BasedOnCollectionName))
            {
                var itemsToSortResultMap = new Dictionary<PluginModel, int?>();
                int i = 0;
                foreach (var item in _sortedItems)
                {
                    var sortFilterResult = RunSortFilter(i++);
                    itemsToSortResultMap.Add(item, sortFilterResult);
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
                for (int i = Items.Count - 1; i >= 0; i--)
                {
                    Remove(Items[i]);
                }

                _baseCollection?.Items.ForEach(InsertItemWithSortAndFilter);
            }
        }

        /// <summary>
        /// Removes the item from this collection.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        public void Remove(PluginModel item) => Remove(item.ID);

        /// <summary>
        /// Unlinks this collection from its base collection and removes all items from the collection.
        /// </summary>
        /// <param name="baseCollection">The collection to unlink from.</param>
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

        private static int? ConvertReturnedObjectToSortFilterResult(object? resultObject)
        {
            if (resultObject is null) return null;
            if (int.TryParse(resultObject.ToString(), out var resultInt)) return resultInt;
            return null;
        }

        private int? RunSortFilter(PluginFunctionLoader pluginFunctionLoader, IContext pluginContext, string functionName, int indexToGetResultFor)
        {
            _cellModelToInjectIndexIntoSortFunction.Index = indexToGetResultFor;
            if (!pluginFunctionLoader.TryGetCellFunction("object", functionName, out var populateFunction)) return 0;
            var result = populateFunction.Run(pluginContext);
            if (result.WasSuccess)
            {
                return ConvertReturnedObjectToSortFilterResult(result.ReturnedObject);
            }
            return 0;
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
            int? sortFilterResult = GetSortFilterResultFromItemNotInList(model);
            if (sortFilterResult != null || string.IsNullOrEmpty(Model.BasedOnCollectionName))
            {
                InsertSorted(model, sortFilterResult);
                _cachedSortFilterResult.Add(model.ID, sortFilterResult);
                _items.Add(model.ID, model);
                model.PropertyChanged += PropertyChangedOnItemInCollection;
                ItemAdded?.Invoke(this, model);
            }
        }

        private int? GetSortFilterResultFromItemNotInList(PluginModel model)
        {
            _sortedItems.Add(model);
            var sortFilterResult = RunSortFilter(_sortedItems.Count - 1);
            _sortedItems.RemoveAt(_sortedItems.Count - 1);
            return sortFilterResult;
        }

        private void InsertSorted(PluginModel model, int? sortFilterResult)
        {
            var inserter = new SortedListInserter<PluginModel>(RunSortFilter);
            inserter.InsertSorted(_sortedItems, model, sortFilterResult ?? 0);
        }

        private int? RunSortFilter(int i)
        {
            return RunSortFilter(_pluginFunctionLoader, _sortFunctionContext, Model.SortAndFilterFunctionName, i) ?? 0;
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
                var sortFilterResult = RunSortFilter(currentIndex);

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
                    InsertSorted(model, sortFilterResult);
                    OrderChanged?.Invoke(this);
                }
            }
        }

        private void Remove(string id)
        {
            if (!_items.TryGetValue(id, out var item)) return;
            _items.Remove(item.ID);
            _sortedItems.Remove(item);
            _cachedSortFilterResult.Remove(item.ID);
            item.PropertyChanged -= PropertyChangedOnItemInCollection;
            ItemRemoved?.Invoke(this, item);
            _baseCollection?.Remove(item);
        }

        private void UserCollectionModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UserCollectionModel.SortAndFilterFunctionName))
            {
                RefreshSortAndFilter();
            }
        }
    }
}
