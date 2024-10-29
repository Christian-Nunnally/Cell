using Cell.Core.Data;
using Cell.Model;
using Cell.Core.Persistence;
using System.ComponentModel;
using Cell.Core.Execution.Functions;
using Cell.ViewModel.Application;
using Cell.Core.Execution.References;

namespace Cell.Core.Execution
{
    /// <summary>
    /// Responsible for running other cells GetText functions when cells or collections they reference are updated.
    /// </summary>
    public class CellPopulateManager
    {
        private readonly Dictionary<CellFunctionModel, List<CellModel>> _cellsToUpdateWhenFunctionChanges = [];
        private readonly CellTextChangesAtLocationNotifier _cellTextChangesAtLocationNotifier;
        private readonly Dictionary<CellModel, string> _cellToPopulateFunctionNameMap = [];
        private readonly Dictionary<CellModel, CellPopulateSubscriber> _cellToPopulateSubscriberMap = [];
        private readonly CellTracker _cellTracker;
        private readonly CollectionChangeNotifier _collectionChangeNotifier;
        private readonly Dictionary<CellModel, List<CellSpecificCollectionReference>> _collectionDependenciesForCellsPopulateFunction = [];
        private readonly PluginFunctionLoader _pluginFunctionLoader;
        private readonly Context _pluginFunctionRunContext;
        private readonly UserCollectionLoader _userCollectionLoader;
        /// <summary>
        /// Creates a new instance of <see cref="CellPopulateManager"/>.
        /// </summary>
        /// <param name="cellTracker">The cell tracker to determine cells to manage.</param>
        /// <param name="pluginFunctionLoader">The function loader used to load the populate function.</param>
        /// <param name="userCollectionLoader">The collection loader used in the context when running populate.</param>
        public CellPopulateManager(CellTracker cellTracker, PluginFunctionLoader pluginFunctionLoader, UserCollectionLoader userCollectionLoader)
        {
            _pluginFunctionRunContext = new Context(cellTracker, userCollectionLoader, new DialogFactory(), CellModel.Null);
            _cellTextChangesAtLocationNotifier = new CellTextChangesAtLocationNotifier(cellTracker);
            _collectionChangeNotifier = new CollectionChangeNotifier(userCollectionLoader);
            _userCollectionLoader = userCollectionLoader;
            _cellTracker = cellTracker;
            _cellTracker.CellAdded += StartMonitoringCellForChanges;
            _cellTracker.CellRemoved += StopMonitoringCellForChanges;
            _pluginFunctionLoader = pluginFunctionLoader;
            foreach (var cell in _cellTracker.AllCells)
            {
                StartMonitoringCellForChanges(cell);
            }
        }

        /// <summary>
        /// Gets all collections the given cell is subscribed to.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <returns>A list of collection names that the cell cares about.</returns>
        public IEnumerable<string> GetAllCollectionSubscriptions(CellModel cell)
        {
            var subscriber = GetOrCreatePopulateSubscriber(cell);
            return _collectionChangeNotifier.GetCollectionsSubscriberIsSubscribedTo(subscriber);
        }

        /// <summary>
        /// Gets all locations the given cell is subscribed to.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <returns>A list of location strings the the cell cares about.</returns>
        public IEnumerable<string> GetAllLocationSubscriptions(CellModel cell)
        {
            var subscriber = GetOrCreatePopulateSubscriber(cell);
            return _cellTextChangesAtLocationNotifier.GetLocationsSubscriberIsSubscribedTo(subscriber);
        }

        /// <summary>
        /// Efficiently gets all cells that use the given function.
        /// </summary>
        /// <param name="function">The function to get the users of.</param>
        /// <returns>The cells that use the given populate function.</returns>
        public List<CellModel> GetCellsThatUsePopulateFunction(CellFunctionModel function)
        {
            return _cellsToUpdateWhenFunctionChanges.TryGetValue(function, out var cells) ? cells : [];
        }

        internal void RunPopulateForCell(CellModel model)
        {
            var subscriber = GetOrCreatePopulateSubscriber(model);
            subscriber.Action();
        }

        private void AddToCellsToUpdateWhenFunctionChangesMap(CellModel cell, CellFunction function)
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

