using Cell.Model;
using System.Collections;

namespace Cell.Core.Data
{
    /// <summary>
    /// A user created list of items that can be used from a cell function.
    /// </summary>
    public class UserList : IEnumerable<UserItem>
    {
        private readonly string _collectionName;
        private readonly IUserCollectionProvider _userCollectionLoader;
        private UserCollection? _internalUserCollection;

        private UserCollection? UserCollection => _internalUserCollection ??= _userCollectionLoader.GetCollection(_collectionName ?? "");

        /// <summary>
        /// Gets or creates wrapper that loads a list from the given loader and provides an interface for it.
        /// </summary>
        /// <param name="collectionName">The name of the collection to get if it exists or create if it doesn't.</param>
        /// <param name="userCollectionLoader">The loader to look for existing collections in.</param>
        /// <returns>The wrapping list object.</returns>
        public UserList(string collectionName, IUserCollectionProvider userCollectionLoader)
        {
            _collectionName = collectionName;
            _userCollectionLoader = userCollectionLoader;
        }

        /// <summary>
        /// Gets an enumerator that iterates through the items in this list.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<UserItem> GetEnumerator() => UserCollection?.Items.GetEnumerator() ?? new List<UserItem>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Adds an item to the user collection.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(UserItem item)
        {
            UserCollection?.Add(item);
        }

        /// <summary>
        /// Removes an item from the user collection, this will delete the item.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        public void Remove(UserItem item)
        {
            UserCollection?.Remove(item);
        }

        /// <summary>
        /// Removes an item from the user collection at the given index.
        /// </summary>
        /// <param name="index">The index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            if (UserCollection is null) return;
            if (index < 0 || index >= UserCollection.Items.Count) return;
            var model = UserCollection.Items[index];
            UserCollection.Remove(model);
        }

        /// <summary>
        /// Gets the item at the given index.
        /// </summary>
        /// <param name="index">The index of the item to get.</param>
        /// <returns></returns>
        public UserItem this[int index]
        {
            get => (UserCollection is not null && index >= 0 && index < UserCollection.Items.Count) 
                ? UserCollection.Items[index]
                : UserItem.Null;
        }

        /// <summary>
        /// Gets the number of items in the collection.
        /// </summary>
        public int Count => UserCollection?.Items.Count ?? 0;
    }
}
