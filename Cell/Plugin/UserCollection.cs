using Cell.Model.Plugin;

namespace Cell.Plugin
{
    internal class UserCollection
    {
        private readonly string _name;
        private readonly Dictionary<string, PluginModel> _items = [];
        private readonly List<PluginModel> _sortedItems = [];

        public event Action<UserCollection, PluginModel> ItemAdded;
        public event Action<UserCollection, PluginModel> ItemRemoved;

        public UserCollection(string name)
        {
            _name = name;
        }

        public void Add(PluginModel item)
        {
            if (_items.ContainsKey(item.ID)) return;
            _items.Add(item.ID, item);
            _sortedItems.Add(item);
            ItemAdded?.Invoke(this, item);
        }

        public void Remove(PluginModel item)
        {
            if (!_items.ContainsKey(item.ID)) return;
            _items.Remove(item.ID);
            _sortedItems.Remove(item);
            ItemRemoved?.Invoke(this, item);
        }

        public void Remove(string id)
        {
            if (!_items.ContainsKey(id)) return;
            var item = _items[id];
            _items.Remove(id);
            _sortedItems.Remove(item);
        }

        public void Sort(Comparison<PluginModel> comparison)
        {
            _sortedItems.Sort((a, b) => comparison(a, b));

            // TODO: Move all of these calls into here.
            CellPopulateManager.NotifyCollectionUpdated(_name);
        }

        internal bool TryGetValue(string id, out PluginModel pluginModel)
        {
            return _items.TryGetValue(id, out pluginModel!);
        }

        public List<PluginModel> Items => _sortedItems;

        public string Name => _name;
    }
}
