using Cell.Model.Plugin;
using Cell.Persistence;
using System.Collections;

namespace Cell.Plugin
{
    public class UserList<T> : IEnumerable<T> where T : PluginModel, new()
    {
        public static Dictionary<string, UserList<T>> UserListsOfT { get; } = [];

        private readonly string _collectionName;
        private readonly UserCollection _userCollection;

        private UserList(string collectionName)
        {
            _collectionName = collectionName;
            _userCollection = UserCollectionLoader.GetCollection(collectionName) ?? throw new Exception("Can't find userlist");
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
            UserCollectionLoader.AddToCollection(_collectionName, item);
        }

        public void Remove(T item)
        {
            UserCollectionLoader.RemoveFromCollection(_collectionName, item.ID);
        }

        public void RemoveAt(int index)
        {
            if (_userCollection == null) return;
            if (index < 0 || index >= _userCollection.Items.Count) return;
            var model = _userCollection.Items[index];
            _userCollection.Remove(model);
            UserCollectionLoader.RemoveFromCollection(_collectionName, model.ID);
        }

        public void Sort(Comparison<T> comparison)
        {
            _userCollection?.Sort((a, b) => comparison((T)a, (T)b));
        }

        public T? this[int key]
        {
            get => key >= 0 && key < _userCollection.Items.Count ? (T)_userCollection.Items[key] : new T();
        }
    }

    public class UserList
    {
        public static List<string> AllUserListNames { get; } = [];

        private UserList() { }
    }
}
