using Cell.Core.Data;
using Cell.Core.Data.Tracker;
using Cell.Core.Execution.Functions;

namespace Cell.Core.Persistence
{
    /// <summary>
    /// A class that provides user collections that are sourced from another collection provider. 
    /// Collections provided by this class will be the same as the ones provided by the underlying collection provider, 
    /// but if they are modified the underlying collection will not be modified.
    /// </summary>
    public class ReadOnlyUserCollectionLoader : IUserCollectionProvider
    {
        private readonly FunctionTracker _functionTracker;
        private readonly Dictionary<string, UserCollection> _readOnlyUserCollectionMap = [];
        private readonly TestingContext _sortFunctionContext;
        private readonly IUserCollectionProvider _underlyingCollectionProvider;
        /// <summary>
        /// Creates a new instance of <see cref="ReadOnlyUserCollectionLoader"/>.
        /// </summary>
        /// <param name="underlyingCollectionProvider">The collection provider to source collections from, but not modify the collections of.</param>
        /// <param name="functionTracker">The function tracker to load sort functions from.</param>
        /// <param name="sortFunctionContext">The context used when running sort functions.</param>
        public ReadOnlyUserCollectionLoader(IUserCollectionProvider underlyingCollectionProvider, FunctionTracker functionTracker, TestingContext sortFunctionContext)
        {
            _underlyingCollectionProvider = underlyingCollectionProvider;
            _functionTracker = functionTracker;
            _sortFunctionContext = sortFunctionContext;
        }

        /// <summary>
        /// Gets a collection by name.
        /// </summary>
        /// <param name="name">The name of the collection to get.</param>
        /// <returns>The collection if it exists, or null.</returns>
        public UserCollection? GetCollection(string name)
        {
            if (_readOnlyUserCollectionMap.TryGetValue(name, out var readOnlyCollection))
            {
                return readOnlyCollection;
            }

            var realCollection = _underlyingCollectionProvider.GetCollection(name);
            if (realCollection is null)
            {
                return null;
            }
            readOnlyCollection = new UserCollection(realCollection.Model, _functionTracker, _sortFunctionContext);
            _readOnlyUserCollectionMap.Add(name, readOnlyCollection);

            foreach (var item in realCollection.Items)
            {
                // TODO: make this only copy the items that are asked for.
                readOnlyCollection.Add(item.Clone());
            }

            return readOnlyCollection;
        }

        /// <summary>
        /// Resets the loader, clearing all cached collections and resetting things to the underlying collection loaders lists.
        /// </summary>
        public void Reset()
        {
            _readOnlyUserCollectionMap.Clear();
        }
    }
}
