using Cell.Model.Plugin;
using System.ComponentModel;

namespace Cell.Plugin
{
    internal class UserCollection(string name)
    {
        private readonly string _name = name;
        private readonly Dictionary<string, PluginModel> _items = [];
        private readonly List<PluginModel> _sortedItems = [];

        public event Action<UserCollection, PluginModel>? ItemAdded;
        public event Action<UserCollection, PluginModel>? ItemRemoved;
        public event Action<UserCollection, PluginModel>? ItemPropertyChanged;
        public event Action<UserCollection>? ItemOrderChanged;

        public List<PluginModel> Items => _sortedItems;

        public string Name => _name;

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
            ItemOrderChanged?.Invoke(this);
            // TODO: Move all of these calls into here.
            CellPopulateManager.NotifyCollectionUpdated(_name);
        }

        internal bool TryGetValue(string id, out PluginModel pluginModel)
        {
            return _items.TryGetValue(id, out pluginModel!);
        }

        private void PropertyChangedOnItemInCollection(object? sender, PropertyChangedEventArgs e)
        {
            ItemPropertyChanged?.Invoke(this, (PluginModel)sender!);
        }
    }
}
