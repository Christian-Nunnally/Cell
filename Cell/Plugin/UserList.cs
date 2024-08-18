using Cell.Common;
using Cell.Model.Plugin;
using Cell.Persistence;
using System.Collections;

namespace Cell.Plugin
{
    public class UserList<T> : IEnumerable<T> where T : PluginModel, new()
    {
        public static Dictionary<string, UserList<T>> UserListsOfT { get; } = [];

        private readonly UserCollection _userCollection;

        private UserList(string collectionName)
        {
            if (string.IsNullOrWhiteSpace(collectionName)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(collectionName));
            _userCollection = UserCollectionLoader.GetCollection(collectionName) ?? throw new CellError($"Collection {collectionName} does not exist");
        }

        public static UserList<T> GetOrCreate(string collectionName)
        {
            if (UserList.AllUserListNames.Contains(collectionName))
            {
                return UserListsOfT[collectionName];
            }
            var newUserList = new UserList<T>(collectionName);
            UserList.AllUserListNames.Add(collectionName);
            UserListsOfT.Add(collectionName, newUserList);
            return UserListsOfT[collectionName];
        }

        public IEnumerator<T> GetEnumerator() => _userCollection?.Items.OfType<T>().GetEnumerator() ?? new List<T>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(T item)
        {
            _userCollection.Add(item);
        }

        public void Remove(T item)
        {
            _userCollection.Remove(item);
        }

        public void RemoveAt(int index)
        {
            if (_userCollection == null) return;
            if (index < 0 || index >= _userCollection.Items.Count) return;
            var model = _userCollection.Items[index];
            _userCollection.Remove(model);
        }

        public void Sort(Comparison<T> comparison)
        {
            _userCollection?.Sort((a, b) => comparison((T)a, (T)b));
        }

        public T? this[int key]
        {
            get => key >= 0 && key < _userCollection.Items.Count ? (T)_userCollection.Items[key] : new T();
        }

        public int Count => _userCollection?.Items.Count ?? 0;
    }

    public class UserList
    {
        private UserList()
        {
        }

        public static List<string> AllUserListNames { get; } = [];
    }
}
