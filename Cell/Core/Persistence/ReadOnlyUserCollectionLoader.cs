using Cell.Core.Data;
using Cell.Core.Execution.Functions;
using Cell.Model.Plugin;

namespace Cell.Core.Persistence
{
    public class ReadOnlyUserCollectionLoader : UserCollectionLoader
    {
        private readonly Dictionary<string, UserCollection> _readOnlyUserCollectionMap;

        private readonly IContext _sortFunctionContext;

        public ReadOnlyUserCollectionLoader(PersistedDirectory collectionsDirectory, PluginFunctionLoader pluginFunctionLoader, CellTracker cellTracker, IContext sortFunctionContext) : base(collectionsDirectory, pluginFunctionLoader, cellTracker)
        {
            _sortFunctionContext = sortFunctionContext;
        }

        public override UserCollection? GetCollection(string name)
        {
            if (_readOnlyUserCollectionMap.TryGetValue(name, out var readOnlyCollection))
            {
                return readOnlyCollection;
            }

            var realCollection = base.GetCollection(name);
            if (realCollection == null)
            {
                return null;
            }
            readOnlyCollection = new UserCollection(realCollection.Model, _pluginFunctionLoader, _sortFunctionContext);
            _readOnlyUserCollectionMap.Add(name, readOnlyCollection);

            foreach (var item in realCollection.Items)
            {
                // TODO: make this only copy the items that are asked for.
                readOnlyCollection.Add((PluginModel)item.Clone());
            }

            return readOnlyCollection;
        }
    }
}
