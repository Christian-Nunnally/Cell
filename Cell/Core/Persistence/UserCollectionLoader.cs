﻿using Cell.Common;
using Cell.Data;
using Cell.Execution.References;
using Cell.Execution.SyntaxWalkers.UserCollections;
using Cell.Model;
using Cell.Model.Plugin;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json;

namespace Cell.Persistence
{
    public class UserCollectionLoader
    {
        private readonly CellTracker _cellTracker;
        private readonly Dictionary<string, UserCollection> _collections = [];
        private readonly PersistedDirectory _persistanceManager;
        private readonly PluginFunctionLoader _pluginFunctionLoader;
        private readonly Dictionary<string, string> _dataTypeForCollectionMap = [];
        private bool _hasGenerateDataTypeForCollectionMapChanged;

        public UserCollectionLoader(PersistedDirectory persistenceManager, PluginFunctionLoader pluginFunctionLoader, CellTracker cellTracker)
        {
            _persistanceManager = persistenceManager;
            _pluginFunctionLoader = pluginFunctionLoader;
            _cellTracker = cellTracker;
        }

        public IEnumerable<string> CollectionNames => _collections.Keys;

        public ObservableCollection<UserCollection> ObservableCollections { get; private set; } = [];

        public UserCollection CreateCollection(string collectionName, string itemTypeName, string baseCollectionName = "")
        {
            var model = new UserCollectionModel(collectionName, itemTypeName, baseCollectionName);
            var collection = new UserCollection(model, this, _pluginFunctionLoader, _cellTracker);
            StartTrackingCollection(collection);
            SaveCollection(collection);
            EnsureLinkedToBaseCollection(collection);
            return collection;
        }

        public void DeleteCollection(UserCollection collection)
        {
            UnlinkFromBaseCollection(collection);
            StopTrackingCollection(collection);
            var directory = Path.Combine("Collections", collection.Name);
            _persistanceManager.DeleteDirectory(directory);
        }

        public UserCollection? GetCollection(string name)
        {
            if (name == string.Empty) return null;
            if (_collections.TryGetValue(name, out UserCollection? value)) return value;
            return null;
        }

        public string GetDataTypeStringForCollection(string collection) => GetCollection(collection)?.Model.ItemTypeName ?? "object";

        public void ImportCollection(string collectionDirectory, string collectionName)
        {
            var toDirectory = Path.Combine("Collections", collectionName);
            _persistanceManager.CopyDirectory(collectionDirectory, toDirectory);
        }

        public void LinkUpBaseCollectionsAfterLoad()
        {
            var loadedCollections = new List<string>();
            foreach (var collection in ObservableCollections)
            {
                collection.RefreshSortAndFilter();
            }
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
            foreach (var function in _pluginFunctionLoader.ObservableFunctions)
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
            var path = Path.Combine("Collections", collectionName, "Items", idToRemove);
            _persistanceManager.DeleteFile(path);
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
            var text = _persistanceManager.LoadFile(path) ?? throw new CellError($"Error while loading {path}");
            var model = JsonSerializer.Deserialize<UserCollectionModel>(text) ?? throw new CellError($"Error while loading {path}");
            var collection = new UserCollection(model, this, _pluginFunctionLoader, _cellTracker);
            var itemsDirectory = Path.Combine(directory, "Items");
            var paths = !_persistanceManager.DirectoryExists(itemsDirectory)
                ? []
                : _persistanceManager.GetFiles(itemsDirectory);
            paths.Select(LoadItem).ToList().ForEach(collection.Add);
            StartTrackingCollection(collection);
        }

        private PluginModel LoadItem(string path)
        {
            var text = _persistanceManager.LoadFile(path) ?? throw new CellError($"Failed to load {path} because it is not a valid {nameof(PluginModel)}");
            return JsonSerializer.Deserialize<PluginModel>(text) ?? throw new CellError($"Failed to load {path} because it is not a valid {nameof(PluginModel)}. File contents = {text}");
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
            _hasGenerateDataTypeForCollectionMapChanged = true;
        }

        private void StopTrackingCollection(UserCollection userCollection)
        {
            userCollection.ItemAdded -= UserCollectionItemAdded;
            userCollection.ItemRemoved -= UserCollectionItemRemoved;
            userCollection.ItemPropertyChanged -= UserCollectionItemChanged;
            userCollection.Model.PropertyChanged -= UserCollectionModelPropertyChanged;
            _collections.Remove(userCollection.Name);
            ObservableCollections.Remove(userCollection);
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
