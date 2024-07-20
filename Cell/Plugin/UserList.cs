using Cell.Model.Plugin;
using Cell.Persistence;
using System.Collections;

namespace Cell.Plugin
{
    public class UserList<T> : IEnumerable<T> where T : PluginModel, new()
    {
        public static Dictionary<string, UserList<T>> UserListsOfT { get; } = [];

        private readonly string _collectionName;
        private readonly List<T> _orderedList;
        private readonly Dictionary<string, T> _idMap;

        private UserList(string collectionName)
        {
            _collectionName = collectionName;
            _orderedList = UserCollectionLoader.GetCollection(collectionName).OfType<T>().ToList();
            _idMap = _orderedList.ToDictionary(item => item.ID);
        }

        public static UserList<T> GetOrCreate(string collectionName)
        {
            if (UserList.AllUserListNames.Contains(collectionName))
            {
                return UserListsOfT[collectionName];
            }
            UserList.AllUserListNames.Add(collectionName);
            UserListsOfT.Add(collectionName, new UserList<T>(collectionName));
            return UserListsOfT[collectionName];
        }

        public IEnumerator<T> GetEnumerator() => _orderedList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(T item)
        {
            _orderedList.Add(item);
            _idMap.Add(item.ID, item);
            UserCollectionLoader.AddToCollection(_collectionName, item);
        }

        public void Remove(T item)
        {
            _orderedList.Remove(item);
            _idMap.Remove(item.ID);
            UserCollectionLoader.RemoveFromCollection(_collectionName, item.ID);
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _orderedList.Count) return;
            var model = _orderedList[index];
            _orderedList.RemoveAt(index);
            _idMap.Remove(model.ID);
            UserCollectionLoader.RemoveFromCollection(_collectionName, model.ID);
        }

        public T GetLast()
        {
            return _orderedList.Last();
        }

        public T? this[int key]
        {
            get => key >= 0 && key < _orderedList.Count ? _orderedList[key] : new T();
        }
    }

    public class UserList
    {
        public static List<string> AllUserListNames { get; } = [];

        private UserList() { }
    }
}
