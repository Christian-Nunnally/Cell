using Cell.Data;
using Cell.Model.Plugin;
using Cell.Persistence;

namespace Cell.Execution
{
    /// <summary>
    /// Notifies subscribers when a collection or its contents have been updated.
    /// </summary>
    public class CollectionChangeNotifier
    {
        private readonly List<string> _collectionsBeingUpdated = [];
        private readonly SubscriberNotifier _subscriberNotifier = new();
        private readonly UserCollectionLoader _userCollectionLoader;
        /// <summary>
        /// Creates a new instance of <see cref="CollectionChangeNotifier"/>.
        /// </summary>
        /// <param name="userCollectionLoader">The collection loader to track the collections of.</param>
        public CollectionChangeNotifier(UserCollectionLoader userCollectionLoader)
        {
            _userCollectionLoader = userCollectionLoader;
            _subscriberNotifier.NewChannelSubscribedTo += StartListeningToCollectionForChanges;
            _subscriberNotifier.LastChannelUnsubscribedFrom += StopListeningToCollectionForChanges;
        }

        /// <summary>
        /// Gets the collections that a subscriber is subscribed to.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        /// <returns>The list of collection names the subscriber is subscribed to.</returns>
        public IEnumerable<string> GetCollectionsSubscriberIsSubscribedTo(ISubscriber subscriber) => _subscriberNotifier.GetChannelsSubscriberIsSubscribedTo(subscriber);

        /// <summary>
        /// Subscribes a subscriber to updates for a collection.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        /// <param name="collectionName">The collection name.</param>
        public void SubscribeToCollectionUpdates(ISubscriber subscriber, string collectionName)
        {
            _subscriberNotifier.SubscribeToChannel(subscriber, collectionName);
        }

        /// <summary>
        /// Unsubscribes a subscriber from updates for all collections.
        /// </summary>
        /// <param name="subscriber">The subscriber to unsubscribe.</param>
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
