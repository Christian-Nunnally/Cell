using Cell.Data;
using Cell.Exceptions;
using Cell.Model.Plugin;
using Cell.Plugin;
using Cell.ViewModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json;

namespace Cell.Persistence
{
    internal static class UserCollectionLoader
    {
        private static readonly Dictionary<string, UserCollection> _collections = [];

        public static IEnumerable<string> CollectionNames => _collections.Keys;

        internal static void AddToCollection(string collectionName, PluginModel model)
        {
            var collection = GetCollection(collectionName) ?? CreateAndTrackNewCollection(collectionName);
            collection.Add(model);
            SaveItem(collectionName, model.ID, model);
            CellPopulateManager.NotifyCollectionUpdated(collectionName);
            model.PropertyChanged += PluginModelPropertyChanged;
        }

        private static void PluginModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is PluginModel model)
            {
                foreach (var (collectionName, collection) in _collections)
                {
                    if (collection.TryGetValue(model.ID, out var pluginModel))
                    {
                        SaveItem(collectionName, pluginModel.ID, pluginModel);
                        CellPopulateManager.NotifyCollectionUpdated(collectionName);
                    }
                }
            }
        }

        public static UserCollection? GetCollection(string collection) => _collections.TryGetValue(collection, out UserCollection? value) ? value : null;

        public static bool CreateEmptyCollection(string collectionName)
        {
            if (!_collections.ContainsKey(collectionName))
            {
                CreateAndTrackNewCollection(collectionName);
                return true;
            }
            return false;
        }

        private static UserCollection CreateAndTrackNewCollection(string collection)
        {
            var userCollection = new UserCollection(collection);
            userCollection.ItemAdded += UserCollectionItemAdded;
            userCollection.ItemRemoved += UserCollectionItemRemoved;
            _collections.Add(collection, userCollection);
            return userCollection;
        }

        private static void UserCollectionItemAdded(UserCollection collection, PluginModel model)
        {
            CellPopulateManager.NotifyCollectionUpdated(collection.Name);
        }

        private static void UserCollectionItemRemoved(UserCollection collection, PluginModel model)
        {
            CellPopulateManager.NotifyCollectionUpdated(collection.Name);
        }

        public static void SaveCollections()
        {
            foreach (var collection in _collections) SaveCollection(collection.Key, collection.Value);
        }

        public static void SaveCollection(string collectionName, UserCollection collection)
        {
            foreach (var model in collection.Items) SaveItem(collectionName, model.ID, model);
        }

        private static void SaveItem(string collectionName, string id, PluginModel model)
        {
            var directory = Path.Combine(GetSaveDirectory(), collectionName);
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, id);
            var serializedModel = JsonSerializer.Serialize(model);
            File.WriteAllText(path, serializedModel);
        }

        internal static void LoadCollections()
        {
            var collectionsDirectory = Path.Combine(PersistenceManager.SaveLocation, "Collections");
            if (Directory.Exists(collectionsDirectory))
            {
                foreach (var directory in Directory.GetDirectories(collectionsDirectory))
                {
                    var collection = new UserCollection(Path.GetFileName(directory));
                    foreach (var file in Directory.GetFiles(directory))
                    {
                        var model = JsonSerializer.Deserialize<PluginModel>(File.ReadAllText(file)) ?? throw new ProjectLoadException($"Failed to load collection {directory} because of mode {file} is not a valid {nameof(PluginModel)}");
                        collection.Add(model);
                        model.PropertyChanged += PluginModelPropertyChanged;
                    }
                    _collections.Add(Path.GetFileName(directory), collection);
                }
            }
        }

        private static string GetSaveDirectory()
        {
            var directory = Path.Combine(PersistenceManager.SaveLocation, "Collections");
            Directory.CreateDirectory(directory);
            return directory;
        }

        public static string GetDataTypeStringForCollection(string collection)
        {
            foreach (var cell in Cells.AllCells)
            {
                if (!cell.StringProperties.TryGetValue(nameof(ListCellViewModel.CollectionName), out var name) || name != collection) continue;
                if (!cell.StringProperties.TryGetValue(nameof(ListCellViewModel.CollectionType), out var type)) continue;
                return type;
            }
            return string.Empty;
        }

        internal static void RemoveFromCollection(string collectionName, string idToRemove)
        {
            if (string.IsNullOrEmpty(collectionName)) return;
            if (_collections.TryGetValue(collectionName, out var collection))
            {
                if (collection.TryGetValue(idToRemove, out var model))
                {
                    model.PropertyChanged -= PluginModelPropertyChanged;
                    collection.Remove(idToRemove);
                    DeleteItem(collectionName, idToRemove);
                    CellPopulateManager.NotifyCollectionUpdated(collectionName);
                }
            }
        }

        private static void DeleteItem(string collectionName, string idToRemove)
        { 
            var directory = Path.Combine(GetSaveDirectory(), collectionName);
            var path = Path.Combine(directory, idToRemove);
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
