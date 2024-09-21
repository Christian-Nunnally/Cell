using Cell.Common;
using Cell.Data;
using Cell.Model;
using Cell.Persistence;
using Cell.ViewModel.Execution;
using System.ComponentModel;

namespace Cell.Execution
{
    /// <summary>
    /// Responsible for running other cells GetText functions when cells or collections they reference are updated.
    /// </summary>
    public class CellPopulateManager
    {
        private readonly Dictionary<CellModel, List<CellSpecificCollectionReference>> _collectionDependenciesForCellsPopulateFunction = [];
        private readonly Dictionary<PluginFunctionModel, List<CellModel>> _cellsToUpdateWhenFunctionChanges = [];
        private readonly CellTextChangesAtLocationNotifier _cellTextChangesAtLocationNotifier;
        private readonly Dictionary<CellModel, string> _cellToPopulateFunctionNameMap = [];
        private readonly Dictionary<CellModel, CellPopulateSubscriber> _cellToPopulateSubscriberMap = [];
        private readonly CellTracker _cellTracker;
        private readonly CollectionChangeNotifier _collectionChangeNotifier;
        private readonly PluginFunctionLoader _pluginFunctionLoader;
        private readonly UserCollectionLoader _userCollectionLoader;
        private readonly PluginContext _pluginFunctionRunContext;
        public CellPopulateManager(CellTracker cellTracker, PluginFunctionLoader pluginFunctionLoader, UserCollectionLoader userCollectionLoader)
        {
            _pluginFunctionRunContext = new PluginContext(cellTracker, userCollectionLoader);
            _cellTextChangesAtLocationNotifier = new CellTextChangesAtLocationNotifier(cellTracker);
            _collectionChangeNotifier = new CollectionChangeNotifier(userCollectionLoader);
            _userCollectionLoader = userCollectionLoader;
            _cellTracker = cellTracker;
            _cellTracker.CellAdded += StartMonitoringCellForChanges;
            _cellTracker.CellRemoved += StopMonitoringCellForChanges;
            foreach (var cell in _cellTracker.AllCells)
            {
                StartMonitoringCellForChanges(cell);
            }
            _pluginFunctionLoader = pluginFunctionLoader;
        }

        public IEnumerable<string> GetAllCollectionSubscriptions(CellModel cell)
        {
            var subscriber = GetOrCreatePopulateSubscriber(cell);
            return _collectionChangeNotifier.GetCollectionsSubscriberIsSubscribedTo(subscriber);
        }

        public IEnumerable<string> GetAllLocationSubscriptions(CellModel cell)
        {
            var subscriber = GetOrCreatePopulateSubscriber(cell);
            return _cellTextChangesAtLocationNotifier.GetLocationsSubscriberIsSubscribedTo(subscriber);
        }

        public List<CellModel> GetCellsThatUsePopulateFunction(PluginFunction function)
        {
            return _cellsToUpdateWhenFunctionChanges.TryGetValue(function.Model, out var cells) ? cells : [];
        }

        private void PopulateCellsText(CellModel cell)
        {
            if (string.IsNullOrEmpty(cell.PopulateFunctionName)) return;
            var result = DynamicCellPluginExecutor.RunPopulate(_pluginFunctionLoader, new PluginContext(_cellTracker, _userCollectionLoader, cell.Index), cell);
            if (result.ExecutionResult == null) return;
            if (result.WasSuccess) cell.Text = result.ExecutionResult;
            else cell.ErrorText = result.ExecutionResult;
        }

        private void StartMonitoringCellForChanges(CellModel cell)
        {
            cell.PropertyChanged += CellPropertyChanged;
            if (string.IsNullOrWhiteSpace(cell.PopulateFunctionName)) return;
            if (!_pluginFunctionLoader.TryGetFunction("object", cell.PopulateFunctionName, out var function)) return;
            UpdateDependencySubscriptions(cell, function);
            _cellToPopulateFunctionNameMap[cell] = cell.PopulateFunctionName;
            AddToCellsToUpdateWhenFunctionChangesMap(cell, function);
        }

