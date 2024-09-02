using Cell.Common;
using Cell.Data;
using Cell.Execution;
using Cell.Execution.SyntaxWalkers;
using Cell.Model;
using Cell.Model.Plugin;
using Cell.ViewModel.Application;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json;

namespace Cell.Persistence
{
    public class UserCollectionLoader
    {
        private static readonly Dictionary<string, UserCollection> _collections = [];
        private readonly PersistenceManager _persistanceManager;
        private readonly CellPopulateManager _cellPopulateManager;

        public static IEnumerable<string> CollectionNames => _collections.Keys;

        public static ObservableCollection<UserCollection> ObservableCollections { get; private set; } = [];

        public UserCollectionLoader(PersistenceManager persistenceManager, CellPopulateManager cellPopulateManager)
        {
            _persistanceManager = persistenceManager;
            _cellPopulateManager = cellPopulateManager;
        }

        public static UserCollection? GetCollection(string name)
        {
            if (name == string.Empty) throw new CellError("Collection name cannot be empty");
            if (_collections.TryGetValue(name, out UserCollection? value)) return value;
            return null;
        }

        public string GetDataTypeStringForCollection(string collection) => GetCollection(collection)?.Model.ItemTypeName ?? "";

        public void LoadCollections()
        {
            var collectionsDirectory = Path.Combine("Collections");
            if (!_persistanceManager.DirectoryExists(collectionsDirectory)) return;
            foreach (var directory in _persistanceManager.GetDirectories(collectionsDirectory))
            {
                LoadCollection(directory);
            }
        }

        public void ProcessCollectionRename(string oldName, string newName)
        {
            if (!_collections.TryGetValue(oldName, out var collection)) return;
            if (_collections.ContainsKey(newName)) return;
            _collections.Remove(oldName);
            _collections.Add(newName, collection);
            var oldDirectory = Path.Combine("Collections", oldName);
            var newDirectory = Path.Combine("Collections", newName);
            _persistanceManager.MoveDirectory(oldDirectory, newDirectory);

            var collectionRenamer = new CollectionReferenceRenameRewriter(oldName, newName);
            foreach (var function in ApplicationViewModel.Instance.PluginFunctionLoader.ObservableFunctions)
            {
                if (function.CollectionDependencies.Contains(oldName))
                {
                    function.Model.Code = collectionRenamer.Visit(CSharpSyntaxTree.ParseText(function.Model.Code).GetRoot())?.ToFullString() ?? "";
                }
            }
        }

        public void SaveCollections()
        {
            foreach (var collection in _collections) SaveCollection(collection.Value);
        }

        internal UserCollection CreateCollection(string collectionName, string itemTypeName, string baseCollectionName)
        {
            var model = new UserCollectionModel(collectionName, itemTypeName, baseCollectionName);
            model.PropertyChanged += UserCollectionModelPropertyChanged;
            var collection = new UserCollection(model);
            StartTrackingCollection(collection);
            SaveCollection(collection);
            EnsureLinkedToBaseCollection(collection);
            return collection;
        }

        internal void DeleteCollection(UserCollection collection)
        {
            UnlinkFromBaseCollection(collection);
            StopTrackingCollection(collection);
            var directory = Path.Combine("Collections", collection.Name);
            _persistanceManager.DeleteDirectory(directory);
        }

        internal void LinkUpBaseCollectionsAfterLoad()
        {
            var loadedCollections = new List<string>();
            while (loadedCollections.Count != ObservableCollections.Count)
            {
                foreach (var collection in ObservableCollections)
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

        private void DeleteItem(string collectionName, string idToRemove)
        {
            var path = Path.Combine("Collections", collectionName, "Items", idToRemove);
            _persistanceManager.DeleteFile(path);
        }

        private static void EnsureLinkedToBaseCollection(UserCollection collection)
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
            var text = File.ReadAllText(path);
            var model = JsonSerializer.Deserialize<UserCollectionModel>(text) ?? throw new CellError($"Error while loading {path}");
            var collection = new UserCollection(model);
            var itemsDirectory = Path.Combine(directory, "Items");
            Directory.GetFiles(itemsDirectory).Select(LoadItem).ToList().ForEach(collection.Add);
            StartTrackingCollection(collection);
        }

        private static PluginModel LoadItem(string path)
        {
            var text = File.ReadAllText(path);
            return JsonSerializer.Deserialize<PluginModel>(text) ?? throw new CellError($"Failed to load {path} because it is not a valid {nameof(PluginModel)}. File contents = {text}");
        }

        public void ExportCollection(string collectionName, string toDirectory)
        {
            var fromDirectory = Path.Combine("Collections", collectionName);
            _persistanceManager.CopyDirectory(fromDirectory, toDirectory);
        }

        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        private void SaveCollection(UserCollection collection)
        {
            SaveCollectionSettings(collection.Model);
            foreach (var item in collection.Items) SaveItem(collection.Name, item.ID, item);
        }

        private void SaveCollectionSettings(UserCollectionModel model)
        {
            var path = Path.Combine("Collections", model.Name, "collection");
            var serializedModel = JsonSerializer.Serialize(model);
            _persistanceManager.SaveFile(path, serializedModel);
        }

        private void SaveItem(string collectionName, string id, PluginModel model)
        {
            var path = Path.Combine("Collections", collectionName, "Items", id);
            var serializedModel = JsonSerializer.Serialize(model);
            _persistanceManager.SaveFile(path, serializedModel);
        }

        private void StartTrackingCollection(UserCollection userCollection)
        {
            userCollection.ItemAdded += UserCollectionItemAdded;
            userCollection.ItemRemoved += UserCollectionItemRemoved;
            userCollection.ItemPropertyChanged += UserCollectionItemChanged;
            userCollection.Model.PropertyChanged += UserCollectionModelPropertyChanged;
            _collections.Add(userCollection.Name, userCollection);
            ObservableCollections.Add(userCollection);
        }

        private void StopTrackingCollection(UserCollection userCollection)
        {
            userCollection.ItemAdded -= UserCollectionItemAdded;
            userCollection.ItemRemoved -= UserCollectionItemRemoved;
            userCollection.ItemPropertyChanged -= UserCollectionItemChanged;
            userCollection.Model.PropertyChanged -= UserCollectionModelPropertyChanged;
            _collections.Remove(userCollection.Name);
            ObservableCollections.Remove(userCollection);
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
            if (!collection.IsFilteredView) SaveItem(collection.Name, model.ID, model);
            _cellPopulateManager.NotifyCollectionUpdated(collection.Name);
        }

        private void UserCollectionItemChanged(UserCollection collection, PluginModel model)
        {
            if (!collection.IsFilteredView) SaveItem(collection.Name, model.ID, model);
            _cellPopulateManager.NotifyCollectionUpdated(collection.Name);
        }

        private void UserCollectionItemRemoved(UserCollection collection, PluginModel model)
        {
            if (!collection.IsFilteredView) DeleteItem(collection.Name, model.ID);
            _cellPopulateManager.NotifyCollectionUpdated(collection.Name);
        }

        private void UserCollectionModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not UserCollectionModel model) return;
            SaveCollectionSettings(model);
        }

        internal void ImportCollection(string collectionDirectory, string collectionName)
        {
            var toDirectory = Path.Combine("Collections", collectionName);
            _persistanceManager.CopyDirectory(collectionDirectory, toDirectory);
        }
    }
}
