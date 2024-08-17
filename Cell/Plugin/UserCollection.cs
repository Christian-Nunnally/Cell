using Cell.Common;
using Cell.Model;
using Cell.Model.Plugin;
using Cell.Persistence;
using Cell.View.ToolWindow;
using Cell.ViewModel;
using System.ComponentModel;

namespace Cell.Plugin
{
    public class UserCollection(UserCollectionModel model) : PropertyChangedBase
    {
        private readonly Dictionary<string, PluginModel> _items = [];
        private readonly List<PluginModel> _sortedItems = [];
        private UserCollection? _baseCollection;

        public event Action<UserCollection, PluginModel>? ItemAdded;
        public event Action<UserCollection, PluginModel>? ItemRemoved;
        public event Action<UserCollection, PluginModel>? ItemPropertyChanged;
        public event Action<UserCollection>? OrderChanged;

        public List<PluginModel> Items => _sortedItems;

        public UserCollectionModel Model { get; private set; } = model;

        public string Name
        {
            set
            {
                if (Model.Name == value) return;
                var oldName = Model.Name;
                var newName = value;
                DialogWindow.ShowYesNoConfirmationDialog("Change Collection Name", $"Do you want to change the collection name from '{oldName}' to '{newName}'?", () =>
                {
                    UserCollectionLoader.ProcessCollectionRename(oldName, newName);
                    Model.Name = newName;
                    NotifyPropertyChanged(nameof(Name));
                });
            }

            get
            {
                return Model.Name;
            }
        }

        public int UsageCount => PluginFunctionLoader.ObservableFunctions.SelectMany(x => x.CollectionDependencies).Count(x => x == Name) + UserCollectionLoader.ObservableCollections.Count(x => x.Model.BasedOnCollectionName == Name);

        public string Type { get; internal set; } = string.Empty;

        public bool IsFilteredView => !string.IsNullOrEmpty(Model.BasedOnCollectionName);

        public void Add(PluginModel item)
        {
            if (_items.ContainsKey(item.ID)) return;
            _items.Add(item.ID, item);
            item.PropertyChanged += PropertyChangedOnItemInCollection;
            _sortedItems.Add(item);
            ItemAdded?.Invoke(this, item);
        }

        public void Remove(PluginModel item) => Remove(item.ID);

        public void Remove(string id)
        {
            if (!_items.TryGetValue(id, out var item)) return;
            _items.Remove(item.ID);
            _sortedItems.Remove(item);
            item.PropertyChanged -= PropertyChangedOnItemInCollection;
            ItemRemoved?.Invoke(this, item);
        }

        public void Sort(Comparison<PluginModel> comparison)
        {
            _sortedItems.Sort((a, b) => comparison(a, b));
            // TODO: Move all of these calls into here.
            CellPopulateManager.NotifyCollectionUpdated(Model.Name);
        }

        private void PropertyChangedOnItemInCollection(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is PluginModel model && Model.SortAndFilterFunctionName is not null)
            {
                ItemPropertyChanged?.Invoke(this, model);
                var currentIndex = _sortedItems.IndexOf(model);
                var sortFilterResult = DynamicCellPluginExecutor.RunSortFilter(new PluginContext(ApplicationViewModel.Instance, currentIndex), Model.SortAndFilterFunctionName);
                if (currentIndex == sortFilterResult) return;
                if (sortFilterResult == null)
                {
                    Remove(model.ID);
                }
                else if (sortFilterResult != currentIndex)
                {
                    _sortedItems.Remove(model);
                    var inserter = new SortedListInserter<PluginModel>(i => DynamicCellPluginExecutor.RunSortFilter(new PluginContext(ApplicationViewModel.Instance, i), Model.SortAndFilterFunctionName) ?? 0);
                    inserter.InsertSorted(_sortedItems, model, sortFilterResult ?? 0);
                    OrderChanged?.Invoke(this);
                }
            }
        }

        internal void BecomeViewIntoCollection(UserCollection baseCollection)
        {
            _baseCollection = baseCollection;
            baseCollection.ItemAdded += BaseCollectionItemAdded;
            baseCollection.ItemRemoved += BaseCollectionItemRemoved;
            baseCollection.Items.ForEach(InsertItemWithSortAndFilter);
        }

        internal void StopBeingViewIntoCollection(UserCollection baseCollection)
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

        private void InsertItemWithSortAndFilter(PluginModel model)
        {
            if (_items.ContainsKey(model.ID)) return;

            // Not sorted yet, so just add it to the end.
            _sortedItems.Add(model);
            var sortFilterResult = DynamicCellPluginExecutor.RunSortFilter(new PluginContext(ApplicationViewModel.Instance, _sortedItems.Count - 1), Model.SortAndFilterFunctionName);
            if (sortFilterResult == null || string.IsNullOrEmpty(Model.BasedOnCollectionName))
            {
                // Exlude item.
                _sortedItems.RemoveAt(_sortedItems.Count - 1);
            }
            else
            {
                _sortedItems.RemoveAt(_sortedItems.Count - 1);
                var inserter = new SortedListInserter<PluginModel>(i => DynamicCellPluginExecutor.RunSortFilter(new PluginContext(ApplicationViewModel.Instance, i), Model.SortAndFilterFunctionName) ?? 0);
                inserter.InsertSorted(_sortedItems, model, sortFilterResult ?? 0);
                _items.Add(model.ID, model);
                model.PropertyChanged += PropertyChangedOnItemInCollection;
                ItemAdded?.Invoke(this, model);
            }
        }

        private void BaseCollectionItemRemoved(UserCollection collection, PluginModel model)
        {
            Remove(model);
        }

        internal void RefreshSortAndFilter()
        {
            // TODO make this clear and notify once.
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                Remove(Items[i]);
            }
            _baseCollection?.Items.ForEach(InsertItemWithSortAndFilter);
        }
    }
}