                var subscriber = GetOrCreatePopulateSubscriber(cell);
                subscriber.Action();
            }
        }

        private void CollectionNameChangedInCellsCollectionDependencies(CellSpecificCollectionReference reference)
        {
            if (string.IsNullOrWhiteSpace(reference.Cell.PopulateFunctionName)) return;
            if (!_pluginFunctionLoader.TryGetCellFunction("object", reference.Cell.PopulateFunctionName, out var function)) return;
            UpdateDependencySubscriptions(reference.Cell, function);
        }

        private CellPopulateSubscriber GetOrCreatePopulateSubscriber(CellModel cell)
        {
            if (_cellToPopulateSubscriberMap.TryGetValue(cell, out var subscriber)) return subscriber;
            subscriber = new CellPopulateSubscriber(cell, _cellTracker, _userCollectionLoader, _pluginFunctionLoader);
            _cellToPopulateSubscriberMap.Add(cell, subscriber);
            return subscriber;
        }

        private void NotifyCellsAboutFunctionDependencyChanges(CellFunction function)
        {
            _cellsToUpdateWhenFunctionChanges[function.Model].ForEach(cell => UpdateDependencySubscriptions(cell, function));
        }

        private void RemoveFromCellsToUpdateWhenFunctionChangesMap(CellModel cell, CellFunction function)
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

        private void ResolveCollectionDependenciesForCell(CellModel cell, CellFunction function)
        {
            UnsubscribeFromAllCollectionUpdates(cell);
            function.Compile();
            foreach (var collectionReference in function.CollectionDependencies)
            {
                var cellSpecificCollectionReference = new CellSpecificCollectionReference(cell, collectionReference, _cellTextChangesAtLocationNotifier, _pluginFunctionRunContext);
                TrackCollectionReference(cell, cellSpecificCollectionReference);

                var collectionName = cellSpecificCollectionReference.GetCollectionName();
                if (string.IsNullOrWhiteSpace(collectionName))
                {
                    throw new InvalidOperationException(collectionName + " is not a valid collection name.");
                }
                SubscribeToCollectionUpdates(cellSpecificCollectionReference.Cell, collectionName);
            }
        }

        private void ResolveLocationDependenciesForCell(CellModel cell, CellFunction function)
        {
            function.Compile();
            UnsubscribeFromAllLocationUpdates(cell);
            var thisLocation = cell.Location.LocationString;
            foreach (var locationDependency in function.LocationDependencies)
            {
                var locations = locationDependency.ResolveLocations(cell.Location);
                foreach (var location in locations)
                {
                    if (thisLocation == location) continue;
                    SubscribeToUpdatesAtLocation(cell, location);
                }
            }

            SubscribeToUpdatesAtLocation(cell, thisLocation);
        }

        private void RunPopulateWhenCodeChanges(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellFunctionModel.Code))
            {
                foreach (var cellToUpdate in _cellsToUpdateWhenFunctionChanges[(CellFunctionModel)sender!])
                {
                    _cellToPopulateSubscriberMap[cellToUpdate].Action();
                }
            }
        }

        private void StartMonitoringCellForChanges(CellModel cell)
        {
            cell.PropertyChanged += CellPropertyChanged;
            if (string.IsNullOrWhiteSpace(cell.PopulateFunctionName)) return;
            if (!_pluginFunctionLoader.TryGetCellFunction("object", cell.PopulateFunctionName, out var function)) return;
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
            if (!_pluginFunctionLoader.TryGetCellFunction("object", cell.PopulateFunctionName, out var function)) return;
            UpdateDependencySubscriptions(cell, function);
            _cellToPopulateFunctionNameMap.Remove(cell);
            RemoveFromCellsToUpdateWhenFunctionChangesMap(cell, function);
        }

        private void SubscribeCellToFunctionChanges(CellModel cell)
        {
            if (!_pluginFunctionLoader.TryGetCellFunction("object", cell.PopulateFunctionName, out var function)) return;
            UpdateDependencySubscriptions(cell, function);
            AddToCellsToUpdateWhenFunctionChangesMap(cell, function);
            _cellToPopulateFunctionNameMap[cell] = cell.PopulateFunctionName;
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

        private void UnsubscribeCellFromFunctionChanges(CellModel cell)
        {
            if (!_cellToPopulateFunctionNameMap.TryGetValue(cell, out var oldFunctionName)) return;
            _cellToPopulateFunctionNameMap.Remove(cell);
            if (!_pluginFunctionLoader.TryGetCellFunction("object", oldFunctionName, out var oldFunction)) return;
            RemoveFromCellsToUpdateWhenFunctionChangesMap(cell, oldFunction);
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

        private void UpdateDependencySubscriptions(CellModel cell, CellFunction function)
        {
            if (string.IsNullOrWhiteSpace(cell.Location.SheetName)) return;
            function.Compile();
            UntrackCollectionReferences(cell);
            ResolveLocationDependenciesForCell(cell, function);
            ResolveCollectionDependenciesForCell(cell, function);
        }
    }
}
