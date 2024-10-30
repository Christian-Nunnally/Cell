
using Cell.Core.Common;
using Cell.Core.Execution.Functions;
using Cell.Core.Execution.References;
using Cell.Core.Execution.SyntaxWalkers.UserCollections;
using Cell.Core.Persistence;
using Cell.Model;
using Cell.ViewModel.Application;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.ObjectModel;
using Cell.Core.Persistence.Loader;

namespace Cell.Core.Data.Tracker
{
    public class UserCollectionTracker : IUserCollectionProvider
    {
        private readonly CellTracker _cellTracker;
        private readonly Dictionary<string, UserCollection> _collections = [];
        private readonly Dictionary<string, string> _dataTypeForCollectionMap = [];
        private readonly PersistedDirectory _collectionsDirectory;
        private readonly FunctionTracker _functionTracker;
        private bool _hasGenerateDataTypeForCollectionMapChanged;

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

        public event Action<UserCollection> UserCollectionAdded;
        public event Action<UserCollection> UserCollectionRemoved;

        /// <summary>
        /// Creates a new collection.
        /// </summary>
        /// <param name="collectionName">The name of the new collection.</param>
        /// <param name="itemTypeName">The data type of the items in the collection.</param>
        /// <param name="baseCollectionName">The name of the collection this collection should be a projection of, if any.</param>
        /// <returns>The new collection.</returns>
        public UserCollection CreateCollection(string collectionName, string itemTypeName, string baseCollectionName = "")
        {
            var model = new UserCollectionModel
            {
                Name = collectionName,
                ItemTypeName = itemTypeName,
                BasedOnCollectionName = baseCollectionName
            };
            var sortContext = new Context(_cellTracker, this, new DialogFactory(), CellModel.Null);
            var collection = new UserCollection(model, _functionTracker, sortContext);
            StartTrackingCollection(collection);
            EnsureLinkedToBaseCollection(collection);
            return collection;
        }

        /// <summary>
        /// Deletes a collection from disk.
        /// </summary>
        /// <param name="collection">The collection to delete.</param>
        public void DeleteCollection(UserCollection collection)
        {
            UnlinkFromBaseCollection(collection);
            StopTrackingCollection(collection);
            _collectionsDirectory.DeleteDirectory(collection.Model.Name);
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
        /// Gets the data type string for a collection.
        /// </summary>
        /// <param name="collection">The collection name to get the data type of its items from.</param>
        /// <returns>The data type name of the items in the collection with the given name.</returns>
        public string GetDataTypeStringForCollection(string collection) => GetCollection(collection)?.Model.ItemTypeName ?? "object";

        /// <summary>
        /// Makes sure all collections are linked to their base collections.
        /// </summary>
        public void LinkUpBaseCollectionsAfterLoad()
        {
            var loadedCollections = new List<string>();
            foreach (var collection in UserCollections)
            {
                collection.RefreshSortAndFilter();
            }
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

        /// <summary>
        /// Does the necessary work after a collection has been renamed.
        /// </summary>
        /// <param name="oldName">The old collection name.</param>
        /// <param name="newName">The new collection name.</param>
        public void ProcessCollectionRename(string oldName, string newName)
        {
            if (!_collections.TryGetValue(oldName, out var collection)) return;
            if (_collections.ContainsKey(newName)) return;
            _collections.Remove(oldName);
            _collections.Add(newName, collection);
            _collectionsDirectory.MoveDirectory(oldName, newName);

            var collectionRenamer = new CollectionReferenceRenameRewriter(oldName, newName);
            foreach (var function in _functionTracker.CellFunctions)
            {
                if (function.CollectionDependencies.OfType<ConstantCollectionReference>().Select(x => x.ConstantCollectionName).Contains(oldName))
                {
                    function.Model.Code = collectionRenamer.Visit(CSharpSyntaxTree.ParseText(function.Model.Code).GetRoot())?.ToFullString() ?? "";
                }
            }
        }

        internal IReadOnlyDictionary<string, string> GenerateDataTypeForCollectionMap()
        {
            if (_hasGenerateDataTypeForCollectionMapChanged)
            {
                _dataTypeForCollectionMap.Clear();
                foreach (var collectionName in CollectionNames)
                {
                    _dataTypeForCollectionMap.Add(collectionName, GetDataTypeStringForCollection(collectionName));
                }
            }
            _hasGenerateDataTypeForCollectionMapChanged = false;
            return _dataTypeForCollectionMap;
        }

        private void EnsureLinkedToBaseCollection(UserCollection collection)
        {
            if (!string.IsNullOrEmpty(collection.Model.BasedOnCollectionName))
            {
                var baseCollection = GetCollection(collection.Model.BasedOnCollectionName) ?? throw new CellError($"Collection {collection.Model.Name} is based on {collection.Model.BasedOnCollectionName} which does not exist.");
                collection.BecomeViewIntoCollection(baseCollection);
            }
        }

        public void StartTrackingCollection(UserCollection userCollection)
        {
            _collections.Add(userCollection.Model.Name, userCollection);
            UserCollections.Add(userCollection);
            _hasGenerateDataTypeForCollectionMapChanged = true;
            UserCollectionAdded?.Invoke(userCollection);
        }

        public void StopTrackingCollection(UserCollection userCollection)
        {
            _collections.Remove(userCollection.Model.Name);
            UserCollections.Remove(userCollection);
            _hasGenerateDataTypeForCollectionMapChanged = true;
            UserCollectionRemoved?.Invoke(userCollection);
        }

        private void UnlinkFromBaseCollection(UserCollection collection)
        {
            if (!string.IsNullOrEmpty(collection.Model.BasedOnCollectionName))
            {
                var baseCollection = GetCollection(collection.Model.BasedOnCollectionName) ?? throw new CellError($"Collection {collection.Model.Name} is based on {collection.Model.BasedOnCollectionName} which does not exist.");
                collection.StopBeingViewIntoCollection(baseCollection);
            }
        }
    }
}
