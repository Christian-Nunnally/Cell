using Cell.Common;
using Cell.Model;
using Cell.ViewModel;

namespace Cell.Plugin
{
    /// <summary>
    /// Responsible for running other cells GetText functions when cells or collections they reference are updated.
    /// </summary>
    internal class CellPopulateManager
    {
        private readonly static Dictionary<string, CellModel> _cellsBeingUpdated = [];
        private readonly static Dictionary<string, Dictionary<CellModel, int>> _cellsToNotifyOnCollectionUpdates = [];
        private readonly static Dictionary<string, Dictionary<CellModel, int>> _cellsToNotifyOnLocationUpdates = [];
        private readonly static List<string> _collectionsBeingUpdated = [];
        private readonly static Dictionary<CellModel, Dictionary<string, int>> _collectionSubcriptionsMadeByCells = [];
        private readonly static Dictionary<string, List<ListCellViewModel>> _listCellsToUpdateWhenCollectionsChange = [];
        private readonly static Dictionary<CellModel, Dictionary<string, int>> _locationSubcriptionsMadeByCells = [];
        public static List<string> GetAllCollectionSubscriptions(CellModel subscriber)
        {
            return GetAllSubscriptions(subscriber, _collectionSubcriptionsMadeByCells);
        }

        public static List<string> GetAllLocationSubscriptions(CellModel subscriber)
        {
            return GetAllSubscriptions(subscriber, _locationSubcriptionsMadeByCells);
        }

        public static void NotifyCellValueUpdated(CellModel cell)
        {
            if (_cellsBeingUpdated.ContainsKey(cell.ID)) return;
            _cellsBeingUpdated.Add(cell.ID, cell);
            var key = cell.GetUnqiueLocationString();
            if (_cellsToNotifyOnLocationUpdates.TryGetValue(key, out var subscribers))
            {
                foreach (var subscriber in subscribers.Keys)
                {
                    var result = DynamicCellPluginExecutor.RunPopulate(new PluginContext(ApplicationViewModel.Instance, subscriber.Index), subscriber);
                    if (result.Success)
                    {
                        if (result.Result != null) subscriber.Text = result.Result;
                        subscriber.ErrorText = string.Empty;
                    }
                    else
                    {
                        if (result.Result != null) subscriber.Text = result.Result;// "Error";
                        if (result.Result != null) subscriber.ErrorText = result.Result;
                    }
                }
            }
            _cellsBeingUpdated.Remove(cell.ID);
        }

        public static void NotifyCollectionUpdated(string userCollectionName)
        {
            if (_collectionsBeingUpdated.Contains(userCollectionName)) return;
            _collectionsBeingUpdated.Add(userCollectionName);
            if (_cellsToNotifyOnCollectionUpdates.TryGetValue(userCollectionName, out var subscribers))
            {
                foreach (var subscriber in subscribers.Keys)
                {
                    var result = DynamicCellPluginExecutor.RunPopulate(new PluginContext(ApplicationViewModel.Instance, subscriber.Index), subscriber);
                    if (result.Success)
                    {
                        if (result.Result != null) subscriber.Text = result.Result;
                        subscriber.ErrorText = string.Empty;
                    }
                    else
                    {
                        if (result.Result != null) subscriber.Text = result.Result;// "Error";
                        if (result.Result != null) subscriber.ErrorText = result.Result;
                    }
                }
            }
            _collectionsBeingUpdated.Remove(userCollectionName);

            if (_listCellsToUpdateWhenCollectionsChange.ContainsKey(userCollectionName))
            {
                foreach (var listCell in _listCellsToUpdateWhenCollectionsChange[userCollectionName])
                {
                    listCell.UpdateList();
                }
            }
        }

        public static void StartMonitoringCellForUpdates(CellModel model)
        {
            model.AfterCellEdited += NotifyCellValueUpdated;
        }

        public static void StopMonitoringCellForUpdates(CellModel model)
        {
            model.AfterCellEdited -= NotifyCellValueUpdated;
        }

        public static void SubscribeToCollectionUpdates(CellModel subscriber, string collectionName)
        {
            SubscribeToReference(subscriber, collectionName, _cellsToNotifyOnCollectionUpdates, _collectionSubcriptionsMadeByCells);
        }

        public static void SubscribeToCollectionUpdates(ListCellViewModel subscriber, string collectionName)
        {
            if (_listCellsToUpdateWhenCollectionsChange.TryGetValue(collectionName, out var subscribers))
            {
                subscribers.Add(subscriber);
            }
            else
            {
                _listCellsToUpdateWhenCollectionsChange.Add(collectionName, [subscriber]);
            }
        }

