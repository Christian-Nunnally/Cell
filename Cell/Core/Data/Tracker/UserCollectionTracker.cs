
using Cell.Core.Common;
using Cell.Core.Execution.Functions;
using Cell.Model;
using Cell.ViewModel.Application;
using System.Collections.ObjectModel;
using Cell.Core.Persistence.Loader;

namespace Cell.Core.Data.Tracker
{
    /// <summary>
    /// Tracks and sorts user collections, allowing for easy access to them. Also provides events for when collections are added or removed. Finally, it provides a way to create new collections.
    /// </summary>
    public class UserCollectionTracker : IUserCollectionProvider
    {
        private readonly Dictionary<UserCollectionModel, string> _collectionToNameMap = [];
        private readonly CellTracker _cellTracker;
        private readonly Dictionary<string, UserCollection> _collections = [];
        private readonly FunctionTracker _functionTracker;

        /// <summary>
        /// Creates a new instance of <see cref="UserCollectionLoader"/>.
        /// </summary>
        /// <param name="functionTracker">The plugin function tracker used to get sort functions for collections.</param>
        /// <param name="cellTracker">The cell tracker that needs to be provided to sort functions.</param>
        public UserCollectionTracker(FunctionTracker functionTracker, CellTracker cellTracker)
        {
            _functionTracker = functionTracker;
            _cellTracker = cellTracker;
        }

        /// <summary>
        /// Gets just the names of all loaded collections.
        /// </summary>
        public IEnumerable<string> CollectionNames => _collections.Keys;

        /// <summary>
        /// Gets all loaded collections.
        /// </summary>
        public ObservableCollection<UserCollection> UserCollections { get; private set; } = [];

        /// <summary>
        /// Occurs when a collection is added to this tracker and is therefore being tracked.
        /// </summary>
        public event Action<UserCollection>? UserCollectionAdded;

        /// <summary>
        /// Occurs when a collection is removed from this tracker and is therefore no longer being tracked.
        /// </summary>
        public event Action<UserCollection>? UserCollectionRemoved;

        /// <summary>
        /// Creates a new collection.
        /// </summary>
        /// <param name="collectionName">The name of the new collection.</param>
        /// <param name="baseCollectionName">The name of the collection this collection should be a projection of, if any.</param>
        /// <returns>The new collection.</returns>
        public UserCollection CreateCollection(string collectionName, string baseCollectionName = "")
        {
            var model = new UserCollectionModel
            {
                Name = collectionName,
                BasedOnCollectionName = baseCollectionName
            };
            var sortContext = new Context(_cellTracker, this, new DialogFactory(), CellModel.Null);
            var collection = new UserCollection(model, _functionTracker, sortContext);
            StartTrackingCollection(collection);
            EnsureLinkedToBaseCollection(collection);
            return collection;
        }

        /// <summary>
        /// Removes a collection from the tracker.
        /// </summary>
        /// <param name="collection">The collection to delete.</param>
        public void StopTrackingCollection(UserCollection collection)
        {
            UnlinkFromBaseCollection(collection);
            _collections.Remove(collection.Model.Name);
            UserCollections.Remove(collection);
            _collectionToNameMap.Remove(collection.Model);
            collection.Model.PropertyChanged -= CollectionModelPropertyChanged;
            UserCollectionRemoved?.Invoke(collection);
        }

        private void CollectionModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var collection = (UserCollectionModel)sender!;
            if (e.PropertyName == nameof(UserCollectionModel.Name))
            {
                var oldName = _collectionToNameMap[collection];
                _collections.Remove(oldName);
                _collections.Add(collection.Name, UserCollections.First(x => x.Model == collection));
                _collectionToNameMap[collection] = collection.Name;
            }
        }

        /// <summary>
        /// Gets a collection by name.
        /// </summary>
        /// <param name="name">The name of the collection to get.</param>
        /// <returns>The collection if it exists, or null.</returns>
        public virtual UserCollection? GetCollection(string name)
        {
            if (name == string.Empty) return new UserCollection(new UserCollectionModel(), _functionTracker, new Context(_cellTracker, this, new DialogFactory(), CellModel.Null));
            if (_collections.TryGetValue(name, out UserCollection? value)) return value;
            return null;
        }

        /// <summary>
        /// Makes sure all collections are linked to their base collections.
        /// </summary>
        public void LinkUpBaseCollectionsAfterLoad()
        {
            var loadedCollections = new List<string>();
            while (loadedCollections.Count != UserCollections.Count)
            {
                foreach (var collection in UserCollections)
                {
                    if (loadedCollections.Contains(collection.Model.Name)) continue;
                    var basedOnCollectionName = collection.Model.BasedOnCollectionName;
                    if (basedOnCollectionName == string.Empty || loadedCollections.Contains(basedOnCollectionName))
                    {
                        loadedCollections.Add(collection.Model.Name);
                        EnsureLinkedToBaseCollection(collection);
                    }
                }
            }
        }

        private void EnsureLinkedToBaseCollection(UserCollection collection)
        {
            if (!string.IsNullOrEmpty(collection.Model.BasedOnCollectionName))
            {
                var baseCollection = GetCollection(collection.Model.BasedOnCollectionName) ?? throw new CellError($"Collection {collection.Model.Name} is based on {collection.Model.BasedOnCollectionName} which does not exist.");
                collection.BecomeViewIntoCollection(baseCollection);
            }
        }

        /// <summary>
        /// Starts tracking a collection.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public void StartTrackingCollection(UserCollection collection)
        {
            _collections.Add(collection.Model.Name, collection);
            UserCollections.Add(collection);
            _collectionToNameMap.Add(collection.Model, collection.Model.Name);
            collection.Model.PropertyChanged += CollectionModelPropertyChanged;
            UserCollectionAdded?.Invoke(collection);
        }

        private void UnlinkFromBaseCollection(UserCollection collection)
        {
            if (!string.IsNullOrEmpty(collection.Model.BasedOnCollectionName))
            {
                var baseCollection = GetCollection(collection.Model.BasedOnCollectionName) ?? throw new CellError($"Collection {collection.Model.Name} is based on {collection.Model.BasedOnCollectionName} which does not exist.");
                collection.StopBeingViewIntoCollection(baseCollection);
            }
        }

        public IReadOnlyDictionary<string, List<string>> GeneratePropertyNamesForCollectionMap()
        {
            var dictionary = new Dictionary<string, List<string>>();
            foreach (var collection in UserCollections)
            {
                dictionary.Add(collection.Model.Name, collection.GeneratePropertyNames());
            }
            return dictionary;
        }
    }
}
