using Cell.Data;
using Cell.Model.Plugin;
using Cell.Persistence;

namespace Cell.Execution
{
    public class CollectionChangeNotifier
    {
        private readonly List<string> _collectionsBeingUpdated = [];
        private readonly SubscriberNotifier _subscriberNotifier = new();
        private readonly UserCollectionLoader _userCollectionLoader;
        public CollectionChangeNotifier(UserCollectionLoader userCollectionLoader)
        {
            _userCollectionLoader = userCollectionLoader;
            _subscriberNotifier.NewChannelSubscribedTo += StartListeningToCollectionForChanges;
            _subscriberNotifier.LastChannelUnsubscribedFrom += StopListeningToCollectionForChanges;
        }

        public IEnumerable<string> GetCollectionsSubscriberIsSubscribedTo(ISubscriber subscriber) => _subscriberNotifier.GetChannelsSubscriberIsSubscribedTo(subscriber);

        public void SubscribeToCollectionUpdates(ISubscriber subscriber, string collectionName)
        {
            _subscriberNotifier.SubscribeToChannel(subscriber, collectionName);
        }

        public void UnsubscribeFromAllCollections(ISubscriber subscriber)
        {
            _subscriberNotifier.UnsubscribeFromAllChannels(subscriber);
        }

        private void ItemAddedToUserCollection(UserCollection collection, PluginModel model)
        {
            NotifyCollectionUpdated(collection.Name);
        }

        private void ItemPropertyChangedInUserCollection(UserCollection collection, PluginModel model)
        {
            NotifyCollectionUpdated(collection.Name);
        }

        private void ItemRemovedFromUserCollection(UserCollection collection, PluginModel model)
        {
            NotifyCollectionUpdated(collection.Name);
        }

        private void NotifyCollectionUpdated(string userCollectionName)
        {
            if (_collectionsBeingUpdated.Contains(userCollectionName)) return;
            _collectionsBeingUpdated.Add(userCollectionName);
            _subscriberNotifier.NotifySubscribers(userCollectionName);
            _collectionsBeingUpdated.Remove(userCollectionName);
        }

        private void StartListeningToCollectionForChanges(string collectionName)
        {
            var collection = _userCollectionLoader.GetCollection(collectionName);
            if (collection == null) return;
            collection.ItemAdded += ItemAddedToUserCollection;
            collection.ItemRemoved += ItemRemovedFromUserCollection;
            collection.ItemPropertyChanged += ItemPropertyChangedInUserCollection;
        }

        private void StopListeningToCollectionForChanges(string collectionName)
        {
            var collection = _userCollectionLoader.GetCollection(collectionName);
            if (collection == null) return;
            collection.ItemAdded -= ItemAddedToUserCollection;
            collection.ItemRemoved -= ItemRemovedFromUserCollection;
            collection.ItemPropertyChanged -= ItemPropertyChangedInUserCollection;
        }
    }
}
