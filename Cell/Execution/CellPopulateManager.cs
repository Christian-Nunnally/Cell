﻿using Cell.Common;
using Cell.Data;
using Cell.Model;
using Cell.Model.Plugin;
using Cell.Persistence;
using Cell.ViewModel.Cells.Types;
using Cell.ViewModel.Execution;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Cell.Execution
{
    /// <summary>
    /// Responsible for running other cells GetText functions when cells or collections they reference are updated.
    /// </summary>
    public class CellPopulateManager
    {
        private readonly Dictionary<string, CellModel> _cellsBeingUpdated = [];
        private readonly Dictionary<string, Dictionary<CellModel, int>> _cellsToNotifyOnCollectionUpdates = [];
        private readonly Dictionary<string, Dictionary<CellModel, int>> _cellsToNotifyOnValueUpdatesAtLocation = [];
        private readonly Dictionary<PluginFunctionModel, List<CellModel>> _cellsToUpdateWhenFunctionChanges = [];
        private readonly Dictionary<CellModel, string> _cellToPopulateFunctionNameMap = [];
        private readonly List<string> _collectionsBeingUpdated = [];
        private readonly Dictionary<CellModel, Dictionary<string, int>> _collectionSubcriptionsMadeByCells = [];
        private readonly Dictionary<string, List<ListCellViewModel>> _listCellsToUpdateWhenCollectionsChange = [];
        private readonly Dictionary<CellModel, Dictionary<string, int>> _locationSubcriptionsMadeByCells = [];
        private readonly PluginFunctionLoader _pluginFunctionLoader;
        private readonly CellTracker _cellTracker;
        private readonly UserCollectionLoader _userCollectionLoader;

        public CellPopulateManager(CellTracker cellTracker, PluginFunctionLoader pluginFunctionLoader, UserCollectionLoader userCollectionLoader)
        {
            _userCollectionLoader = userCollectionLoader;
            foreach (var userCollection in _userCollectionLoader.ObservableCollections)
            {
                userCollection.ItemAdded += ItemAddedToUserCollection;
                userCollection.ItemRemoved += ItemRemovedFromUserCollection;
                userCollection.ItemPropertyChanged += ItemPropertyChangedInUserCollection;
            }
            _userCollectionLoader.ObservableCollections.CollectionChanged += UserCollectionsCollectionChanged;
            _cellTracker = cellTracker;
            _cellTracker.CellAdded += StartMonitoringCellForUpdates;
            _cellTracker.CellRemoved += StopMonitoringCellForUpdates;
            foreach (var cell in _cellTracker.AllCells)
            {
                // TODO: Only monitor cells that a populate function depends on.
                StartMonitoringCellForUpdates(cell);
            }
            _pluginFunctionLoader = pluginFunctionLoader;
        }

        private void ItemPropertyChangedInUserCollection(UserCollection collection, PluginModel model)
        {
            NotifyCollectionUpdated(collection.Name);
        }

        private void ItemRemovedFromUserCollection(UserCollection collection, PluginModel model)
        {
            NotifyCollectionUpdated(collection.Name);
        }

        private void ItemAddedToUserCollection(UserCollection collection, PluginModel model)
        {
            NotifyCollectionUpdated(collection.Name);
        }

        private void UserCollectionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems?[0] is UserCollection userCollection)
                {
                    userCollection.ItemAdded += ItemAddedToUserCollection;
                    userCollection.ItemRemoved += ItemRemovedFromUserCollection;
                    userCollection.ItemPropertyChanged += ItemPropertyChangedInUserCollection;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems?[0] is UserCollection userCollection)
                {
                    userCollection.ItemAdded -= ItemAddedToUserCollection;
                    userCollection.ItemRemoved -= ItemRemovedFromUserCollection;
                    userCollection.ItemPropertyChanged -= ItemPropertyChangedInUserCollection;
                }
            }
        }

        public List<string> GetAllCollectionSubscriptions(CellModel subscriber)
        {
            return GetAllSubscriptions(subscriber, _collectionSubcriptionsMadeByCells);
        }

        public List<string> GetAllLocationSubscriptions(CellModel subscriber)
        {
            return GetAllSubscriptions(subscriber, _locationSubcriptionsMadeByCells);
        }

        public void NotifyCellValueUpdated(CellModel cell)
        {
            if (_cellsBeingUpdated.ContainsKey(cell.ID)) return;
            _cellsBeingUpdated.Add(cell.ID, cell);
            var key = cell.GetUnqiueLocationString();
            if (_cellsToNotifyOnValueUpdatesAtLocation.TryGetValue(key, out var subscribers))
            {
                RunPopulateForSubscribers(subscribers);
            }
            _cellsBeingUpdated.Remove(cell.ID);
        }

        public void NotifyCollectionUpdated(string userCollectionName)
        {
            if (_collectionsBeingUpdated.Contains(userCollectionName)) return;
            _collectionsBeingUpdated.Add(userCollectionName);
            if (_cellsToNotifyOnCollectionUpdates.TryGetValue(userCollectionName, out var subscribers))
            {
                RunPopulateForSubscribers(subscribers);
            }
            _collectionsBeingUpdated.Remove(userCollectionName);

            if (_listCellsToUpdateWhenCollectionsChange.TryGetValue(userCollectionName, out List<ListCellViewModel>? value))
            {
                foreach (var listCell in value)
                {
                    listCell.UpdateList();
                }
            }
        }

        public void StartMonitoringCellForUpdates(CellModel cell)
        {
            cell.AfterCellEdited += NotifyCellValueUpdated;
            cell.PropertyChanged += CellPropertyChanged;
            if (!string.IsNullOrWhiteSpace(cell.PopulateFunctionName))
            {
                if (_pluginFunctionLoader.TryGetFunction("object", cell.PopulateFunctionName, out var function))
                {
                    UpdateDependencySubscriptions(cell, function);
                    _cellToPopulateFunctionNameMap[cell] = cell.PopulateFunctionName;
                    AddToCellsToUpdateWhenFunctionChangesMap(cell, function);
                }
            }
        }

        private void AddToCellsToUpdateWhenFunctionChangesMap(CellModel cell, FunctionViewModel function)
        {
            if (_cellsToUpdateWhenFunctionChanges.TryGetValue(function.Model, out var cellList))
            {
                if (!cellList.Contains(cell)) cellList.Add(cell);
            }
            else
            {
                _cellsToUpdateWhenFunctionChanges.Add(function.Model, [cell]);
                function.DependenciesChanged += NotifyCellsAboutFunctionDependencyChanges;
                function.Model.PropertyChanged += PopulateFunctionPropertyChanged;
            }
        }

        public void StopMonitoringCellForUpdates(CellModel cell)
        {
            cell.AfterCellEdited -= NotifyCellValueUpdated;
            cell.PropertyChanged -= CellPropertyChanged;
            if (!string.IsNullOrWhiteSpace(cell.PopulateFunctionName))
            {
                if (_pluginFunctionLoader.TryGetFunction("object", cell.PopulateFunctionName, out var function))
                {
                    UpdateDependencySubscriptions(cell, function);
                    _cellToPopulateFunctionNameMap.Remove(cell);
                    RemoveFromCellsToUpdateWhenFunctionChangesMap(cell, function);
                }
            }
            UnsubscribeFromAllCollectionUpdates(cell);
            UnsubscribeFromAllLocationUpdates(cell);
        }

        private void RemoveFromCellsToUpdateWhenFunctionChangesMap(CellModel cell, FunctionViewModel function)
        {
            if (_cellsToUpdateWhenFunctionChanges.TryGetValue(function.Model, out var cellList))
            {
                cellList.Remove(cell);
                if (cellList.Count == 0)
                {
                    _cellsToUpdateWhenFunctionChanges.Remove(function.Model);
                    function.DependenciesChanged -= NotifyCellsAboutFunctionDependencyChanges;
                    function.Model.PropertyChanged -= PopulateFunctionPropertyChanged;
                }
            }
        }

        private void PopulateFunctionPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not PluginFunctionModel pluginFunctionModel) return;
            if (e.PropertyName == nameof(PluginFunctionModel.Code))
            {
                _cellsToUpdateWhenFunctionChanges[pluginFunctionModel].ForEach(PopulateCellsText);
            }
        }

        public void PopulateCellsText(CellModel cell)
        {
            if (string.IsNullOrEmpty(cell.PopulateFunctionName)) return;
            var result = DynamicCellPluginExecutor.RunPopulate(_pluginFunctionLoader, new PluginContext(_cellTracker, _userCollectionLoader, cell.Index), cell);
            if (result.Result == null) return;
            if (result.Success) cell.Text = result.Result;
            else cell.ErrorText = result.Result;
        }

        public void SubscribeToCollectionUpdates(CellModel subscriber, string collectionName)
        {
            SubscribeToReference(subscriber, collectionName, _cellsToNotifyOnCollectionUpdates, _collectionSubcriptionsMadeByCells);
        }

        public void SubscribeToCollectionUpdates(ListCellViewModel subscriber, string collectionName)
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

        public void SubscribeToUpdatesAtLocation(CellModel subscriber, string sheet, int row, int column)
        {
            var key = Utilities.GetUnqiueLocationString(sheet, row, column);
            SubscribeToReference(subscriber, key, _cellsToNotifyOnValueUpdatesAtLocation, _locationSubcriptionsMadeByCells);
        }

        public void UnsubscribeFromAllCollectionUpdates(CellModel model)
        {
            UnsubscribeFromAllReferences(model, _cellsToNotifyOnCollectionUpdates, _collectionSubcriptionsMadeByCells);
        }

        public void UnsubscribeFromAllLocationUpdates(CellModel model)
        {
            UnsubscribeFromAllReferences(model, _cellsToNotifyOnValueUpdatesAtLocation, _locationSubcriptionsMadeByCells);
        }

        public void UnsubscribeFromCollectionUpdates(CellModel model, string collectionName)
        {
            UnsubscribeFromReference(model, collectionName, _cellsToNotifyOnCollectionUpdates, _collectionSubcriptionsMadeByCells);
        }

        public void UnsubscribeFromCollectionUpdates(ListCellViewModel subscriber, string collectionName)
        {
            if (_listCellsToUpdateWhenCollectionsChange.TryGetValue(collectionName, out var subscribers))
            {
                subscribers.Remove(subscriber);
                if (subscribers.Count == 0) _listCellsToUpdateWhenCollectionsChange.Remove(collectionName);
            }
        }

        public void UnsubscribeFromUpdatesAtLocation(CellModel model, string locationString)
        {
            UnsubscribeFromReference(model, locationString, _cellsToNotifyOnValueUpdatesAtLocation, _locationSubcriptionsMadeByCells);
        }

        public void UpdateDependencySubscriptions(CellModel cell, FunctionViewModel function)
        {
            if (string.IsNullOrWhiteSpace(cell.SheetName)) return;
            var _ = function.CompiledMethod;

            UnsubscribeFromAllLocationUpdates(cell);
            UnsubscribeFromAllCollectionUpdates(cell);
            if (!string.IsNullOrWhiteSpace(function.Model.Code))
            {
                foreach (var locationDependency in function.LocationDependencies)
                {
                    var sheetName = string.IsNullOrWhiteSpace(locationDependency.SheetName) ? cell.SheetName : locationDependency.SheetName;

                    var row = locationDependency.ResolveRow(cell);
                    var column = locationDependency.ResolveColumn(cell);
                    if (row == cell.Row && column == cell.Column) continue;
                    if (locationDependency.IsRange)
                    {
                        var rowRangeEnd = locationDependency.ResolveRowRangeEnd(cell);
                        var columnRangeEnd = locationDependency.ResolveColumnRangeEnd(cell);
                        for (var r = row; r <= rowRangeEnd; r++)
                        {
                            for (var c = column; c <= columnRangeEnd; c++)
                            {
                                SubscribeToUpdatesAtLocation(cell, sheetName, r, c);
                            }
                        }
                    }
                    else
                    {
                        SubscribeToUpdatesAtLocation(cell, sheetName, row, column);
                    }
                }
                SubscribeToUpdatesAtLocation(cell, cell.SheetName, cell.Row, cell.Column);
                foreach (var collectionName in function.CollectionDependencies)
                {
                    SubscribeToCollectionUpdates(cell, collectionName);
                }
            }
        }

        private static List<string> GetAllSubscriptions(
            CellModel model,
            Dictionary<CellModel, Dictionary<string, int>> subscriberToReferenceMap)
        {
            if (!subscriberToReferenceMap.TryGetValue(model, out var subscriptions)) return [];
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

        private void CellPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellModel.PopulateFunctionName) || e.PropertyName == nameof(CellModel.SheetName))
            {
                if (sender is not CellModel cell) return;
                if (_cellToPopulateFunctionNameMap.TryGetValue(cell, out var oldFunctionName))
                {
                    if (_pluginFunctionLoader.TryGetFunction("object", oldFunctionName, out var oldFunction))
                    {
                        RemoveFromCellsToUpdateWhenFunctionChangesMap(cell, oldFunction);
                    }
                    _cellToPopulateFunctionNameMap.Remove(cell);
                }

                if (_pluginFunctionLoader.TryGetFunction("object", cell.PopulateFunctionName, out var function))
                {
                    // TODO: split into add and remove.
                    UpdateDependencySubscriptions(cell, function);
                    AddToCellsToUpdateWhenFunctionChangesMap(cell, function);
                    _cellToPopulateFunctionNameMap[cell] = cell.PopulateFunctionName;
                }
            }
        }

        private void NotifyCellsAboutFunctionDependencyChanges(FunctionViewModel function) => _cellsToUpdateWhenFunctionChanges[function.Model].ForEach(cell => UpdateDependencySubscriptions(cell, function));

        private void RunPopulateForSubscriber(CellModel subscriber)
        {
            var pluginContext = new PluginContext(_cellTracker, _userCollectionLoader,  subscriber.Index, subscriber);
            var result = DynamicCellPluginExecutor.RunPopulate(_pluginFunctionLoader, pluginContext, subscriber);
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

        private void RunPopulateForSubscribers(Dictionary<CellModel, int> subscribers)
        {
            foreach (var subscriber in subscribers.Keys.ToList())
            {
                RunPopulateForSubscriber(subscriber);
            }
        }

        public List<CellModel> GetCellsThatUsePopulateFunction(FunctionViewModel function)
        {
            return _cellsToUpdateWhenFunctionChanges.TryGetValue(function.Model, out var cells) ? cells : [];
        }
    }
}
