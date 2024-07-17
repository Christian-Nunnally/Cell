﻿using Cell.Model.Plugin;
using Cell.Persistence;

namespace Cell.Plugin
{
    public class UserList<T> where T : PluginModel, new()
    {
        public static Dictionary<string, UserList<T>> UserListsOfT { get; } = [];

        private readonly string _collectionName;
        private readonly List<T> _orderedList;
        private readonly Dictionary<string, T> _idMap;

        private UserList(string collectionName)
        {
            _collectionName = collectionName;
            _orderedList = UserCollectionLoader.GetCollection(collectionName).Select(PluginModel.FromString<T>).ToList();
            _idMap = _orderedList.ToDictionary(item => item.ID);
        }

        public void Add(T item)
        {
            _orderedList.Add(item);
            _idMap.Add(item.ID, item);
            UserCollectionLoader.AddToCollection(_collectionName, item.ToString());
        }

        public T GetLast()
        {
            return _orderedList.Last();
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
    }

    public class UserList
    {
        public static List<string> AllUserListNames { get; } = [];

        private UserList() { }
    }
}
