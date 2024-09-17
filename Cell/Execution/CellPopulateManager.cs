using Cell.Data;
using Cell.Model;
using Cell.Persistence;
using Cell.ViewModel.Cells.Types;
using Cell.ViewModel.Execution;
using System.ComponentModel;

namespace Cell.Execution
{
    /// <summary>
    /// Responsible for running other cells GetText functions when cells or collections they reference are updated.
    /// </summary>
    public class CellPopulateManager
    {
        private readonly Dictionary<PluginFunctionModel, List<CellModel>> _cellsToUpdateWhenFunctionChanges = [];
        private readonly CellTextChangesAtLocationNotifier _cellTextChangesAtLocationNotifier;
        private readonly Dictionary<CellModel, string> _cellToPopulateFunctionNameMap = [];
        private readonly Dictionary<CellModel, CellPopulateSubscriber> _cellToPopulateSubscriberMap = [];
        private readonly CellTracker _cellTracker;
        private readonly CollectionChangeNotifier _collectionChangeNotifier;
        private readonly PluginFunctionLoader _pluginFunctionLoader;
        private readonly UserCollectionLoader _userCollectionLoader;
        public CellPopulateManager(CellTracker cellTracker, PluginFunctionLoader pluginFunctionLoader, UserCollectionLoader userCollectionLoader)
        {
            _cellTextChangesAtLocationNotifier = new CellTextChangesAtLocationNotifier(cellTracker);
            _collectionChangeNotifier = new CollectionChangeNotifier(userCollectionLoader);
            _userCollectionLoader = userCollectionLoader;
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

        public void PopulateCellsText(CellModel cell)
        {
            if (string.IsNullOrEmpty(cell.PopulateFunctionName)) return;
            var result = DynamicCellPluginExecutor.RunPopulate(_pluginFunctionLoader, new PluginContext(_cellTracker, _userCollectionLoader, cell.Index), cell);
            if (result.ExecutionResult == null) return;
            if (result.WasSuccess) cell.Text = result.ExecutionResult;
            else cell.ErrorText = result.ExecutionResult;
        }

        public void StartMonitoringCellForUpdates(CellModel cell)
        {
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

        public void StopMonitoringCellForUpdates(CellModel cell)
        {
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

        public void SubscribeToCollectionUpdates(CellModel cell, string collectionName)
        {
            var subscriber = GetOrCreatePopulateSubscriber(cell);
            _collectionChangeNotifier.SubscribeToCollectionUpdates(subscriber, collectionName);
        }

        public void SubscribeToUpdatesAtLocation(CellModel cell, string sheet, int row, int column)
        {
            var subscriber = GetOrCreatePopulateSubscriber(cell);
            _cellTextChangesAtLocationNotifier.SubscribeToUpdatesAtLocation(subscriber, sheet, row, column);
        }

        public void UnsubscribeFromAllCollectionUpdates(CellModel cell)
        {
            var subscriber = GetOrCreatePopulateSubscriber(cell);
            _collectionChangeNotifier.UnsubscribeFromAllCollections(subscriber);
        }

        public void UnsubscribeFromAllLocationUpdates(CellModel cell)
        {
            var subscriber = GetOrCreatePopulateSubscriber(cell);
            _cellTextChangesAtLocationNotifier.UnsubscribeFromAllLocations(subscriber);
        }

        public void UpdateDependencySubscriptions(CellModel cell, PluginFunction function)
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
