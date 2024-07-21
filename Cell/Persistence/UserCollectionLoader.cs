using Cell.Data;
using Cell.Exceptions;
using Cell.Model.Plugin;
using Cell.Plugin;
using Cell.ViewModel;
using System.IO;
using System.Text.Json;

namespace Cell.Persistence
{
    internal static class UserCollectionLoader
    {
        private static readonly Dictionary<string, UserCollection> _collections = [];

        public static IEnumerable<string> CollectionNames => _collections.Keys;

        public static UserCollection GetOrCreateCollection(string name)
        {
            if (_collections.TryGetValue(name, out UserCollection? value)) return value;
            var userCollection = new UserCollection(name);
            StartTrackingCollection(userCollection);
            return userCollection;
        }

        private static void StartTrackingCollection(UserCollection userCollection)
        {
            userCollection.ItemAdded += UserCollectionItemAdded;
            userCollection.ItemRemoved += UserCollectionItemRemoved;
            userCollection.ItemPropertyChanged += UserCollectionItemChanged;
            userCollection.ItemOrderChanged += UserCollectionOrderChanged;
            _collections.Add(userCollection.Name, userCollection);
        }

        private static void UserCollectionItemChanged(UserCollection collection, PluginModel model)
        {
            SaveItem(collection.Name, model.ID, model);
            CellPopulateManager.NotifyCollectionUpdated(collection.Name);
        }

        private static void UserCollectionItemAdded(UserCollection collection, PluginModel model)
        {
            SaveItem(collection.Name, model.ID, model);
            CellPopulateManager.NotifyCollectionUpdated(collection.Name);
        }

        private static void UserCollectionItemRemoved(UserCollection collection, PluginModel model)
        {
            DeleteItem(collection.Name, model.ID);
            CellPopulateManager.NotifyCollectionUpdated(collection.Name);
        }

        private static void UserCollectionOrderChanged(UserCollection collection)
        {
            CellPopulateManager.NotifyCollectionUpdated(collection.Name);
        }

        public static void SaveCollections()
        {
            foreach (var collection in _collections) SaveCollection(collection.Key, collection.Value);
        }

        private static void SaveCollection(string collectionName, UserCollection collection)
        {
            foreach (var model in collection.Items) SaveItem(collectionName, model.ID, model);
        }

        public static void LoadCollections()
        {
            var collectionsDirectory = Path.Combine(PersistenceManager.SaveLocation, "Collections");
            if (!Directory.Exists(collectionsDirectory)) return;
            foreach (var directory in Directory.GetDirectories(collectionsDirectory))
            {
                LoadCollection(directory);
            }
        }

        private static void LoadCollection(string directory)
        {
            var collection = new UserCollection(Path.GetFileName(directory));
            Directory.GetFiles(directory).Select(LoadItem).ToList().ForEach(collection.Add);
            StartTrackingCollection(collection);
        }

        private static void SaveItem(string collectionName, string id, PluginModel model)
        {
            var directory = Path.Combine(GetSaveDirectory(), collectionName);
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, id);
            var serializedModel = JsonSerializer.Serialize(model);
            File.WriteAllText(path, serializedModel);
        }

        private static PluginModel LoadItem(string path)
        {
            var text = File.ReadAllText(path);
            return JsonSerializer.Deserialize<PluginModel>(text) ?? throw new ProjectLoadException($"Failed to load {path} because it is not a valid {nameof(PluginModel)}. File contents = {text}");
        }

        private static void DeleteItem(string collectionName, string idToRemove)
        { 
            var directory = Path.Combine(GetSaveDirectory(), collectionName);
            var path = Path.Combine(directory, idToRemove);
            if (File.Exists(path)) File.Delete(path);
        }

        private static string GetSaveDirectory()
        {
            var directory = Path.Combine(PersistenceManager.SaveLocation, "Collections");
            Directory.CreateDirectory(directory);
            return directory;
        }

        public static string GetDataTypeStringForCollection(string collection) => Cells.AllCells
            .Where(x => x.IsCollection(collection) && !string.IsNullOrWhiteSpace(x.GetCollectionType()))
            .Select(x => x.GetCollectionType())
            .FirstOrDefault(string.Empty);
    }
}
