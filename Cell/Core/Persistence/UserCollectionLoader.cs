using Cell.Core.Common;
using Cell.Core.Data;
using Cell.Core.Execution.References;
using Cell.Core.Execution.SyntaxWalkers.UserCollections;
using Cell.Model;
using Cell.Model.Plugin;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json;

namespace Cell.Core.Persistence
{
    /// <summary>
    /// Loads and saves user collections from a project.
    /// </summary>
    public class UserCollectionLoader
    {
        private readonly CellTracker _cellTracker;
        private readonly Dictionary<string, UserCollection> _collections = [];
        private readonly Dictionary<string, string> _dataTypeForCollectionMap = [];
        private readonly PersistedDirectory _collectionsDirectory;
        private readonly PluginFunctionLoader _pluginFunctionLoader;
        private bool _hasGenerateDataTypeForCollectionMapChanged;
        /// <summary>
        /// Creates a new instance of <see cref="UserCollectionLoader"/>.
        /// </summary>
        /// <param name="collectionsDirectory">A directory to store and load collections from.</param>
        /// <param name="pluginFunctionLoader">The plugin function loader used to get sort functions for collections.</param>
        /// <param name="cellTracker">The cell tracker that needs to be provided to sort functions.</param>
        public UserCollectionLoader(PersistedDirectory collectionsDirectory, PluginFunctionLoader pluginFunctionLoader, CellTracker cellTracker)
        {
            _collectionsDirectory = collectionsDirectory;
            _pluginFunctionLoader = pluginFunctionLoader;
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
            var collection = new UserCollection(model, this, _pluginFunctionLoader, _cellTracker);
            StartTrackingCollection(collection);
            SaveCollection(collection);
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
            _collectionsDirectory.DeleteDirectory(collection.Name);
        }

        /// <summary>
        /// Gets a collection by name.
        /// </summary>
        /// <param name="name">The name of the collection to get.</param>
        /// <returns>The collection if it exists, or null.</returns>
        public UserCollection? GetCollection(string name)
        {
            if (name == string.Empty) return null;
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
                    if (loadedCollections.Contains(collection.Name)) continue;
                    var basedOnCollectionName = collection.Model.BasedOnCollectionName;
                    if (basedOnCollectionName == string.Empty || loadedCollections.Contains(basedOnCollectionName))
                    {
                        loadedCollections.Add(collection.Name);
                        EnsureLinkedToBaseCollection(collection);
                    }
                }
            }
        }

        /// <summary>
        /// Loads all collections from disk.
        /// </summary>
        public void LoadCollections()
        {
            foreach (var directory in _collectionsDirectory.GetDirectories())
            {
                LoadCollection(directory);
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
            foreach (var function in _pluginFunctionLoader.CellFunctions)
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

        private void DeleteItem(string collectionName, string idToRemove)
        {
            var path = Path.Combine(collectionName, "Items", idToRemove);
            _collectionsDirectory.DeleteFile(path);
        }

        private void EnsureLinkedToBaseCollection(UserCollection collection)
        {
            if (!string.IsNullOrEmpty(collection.Model.BasedOnCollectionName))
            {
                var baseCollection = GetCollection(collection.Model.BasedOnCollectionName) ?? throw new CellError($"Collection {collection.Model.Name} is based on {collection.Model.BasedOnCollectionName} which does not exist.");
                collection.BecomeViewIntoCollection(baseCollection);
            }
        }

        private void LoadCollection(string directory)
        {
            var path = Path.Combine(directory, "collection");
            var text = _collectionsDirectory.LoadFile(path) ?? throw new CellError($"Error while loading {path}");
            var model = JsonSerializer.Deserialize<UserCollectionModel>(text) ?? throw new CellError($"Error while loading {path}");
            var collection = new UserCollection(model, this, _pluginFunctionLoader, _cellTracker);
            var itemsDirectory = Path.Combine(directory, "Items");
            var paths = !_collectionsDirectory.DirectoryExists(itemsDirectory)
                ? []
                : _collectionsDirectory.GetFiles(itemsDirectory);
            paths.Select(LoadItem).ToList().ForEach(collection.Add);
            StartTrackingCollection(collection);
        }

        private PluginModel LoadItem(string path)
        {
            var text = _collectionsDirectory.LoadFile(path) ?? throw new CellError($"Failed to load {path} because it is not a valid {nameof(PluginModel)}");
            return JsonSerializer.Deserialize<PluginModel>(text) ?? throw new CellError($"Failed to load {path} because it is not a valid {nameof(PluginModel)}. File contents = {text}");
        }

        private void SaveCollection(UserCollection collection)
        {
            SaveCollectionSettings(collection.Model);
            foreach (var item in collection.Items) SaveItem(collection.Name, item.ID, item);
        }

        private void SaveCollectionSettings(UserCollectionModel model)
        {
            var path = Path.Combine(model.Name, "collection");
            var serializedModel = JsonSerializer.Serialize(model);
            _collectionsDirectory.SaveFile(path, serializedModel);
        }

        private void SaveItem(string collectionName, string id, PluginModel model)
        {
            var path = Path.Combine(collectionName, "Items", id);
            var serializedModel = JsonSerializer.Serialize(model);
            _collectionsDirectory.SaveFile(path, serializedModel);
        }

        private void StartTrackingCollection(UserCollection userCollection)
        {
            userCollection.ItemAdded += UserCollectionItemAdded;
            userCollection.ItemRemoved += UserCollectionItemRemoved;
            userCollection.ItemPropertyChanged += UserCollectionItemChanged;
            userCollection.Model.PropertyChanged += UserCollectionModelPropertyChanged;
            _collections.Add(userCollection.Name, userCollection);
            UserCollections.Add(userCollection);
            _hasGenerateDataTypeForCollectionMapChanged = true;
        }

        private void StopTrackingCollection(UserCollection userCollection)
        {
            userCollection.ItemAdded -= UserCollectionItemAdded;
            userCollection.ItemRemoved -= UserCollectionItemRemoved;
            userCollection.ItemPropertyChanged -= UserCollectionItemChanged;
            userCollection.Model.PropertyChanged -= UserCollectionModelPropertyChanged;
            _collections.Remove(userCollection.Name);
            UserCollections.Remove(userCollection);
            _hasGenerateDataTypeForCollectionMapChanged = true;
        }

        private void UnlinkFromBaseCollection(UserCollection collection)
        {
            if (!string.IsNullOrEmpty(collection.Model.BasedOnCollectionName))
            {
                var baseCollection = GetCollection(collection.Model.BasedOnCollectionName) ?? throw new CellError($"Collection {collection.Model.Name} is based on {collection.Model.BasedOnCollectionName} which does not exist.");
                collection.StopBeingViewIntoCollection(baseCollection);
            }
        }

        private void UserCollectionItemAdded(UserCollection collection, PluginModel model)
        {
            if (collection.IsFilteredView) return;
            SaveItem(collection.Name, model.ID, model);
        }

        private void UserCollectionItemChanged(UserCollection collection, PluginModel model)
        {
            if (collection.IsFilteredView) return;
            SaveItem(collection.Name, model.ID, model);
        }

        private void UserCollectionItemRemoved(UserCollection collection, PluginModel model)
        {
            if (collection.IsFilteredView) return;
            DeleteItem(collection.Name, model.ID);
        }

        private void UserCollectionModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not UserCollectionModel model) return;
            SaveCollectionSettings(model);
        }
    }
}
