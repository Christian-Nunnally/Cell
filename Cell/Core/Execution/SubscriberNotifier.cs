namespace Cell.Execution
{
    /// <summary>
    /// Maintains subscriptions to 'Channels' which are arbitrary strings. When a channel is published to, all subscribers to that channel have thier actions invoked.
    /// Subscriptions are ref counted, so a subscriber can subscribe to a channel multiple times and will only be unsubscribed when the ref count reaches 0.
    /// Subscribers will only be notified once even if they subscribe twice. If they subscribe twice, they will need to unsubscribe twice to stop being notified.
    /// The first time a channel is subscribed to, NewChannelSubscribedTo will be invoked.
    /// When the last reference to a channel is unsubscribed from LastChannelUnsubscribedFrom will be invoked.
    /// </summary>
    public class SubscriberNotifier
    {
        private readonly Dictionary<string, Dictionary<ISubscriber, int>> _channelToSubscriberMap = [];
        private readonly Dictionary<ISubscriber, Dictionary<string, int>> _subscriberToChannelMap = [];
        public event Action<string>? LastChannelUnsubscribedFrom;

        public event Action<string>? NewChannelSubscribedTo;

        public IEnumerable<string> GetChannelsSubscriberIsSubscribedTo(ISubscriber subscriber)
        {
            return _subscriberToChannelMap.TryGetValue(subscriber, out var subscriptions) ? [.. subscriptions.Keys] : [];
        }

        public IEnumerable<ISubscriber> GetSubscribersToChannel(string channel)
        {
            return _channelToSubscriberMap.TryGetValue(channel, out var subscribers) ? [.. subscribers.Keys] : [];
        }

        public void NotifySubscribers(string reference)
        {
            var subscribers = GetSubscribersToChannel(reference);
            foreach (var subscriber in subscribers)
            {
                subscriber.Action();
            }
        }

        public void SubscribeToChannel(ISubscriber subscriber, string channel)
        {
            if (_channelToSubscriberMap.TryGetValue(channel, out var subscribers))
            {
                if (subscribers.TryGetValue(subscriber, out int value)) subscribers[subscriber] = ++value;
                else subscribers.Add(subscriber, 1);
            }
            else
            {
                var refCountDictionary = new Dictionary<ISubscriber, int> { { subscriber, 1 } };
                _channelToSubscriberMap.Add(channel, refCountDictionary);
                NewChannelSubscribedTo?.Invoke(channel);
            }

            if (_subscriberToChannelMap.TryGetValue(subscriber, out var subscriptions))
            {
                if (subscriptions.TryGetValue(channel, out int value)) subscriptions[channel] = ++value;
                else subscriptions.Add(channel, 1);
            }
            else
            {
                var refCountDictionary = new Dictionary<string, int> { { channel, 1 } };
                _subscriberToChannelMap.Add(subscriber, refCountDictionary);
            }
        }

        public void UnsubscribeFromAllChannels(ISubscriber subscriber)
        {
            if (!_subscriberToChannelMap.TryGetValue(subscriber, out var subscriptions)) return;
            foreach (var subscription in subscriptions.ToList())
            {
                UnsubscribeFromChannel(subscriber, subscription.Key);
            }
        }

        private void UnsubscribeFromChannel(ISubscriber subscriber, string channel)
        {
            if (!_subscriberToChannelMap.TryGetValue(subscriber, out var subscriptions)) return;
            if (!subscriptions.TryGetValue(channel, out var count)) return;
            if (count == 1) subscriptions.Remove(channel);
            else subscriptions[channel] = --count;
            if (subscriptions.Count == 0) _subscriberToChannelMap.Remove(subscriber);

            if (!_channelToSubscriberMap.TryGetValue(channel, out var subscribers)) return;
            if (!subscribers.TryGetValue(subscriber, out var value)) return;
            if (value == 1) subscribers.Remove(subscriber);
            else subscribers[subscriber] = --value;
            if (subscribers.Count == 0)
            {
                _channelToSubscriberMap.Remove(channel);
                LastChannelUnsubscribedFrom?.Invoke(channel);
            }
        }
    }
}
