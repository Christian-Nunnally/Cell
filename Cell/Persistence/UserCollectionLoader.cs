using Cell.ViewModel;
using System.IO;
using System.Text.Json;

namespace Cell.Persistence
{
    internal static class UserCollectionLoader
    {
        private static readonly Dictionary<string, List<string>> _collections = [];

        public static IEnumerable<string> CollectionNames => _collections.Keys;

        internal static void AddToCollection(string collection, string data)
        {
            if (_collections.TryGetValue(collection, out List<string>? value))
            {
                value.Add(data);
            }
            else
            {
                _collections.Add(collection, [data]);
            }
            SaveCollection(collection, _collections[collection]);
        }

        internal static IEnumerable<string> GetCollection(string collection) => _collections.TryGetValue(collection, out List<string>? value) ? value : (IEnumerable<string>)([]);

        public static bool CreateEmptyCollection(string collection)
        {
            if (!_collections.ContainsKey(collection))
            {
                _collections.Add(collection, []);
                SaveCollection(collection, _collections[collection]);
                return true;
            }
            return false;
        }

        internal static void SaveCollections()
        {
            foreach (var collection in _collections) SaveCollection(collection.Key, collection.Value);
        }

        private static void SaveCollection(string key, List<string> collection)
        {
            var directory = GetSaveDirectory();
            var path = Path.Combine(directory, key);
            var serializedCollection = JsonSerializer.Serialize(collection);
            File.WriteAllText(path, serializedCollection);
        }

        internal static void LoadCollections()
        {
            var directory = Path.Combine(PersistenceManager.SaveLocation, "Collections");
            if (Directory.Exists(directory))
            {
                foreach (var file in Directory.GetFiles(directory))
                {
                    var collection = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(file));
                    if (collection == null) continue;
                    _collections.Add(Path.GetFileName(file), collection);
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
    }
}