        private void StopMonitoringCellForChanges(CellModel cell)
        {
            UnsubscribeFromAllCollectionUpdates(cell);
            UnsubscribeFromAllLocationUpdates(cell);
            cell.PropertyChanged -= CellPropertyChanged;
            if (string.IsNullOrWhiteSpace(cell.PopulateFunctionName)) return;
            if (!_pluginFunctionLoader.TryGetFunction("object", cell.PopulateFunctionName, out var function)) return;
            UpdateDependencySubscriptions(cell, function);
            _cellToPopulateFunctionNameMap.Remove(cell);
            RemoveFromCellsToUpdateWhenFunctionChangesMap(cell, function);
        }

        private void SubscribeToCollectionUpdates(CellModel cell, string collectionName)
        {
            var subscriber = GetOrCreatePopulateSubscriber(cell);
            _collectionChangeNotifier.SubscribeToCollectionUpdates(subscriber, collectionName);
        }

        private void SubscribeToUpdatesAtLocation(CellModel cell, string locationString)
        {
            var subscriber = GetOrCreatePopulateSubscriber(cell);
            _cellTextChangesAtLocationNotifier.SubscribeToUpdatesAtLocation(subscriber, locationString);
        }

        private void UnsubscribeFromAllCollectionUpdates(CellModel cell)
        {
            var subscriber = GetOrCreatePopulateSubscriber(cell);
            _collectionChangeNotifier.UnsubscribeFromAllCollections(subscriber);
            _collectionDependenciesForCellsPopulateFunction.Remove(cell);
        }

        private void UnsubscribeFromAllLocationUpdates(CellModel cell)
        {
            var subscriber = GetOrCreatePopulateSubscriber(cell);
            _cellTextChangesAtLocationNotifier.UnsubscribeFromAllLocations(subscriber);
        }

        private void UpdateDependencySubscriptions(CellModel cell, PluginFunction function)
        {
            if (string.IsNullOrWhiteSpace(cell.SheetName)) return;
            var _ = function.CompiledMethod;

            UntrackCollectionReferences(cell);
            UnsubscribeFromAllLocationUpdates(cell);
            UnsubscribeFromAllCollectionUpdates(cell);
            ResolveLocationDependenciesForCell(cell, function);
            ResolveCollectionDependenciesForCell(cell, function);
        }

        private void ResolveCollectionDependenciesForCell(CellModel cell, PluginFunction function)
        {
            UnsubscribeFromAllCollectionUpdates(cell);
            foreach (var collectionReference in function.CollectionDependencies)
            {
                var cellSpecificCollectionReference = new CellSpecificCollectionReference(cell, collectionReference, _cellTextChangesAtLocationNotifier, _pluginFunctionRunContext);
                TrackCollectionReference(cell, cellSpecificCollectionReference);

                var collectionName = cellSpecificCollectionReference.GetCollectionName();
                SubscribeToCollectionUpdates(cellSpecificCollectionReference.Cell, collectionName);
            }
        }

        private void TrackCollectionReference(CellModel cell, CellSpecificCollectionReference cellSpecificCollectionReference)
        {
            cellSpecificCollectionReference.CollectionNameChanged += CollectionNameChangedInCellsCollectionDependencies;
            if (_collectionDependenciesForCellsPopulateFunction.TryGetValue(cell, out var cellSpecificCollectionReferences))
            {
                cellSpecificCollectionReferences.Add(cellSpecificCollectionReference);
            }
            else
            {
                _collectionDependenciesForCellsPopulateFunction.Add(cell, [cellSpecificCollectionReference]);
            }
        }

        private void UntrackCollectionReferences(CellModel cell)
        {
            if (_collectionDependenciesForCellsPopulateFunction.TryGetValue(cell, out var cellSpecificCollectionReferences))
            {
                foreach (var cellSpecificCollectionReference in cellSpecificCollectionReferences)
                {
                    cellSpecificCollectionReference.CollectionNameChanged -= CollectionNameChangedInCellsCollectionDependencies;
                }
                _collectionDependenciesForCellsPopulateFunction.Remove(cell);
            }
        }

        private void CollectionNameChangedInCellsCollectionDependencies(CellSpecificCollectionReference reference)
        {
            if (string.IsNullOrWhiteSpace(reference.Cell.PopulateFunctionName)) return;
            if (!_pluginFunctionLoader.TryGetFunction("object", reference.Cell.PopulateFunctionName, out var function)) return;
            UpdateDependencySubscriptions(reference.Cell, function);
        }

