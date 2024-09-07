using Cell.Common;
using Cell.Model.Plugin;
using Cell.Persistence;
using System.Collections;

namespace Cell.Data
{
    public class UserList<T> : IEnumerable<T> where T : PluginModel, new()
    {
        private readonly string _collectionName;
        private readonly UserCollectionLoader _userCollectionLoader;
        private UserCollection? _internalUserCollection;

        private UserCollection UserCollection => _internalUserCollection ??= _userCollectionLoader.GetCollection(_collectionName) ?? throw new CellError($"Collection {_collectionName} does not exist");

        private UserList(string collectionName, UserCollectionLoader userCollectionLoader)
        {
            _collectionName = collectionName;
            _userCollectionLoader = userCollectionLoader;
        }

        public static UserList<T> GetOrCreate(string collectionName, UserCollectionLoader userCollectionLoader)
        {
            return new UserList<T>(collectionName, userCollectionLoader);
        }

        public IEnumerator<T> GetEnumerator() => UserCollection?.Items.OfType<T>().GetEnumerator() ?? new List<T>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(T item)
        {
            UserCollection.Add(item);
        }

        public void Remove(T item)
        {
            UserCollection.Remove(item);
        }

        public void RemoveAt(int index)
        {
            if (UserCollection == null) return;
            if (index < 0 || index >= UserCollection.Items.Count) return;
            var model = UserCollection.Items[index];
            UserCollection.Remove(model);
        }

        public T? this[int key]
        {
            get => key >= 0 && key < UserCollection.Items.Count ? (T)UserCollection.Items[key] : new T();
        }

        public int Count => UserCollection?.Items.Count ?? 0;
    }
}
