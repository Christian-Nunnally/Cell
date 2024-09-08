using Cell.Model;
using Cell.Persistence;
using Cell.View.ToolWindow;
using Cell.ViewModel.Execution;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace Cell.Data
{
    public class SheetTracker
    {
        private const string TemplatesSaveDirectory = "Templates";
        private readonly UserCollectionLoader _userCollectionLoader;
        private readonly PluginFunctionLoader _pluginFunctionLoader;
        private readonly PersistenceManager _persistenceManager;
        private readonly CellTracker _cellTracker;
        private readonly CellLoader _cellLoader;

        public SheetTracker(PersistenceManager persistenceManager, CellLoader cellLoader, CellTracker cellTracker, PluginFunctionLoader pluginFunctionLoader, UserCollectionLoader userCollectionLoader)
        {
            _userCollectionLoader = userCollectionLoader;
            _pluginFunctionLoader = pluginFunctionLoader;
            _persistenceManager = persistenceManager;
            _cellTracker = cellTracker;
            _cellLoader = cellLoader;
            _cellTracker.CellAdded += CellAddedToTracker;
            _cellTracker.CellRemoved += CellRemovedFromTracker;
            Sheets.CollectionChanged += SheetsCollectionChanged;
        }

        private void CellRemovedFromTracker(CellModel model)
        {
            var sheet = Sheets.First(x => x.Name == model.SheetName);
            sheet.Cells.Remove(model);
            if (sheet.Cells.Count == 0)
            {
                if (sheet != null)
                {
                    Sheets.Remove(sheet);
                    RefreshOrderedSheetsList();
                }
            }
        }

        private void CellAddedToTracker(CellModel model)
        {
            var sheet = Sheets.FirstOrDefault(x => x.Name == model.SheetName);
            if (sheet == null)
            {
                sheet = new SheetModel(model.SheetName);
                Sheets.Add(sheet);
            }

            sheet.Cells.Add(model);

            if (model.CellType == CellType.Corner)
            {
                sheet.CornerCell = model;
            }
        }

        public ObservableCollection<SheetModel> OrderedSheets { get; } = [];

        public ObservableCollection<SheetModel> Sheets { get; } = [];

        public void RenameSheet(string oldSheetName, string newSheetName)
        {
            _cellTracker.RenameSheet(oldSheetName, newSheetName);
            _cellTracker.GetCellModelsForSheet(oldSheetName).ForEach(x => x.SheetName = newSheetName);
        }

        private void SheetPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is not SheetModel sheetModel) return;
            if (e.PropertyName == nameof(SheetModel.Order))
            {
                RefreshOrderedSheetsList();
            }
            if (e.PropertyName == nameof(SheetModel.Name))
            {
                RenameSheet(sheetModel.OldName, sheetModel.Name);
                sheetModel.OldName = sheetModel.Name;
            }
        }

        private void RefreshOrderedSheetsList()
        {
            OrderedSheets.Clear();
            foreach (var sheet in Sheets.OrderBy(x => x.Order))
            {
                OrderedSheets.Add(sheet);
            }
        }

        private void SheetsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var sheet in e.NewItems.OfType<SheetModel>())
                {
                    sheet.PropertyChanged += SheetPropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (var sheet in e.OldItems.OfType<SheetModel>())
                {
                    sheet.PropertyChanged -= SheetPropertyChanged;
                }
            }
        }

        public void ExportSheetTemplate(string sheetName)
        {
            var copiedCells = CreateUntrackedCopiesOfCellsInSheet(sheetName);

            var templateDirectory = Path.Combine(TemplatesSaveDirectory, sheetName);
            var cellDirectory = Path.Combine(templateDirectory, "Cells");

            foreach (var copiedCell in copiedCells)
            {
                _cellLoader.SaveCell(cellDirectory, copiedCell);
            }

            var populateFunctions = copiedCells.Select(c => c.PopulateFunctionName).Where(x => !string.IsNullOrEmpty(x)).Distinct().Select(x => _pluginFunctionLoader.GetOrCreateFunction("object", x));
            var triggerFunctions = copiedCells.Select(c => c.TriggerFunctionName).Where(x => !string.IsNullOrEmpty(x)).Distinct().Select(x => _pluginFunctionLoader.GetOrCreateFunction("void", x));
            var populateAndTriggerFunctions = populateFunctions.Concat(triggerFunctions).ToList();

            var usedCollections = populateAndTriggerFunctions.SelectMany(f => f.CollectionDependencies).Distinct().ToList();
            AddBaseCollectionsToUsedCollectionList(usedCollections);

            var collectionSortFunctions = usedCollections.Select(x => _pluginFunctionLoader.GetOrCreateFunction("object", x));
            var allFunctions = populateAndTriggerFunctions.Concat(collectionSortFunctions);
            foreach (var function in allFunctions)
            {
                _pluginFunctionLoader.SavePluginFunction(templateDirectory, function.Model.ReturnType, function.Model);
            }

            foreach (var collection in usedCollections)
            {
                var collectionDirectory = Path.Combine(templateDirectory, "Collections", collection);
                _userCollectionLoader.ExportCollection(collection, collectionDirectory);
            }
        }

        public void ImportSheetTemplate(string templateName, string sheetName, bool skipExistingCollectionsDuringImport)
        {
            var templatesDirectory = Path.Combine(TemplatesSaveDirectory);
            if (!_persistenceManager.DirectoryExists(templatesDirectory)) return;
            var templatePath = Path.Combine(templatesDirectory, templateName);
            var cellsPath = Path.Combine(templatePath, "Cells");
            if (!_persistenceManager.DirectoryExists(cellsPath)) return;

            var cellsToAdd = _cellLoader.LoadSheet(cellsPath);
            UpdateIdentitiesOfCellsForNewSheet(sheetName, cellsToAdd);

            var functionsBeingImported = GetFunctionsFromTemplate(templatePath);
            if (!CanFunctionsBeMerged(functionsBeingImported, out var reason))
            {
                DialogWindow.ShowDialog("Import canceled", reason);
                return;
            }

            var collectionsBeingImported = new List<string>();
            var collectionsDirectory = Path.Combine(templatePath, "Collections");
            foreach (var collectionDirectory in _persistenceManager.GetDirectories(collectionsDirectory))
            {
                var collectionName = Path.GetFileName(collectionDirectory);
                collectionsBeingImported.Add(collectionName);
            }

            var collectionsBeingImportedWithoutConflicts = new List<string>();
            foreach (var collectionBeingImported in collectionsBeingImported)
            {
                if (!_userCollectionLoader.CollectionNames.Contains(collectionBeingImported))
                {
                    collectionsBeingImportedWithoutConflicts.Add(collectionBeingImported);
                }
                else if (!skipExistingCollectionsDuringImport)
                {
                    DialogWindow.ShowDialog("Import canceled", $"A collection with the name '{collectionBeingImported}' already exists. Please rename that collection before importing or enable 'Skip Existing Collections'.");
                    return;
                }
            }

            AddAndSaveCells(cellsToAdd);

            foreach (var functionModel in functionsBeingImported)
            {
                var function = new FunctionViewModel(functionModel);
                _pluginFunctionLoader.AddPluginFunctionToNamespace(functionModel.ReturnType, function);
                _pluginFunctionLoader.SavePluginFunction("", functionModel.ReturnType, functionModel);
            }

            foreach (var collectionName in collectionsBeingImportedWithoutConflicts)
            {
                var collectionDirectory = Path.Combine(templatePath, "Collections", collectionName);
                _userCollectionLoader.ImportCollection(collectionDirectory, collectionName);
            }
        }

        public List<CellModel> CreateUntrackedCopiesOfCellsInSheet(string sheetName)
        {
            var copiedCells = _cellTracker.GetCellModelsForSheet(sheetName).Select(c => c.Copy()).ToList();
            foreach (var copiedCell in copiedCells)
            {
                copiedCell.SheetName = "";
            }

            return copiedCells;
        }

        private void AddBaseCollectionsToUsedCollectionList(List<string> usedCollections)
        {
            for (int i = 0; i < usedCollections.Count; i++)
            {
                var collection = usedCollections[i];
                var baseCollectionName = _userCollectionLoader.GetCollection(collection)?.Model.BasedOnCollectionName;
                if (!string.IsNullOrWhiteSpace(baseCollectionName) && !usedCollections.Contains(baseCollectionName))
                {
                    usedCollections.Add(baseCollectionName);
                }
            }
        }

        private bool CanFunctionsBeMerged(List<PluginFunctionModel> functionsBeingImported, out string reason)
        {
            reason = string.Empty;
            foreach (var function in functionsBeingImported)
            {
                var existingFunction = GetExistingFunction(function);
                if (existingFunction == null) continue;
                if (existingFunction.Model.Code == function.Code) continue;
                reason = $"Unable to import because function '{function.Name}' already exists and has a different implementation.";
                return false;
            }
            return true;

            FunctionViewModel? GetExistingFunction(PluginFunctionModel function)
            {
                return _pluginFunctionLoader.ObservableFunctions.FirstOrDefault(x => x.Model.Name == function.Name);
            }
        }

        private void FixMergedCellsWithNewIdentities(IEnumerable<CellModel> cells, Dictionary<string, string> oldIdToNewIdMap)
        {
            foreach (var cell in cells.Where(cell => cell.MergedWith != string.Empty))
            {
                cell.MergedWith = oldIdToNewIdMap[cell.MergedWith];
            }
        }

        private List<PluginFunctionModel> GetFunctionsFromTemplate(string templatesDirectory)
        {
            var functionsBeingImported = new List<PluginFunctionModel>();
            var functionsTemplatePath = Path.Combine(templatesDirectory, "Functions");
            foreach (var spaceDirectory in _persistenceManager.GetDirectories(functionsTemplatePath))
            {
                var paths = _persistenceManager.GetFiles(spaceDirectory);
                foreach (var path in paths)
                {
                    functionsBeingImported.Add(_pluginFunctionLoader.LoadFunction(path));
                }
            }
            return functionsBeingImported;
        }

        private Dictionary<string, string> GiveCellsNewUniqueIndentities(string sheetName, IEnumerable<CellModel> cells)
        {
            var oldIdToNewIdMap = new Dictionary<string, string>();
            foreach (var cell in cells)
            {
                var newId = Guid.NewGuid().ToString();
                oldIdToNewIdMap[cell.ID] = newId;
                cell.ID = newId;
                cell.SheetName = sheetName;
            }
            return oldIdToNewIdMap;
        }

        public void UpdateIdentitiesOfCellsForNewSheet(string sheetName, IEnumerable<CellModel> cellsToAdd)
        {
            var oldIdToNewIdMap = GiveCellsNewUniqueIndentities(sheetName, cellsToAdd);
            FixMergedCellsWithNewIdentities(cellsToAdd, oldIdToNewIdMap);
        }

        public void AddAndSaveCells(IEnumerable<CellModel> cellsToAdd)
        {
            foreach (var cell in cellsToAdd)
            {
                _cellTracker.AddCell(cell, saveAfterAdding: true);
            }
        }
    }
}