        private void ResolveLocationDependenciesForCell(CellModel cell, PluginFunction function)
        {
            var thisLocation = Utilities.GetUnqiueLocationString(cell.SheetName, cell.Row, cell.Column);
            foreach (var locationDependency in function.LocationDependencies)
            {
                var locations = locationDependency.ResolveLocations(cell);
                foreach (var location in locations)
                {
                    if (thisLocation == location) continue;
                    SubscribeToUpdatesAtLocation(cell, location);
                }
            }

            // Because this cell has a populate function, always listen to changes to itself.
            SubscribeToUpdatesAtLocation(cell, thisLocation);
        }

        private void AddToCellsToUpdateWhenFunctionChangesMap(CellModel cell, PluginFunction function)
        {
            if (_cellsToUpdateWhenFunctionChanges.TryGetValue(function.Model, out var cellList))
            {
                if (!cellList.Contains(cell)) cellList.Add(cell);
            }
            else
            {
                _cellsToUpdateWhenFunctionChanges.Add(function.Model, [cell]);
                function.DependenciesChanged += NotifyCellsAboutFunctionDependencyChanges;
                function.Model.PropertyChanged += RunPopulateWhenCodeChanges;
            }
        }

        private void CellPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var cell = (CellModel)sender!;
            if (e.PropertyName == nameof(CellModel.PopulateFunctionName))
            {
                UnsubscribeCellFromFunctionChanges(cell);
                SubscribeCellToFunctionChanges(cell);
            }
            //if (e.PropertyName == nameof(CellModel.Text))
            //{
            //    _cellTextChangesAtLocationNotifier.
            //}
        }

        private void SubscribeCellToFunctionChanges(CellModel cell)
        {
            if (!_pluginFunctionLoader.TryGetFunction("object", cell.PopulateFunctionName, out var function)) return;
            UpdateDependencySubscriptions(cell, function);
            AddToCellsToUpdateWhenFunctionChangesMap(cell, function);
            _cellToPopulateFunctionNameMap[cell] = cell.PopulateFunctionName;
        }

        private void UnsubscribeCellFromFunctionChanges(CellModel cell)
        {
            if (!_cellToPopulateFunctionNameMap.TryGetValue(cell, out var oldFunctionName)) return;
            _cellToPopulateFunctionNameMap.Remove(cell);
            if (!_pluginFunctionLoader.TryGetFunction("object", oldFunctionName, out var oldFunction)) return;
            RemoveFromCellsToUpdateWhenFunctionChangesMap(cell, oldFunction);
        }

        private CellPopulateSubscriber GetOrCreatePopulateSubscriber(CellModel cell)
        {
            if (_cellToPopulateSubscriberMap.TryGetValue(cell, out var subscriber)) return subscriber;
            subscriber = new CellPopulateSubscriber(cell, _cellTracker, _userCollectionLoader, _pluginFunctionLoader);
            _cellToPopulateSubscriberMap.Add(cell, subscriber);
            return subscriber;
        }

        private void NotifyCellsAboutFunctionDependencyChanges(PluginFunction function)
        {
            _cellsToUpdateWhenFunctionChanges[function.Model].ForEach(cell => UpdateDependencySubscriptions(cell, function));
        }

        private void RunPopulateWhenCodeChanges(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PluginFunctionModel.Code))
            {
                _cellsToUpdateWhenFunctionChanges[(PluginFunctionModel)sender!].ForEach(PopulateCellsText);
            }
        }

        private void RemoveFromCellsToUpdateWhenFunctionChangesMap(CellModel cell, PluginFunction function)
        {
            if (_cellsToUpdateWhenFunctionChanges.TryGetValue(function.Model, out var cellList))
            {
                cellList.Remove(cell);
                if (cellList.Count == 0)
                {
                    _cellsToUpdateWhenFunctionChanges.Remove(function.Model);
                    function.DependenciesChanged -= NotifyCellsAboutFunctionDependencyChanges;
                    function.Model.PropertyChanged -= RunPopulateWhenCodeChanges;
                }
            }
        }
    }
}