        public static void SubscribeToUpdatesAtLocation(CellModel subscriber, string sheet, int row, int column)
        {
            var key = Utilities.GetUnqiueLocationString(sheet, row, column);
            SubscribeToReference(subscriber, key, _cellsToNotifyOnLocationUpdates, _locationSubcriptionsMadeByCells);
        }

        public static void UnsubscribeFromAllCollectionUpdates(CellModel model)
        {
            UnsubscribeFromAllReferences(model, _cellsToNotifyOnCollectionUpdates, _collectionSubcriptionsMadeByCells);
        }

        public static void UnsubscribeFromAllLocationUpdates(CellModel model)
        {
            UnsubscribeFromAllReferences(model, _cellsToNotifyOnLocationUpdates, _locationSubcriptionsMadeByCells);
        }

        public static void UnsubscribeFromCollectionUpdates(CellModel model, string collectionName)
        {
            UnsubscribeFromReference(model, collectionName, _cellsToNotifyOnCollectionUpdates, _collectionSubcriptionsMadeByCells);
        }

        public static void UnsubscribeFromCollectionUpdates(ListCellViewModel subscriber, string collectionName)
        {
            if (_listCellsToUpdateWhenCollectionsChange.TryGetValue(collectionName, out var subscribers))
            {
                subscribers.Remove(subscriber);
                if (subscribers.Count == 0) _listCellsToUpdateWhenCollectionsChange.Remove(collectionName);
            }
        }

        public static void UnsubscribeFromUpdatesAtLocation(CellModel model, string locationString)
        {
            UnsubscribeFromReference(model, locationString, _cellsToNotifyOnLocationUpdates, _locationSubcriptionsMadeByCells);
        }

        private static List<string> GetAllSubscriptions(
            CellModel model,
            Dictionary<CellModel, Dictionary<string, int>> subscriberToReferenceMap)
        {
            if (!subscriberToReferenceMap.TryGetValue(model, out var subscriptions)) return new();
            return [.. subscriptions.Keys];
        }

        private static void SubscribeToReference(
            CellModel subscriber,
            string reference,
            Dictionary<string, Dictionary<CellModel, int>> referenceToSubscriberMap,
            Dictionary<CellModel, Dictionary<string, int>> subscriberToReferenceMap)
        {
            if (referenceToSubscriberMap.TryGetValue(reference, out var subscribers))
            {
                if (subscribers.TryGetValue(subscriber, out int value)) subscribers[subscriber] = ++value;
                else subscribers.Add(subscriber, 1);
            }
            else
            {
                var refCountDictionary = new Dictionary<CellModel, int> { { subscriber, 1 } };
                referenceToSubscriberMap.Add(reference, refCountDictionary);
            }

            if (subscriberToReferenceMap.TryGetValue(subscriber, out var subscriptions))
            {
                if (subscriptions.TryGetValue(reference, out int value)) subscriptions[reference] = ++value;
                else subscriptions.Add(reference, 1);
            }
            else
            {
                var refCountDictionary = new Dictionary<string, int> { { reference, 1 } };
                subscriberToReferenceMap.Add(subscriber, refCountDictionary);
            }
        }

        private static void UnsubscribeFromAllReferences(
            CellModel model,
            Dictionary<string, Dictionary<CellModel, int>> referenceToSubscriberMap,
            Dictionary<CellModel, Dictionary<string, int>> subscriberToReferenceMap)
        {
            if (!subscriberToReferenceMap.TryGetValue(model, out var subscriptions)) return;
            foreach (var subscription in subscriptions.ToList())
            {
                UnsubscribeFromReference(model, subscription.Key, referenceToSubscriberMap, subscriberToReferenceMap);
            }
        }

        private static void UnsubscribeFromReference(
            CellModel model,
            string locationString,
            Dictionary<string, Dictionary<CellModel, int>> referenceToSubscriberMap,
            Dictionary<CellModel, Dictionary<string, int>> subscriberToReferenceMap)
        {
            if (!subscriberToReferenceMap.TryGetValue(model, out var subscriptions)) return;
            if (!subscriptions.TryGetValue(locationString, out var count)) return;
            if (count == 1) subscriptions.Remove(locationString);
            else subscriptions[locationString] = --count;
            if (subscriptions.Count == 0) subscriberToReferenceMap.Remove(model);

            if (!referenceToSubscriberMap.TryGetValue(locationString, out var subscribers)) return;
            if (!subscribers.TryGetValue(model, out var value)) return;
            if (value == 1) subscribers.Remove(model);
            else subscribers[model] = --value;
            if (subscribers.Count == 0) referenceToSubscriberMap.Remove(locationString);
        }
    }
}
