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
        private static readonly Dictionary<string, Dictionary<string, PluginModel>> _collections = [];

        public static IEnumerable<string> CollectionNames => _collections.Keys;

        internal static void AddToCollection(string collection, PluginModel model)
        {
            if (_collections.TryGetValue(collection, out Dictionary<string, PluginModel>? value))
            {
                if (value.ContainsKey(model.ID)) return;
                value.Add(model.ID, model);
            }
            else
            {
                _collections.Add(collection, new Dictionary<string, PluginModel> {{model.ID, model}});
            }
            SaveItem(collection, model.ID, model);
            CellPopulateManager.NotifyCollectionUpdated(collection);
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

        public static IEnumerable<PluginModel> GetCollection(string collection) => _collections.TryGetValue(collection, out Dictionary<string, PluginModel>? value) ? value.Values : ([]);

        public static bool CreateEmptyCollection(string collection)
        {
            if (!_collections.ContainsKey(collection))
            {
                _collections.Add(collection, []);
                return true;
            }
            return false;
        }

        public static void SaveCollections()
        {
            foreach (var collection in _collections) SaveCollection(collection.Key, collection.Value);
        }

        public static void SaveCollection(string collectionName, Dictionary<string, PluginModel> collection)
        {
            foreach (var (id, model) in collection) SaveItem(collectionName, id, model);
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
                    var collection = new Dictionary<string, PluginModel>();
                    foreach (var file in Directory.GetFiles(directory))
                    {
                        var model = JsonSerializer.Deserialize<PluginModel>(File.ReadAllText(file)) ?? throw new ProjectLoadException($"Failed to load collection {directory} because of mode {file} is not a valid {nameof(PluginModel)}");
                        collection.Add(Path.GetFileName(file), model);
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
