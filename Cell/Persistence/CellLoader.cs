using Cell.Common;
using Cell.Data;
using Cell.Model;
using Cell.View.ToolWindow;
using Cell.ViewModel.Execution;
using System.IO;
using System.Text.Json;

namespace Cell.Persistence
{
    public class CellLoader
    {
        private const string SheetsSaveDirectory = "Sheets";
        private const string TemplatesSaveDirectory = "Templates";
        private PersistenceManager _persistenceManager;

        public CellLoader(PersistenceManager persistenceManager)
        {
            _persistenceManager = persistenceManager;
        }

        public static CellModel LoadCell(string file)
        {
            var text = File.ReadAllText(file) ?? throw new CellError($"Loading file failed at {file}"); ;
            var cell = JsonSerializer.Deserialize<CellModel>(text) ?? throw new CellError($"Deserialization failed for {File.ReadAllText(file)} at {file}");
            return cell;
        }

        public static IEnumerable<CellModel> LoadSheet(string directory)
        {
            var result = new List<CellModel>();
            foreach (var file in Directory.GetFiles(directory)) result.Add(LoadCell(file));
            return result;
        }

        public void DeleteCell(CellModel cellModel)
        {
            var cellPath = Path.Combine(SheetsSaveDirectory, cellModel.SheetName, cellModel.ID);
            _persistenceManager.DeleteFile(cellPath);
        }

        // TODO: Clean
        public void ExportSheetTemplate(string sheetName)
        {
            var copiedCells = CreateUntrackedCopiesOfCellsInSheet(sheetName);

            var templateDirectory = Path.Combine(TemplatesSaveDirectory, sheetName);
            var cellDirectory = Path.Combine(templateDirectory, "Cells");

            foreach (var copiedCell in copiedCells)
            {
                SaveCell(cellDirectory, copiedCell);
            }

            var populateFunctions = copiedCells.Select(c => c.PopulateFunctionName).Where(x => !string.IsNullOrEmpty(x)).Distinct().Select(x => PluginFunctionLoader.GetOrCreateFunction("object", x));
            var triggerFunctions = copiedCells.Select(c => c.TriggerFunctionName).Where(x => !string.IsNullOrEmpty(x)).Distinct().Select(x => PluginFunctionLoader.GetOrCreateFunction("void", x));
            var populateAndTriggerFunctions = populateFunctions.Concat(triggerFunctions).ToList();

            var usedCollections = populateAndTriggerFunctions.SelectMany(f => f.CollectionDependencies).Distinct().ToList();
            AddBaseCollectionsToUsedCollectionList(usedCollections);

            var collectionSortFunctions = usedCollections.Select(x => PluginFunctionLoader.GetOrCreateFunction("object", x));
            var allFunctions = populateAndTriggerFunctions.Concat(collectionSortFunctions);
            foreach (var function in allFunctions)
            {
                PluginFunctionLoader.SavePluginFunction(templateDirectory, function.Model.ReturnType, function.Model);
            }

            foreach (var collection in usedCollections)
            {
                var collectionDirectory = Path.Combine(templateDirectory, "Collections", collection);
                UserCollectionLoader.ExportCollection(collection, collectionDirectory);
            }
        }

        private static List<CellModel> CreateUntrackedCopiesOfCellsInSheet(string sheetName)
        {
            var copiedCells = CellTracker.Instance.GetCellModelsForSheet(sheetName).Select(c => c.Copy()).ToList();
            foreach (var copiedCell in copiedCells)
            {
                copiedCell.SheetName = "";
            }

            return copiedCells;
        }

        private static void AddBaseCollectionsToUsedCollectionList(List<string> usedCollections)
        {
            for (int i = 0; i < usedCollections.Count; i++)
            {
                var collection = usedCollections[i];
                var baseCollectionName = UserCollectionLoader.GetCollection(collection)?.Model.BasedOnCollectionName;
                if (!string.IsNullOrWhiteSpace(baseCollectionName) && !usedCollections.Contains(baseCollectionName))
                {
                    usedCollections.Add(baseCollectionName);
                }
            }
        }

        public void ImportSheetTemplate(string templateName, string sheetName)
        {
            var templatesDirectory = Path.Combine(TemplatesSaveDirectory);
            if (!_persistenceManager.DirectoryExists(templatesDirectory)) return;
            var templatePath = Path.Combine(templatesDirectory, templateName);
            var cellsPath = Path.Combine(templatePath, "Cells");
            if (!_persistenceManager.DirectoryExists(cellsPath)) return;

            var cellsToAdd = LoadSheet(cellsPath);
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
            var conflictingCollection = collectionsBeingImported.FirstOrDefault(UserCollectionLoader.CollectionNames.Contains);
            if (conflictingCollection is not null)
            {
                DialogWindow.ShowDialog("Import canceled", $"A collection with the name '{conflictingCollection}' already exists. Please rename that collection before importing.");
                return;
            }

            AddAndTrackCells(cellsToAdd);

            foreach (var functionModel in functionsBeingImported)
            {
                var function = new FunctionViewModel(functionModel);
                PluginFunctionLoader.AddPluginFunctionToNamespace(functionModel.ReturnType, function);
                PluginFunctionLoader.SavePluginFunction("", functionModel.ReturnType, functionModel);
            }

            foreach (var collectionName in collectionsBeingImported)
            {
                var collectionDirectory = Path.Combine(templatePath, "Collections", collectionName);
                UserCollectionLoader.ImportCollection(collectionDirectory, collectionName);
            }
        }

        private static void AddAndTrackCells(IEnumerable<CellModel> cellsToAdd)
        {
            foreach (var cell in cellsToAdd)
            {
                CellTracker.Instance.AddCell(cell, saveAfterAdding: true);
            }
        }

        private static void UpdateIdentitiesOfCellsForNewSheet(string sheetName, IEnumerable<CellModel> cellsToAdd)
        {
            var oldIdToNewIdMap = GiveCellsNewUniqueIndentities(sheetName, cellsToAdd);
            FixMergedCellsWithNewIdentities(cellsToAdd, oldIdToNewIdMap);
        }

        public void CopySheet(string sheetName)
        {
            var copiedSheetName = sheetName + "Copy";
            while (SheetTracker.Instance.Sheets.Any(x => x.Name == copiedSheetName)) copiedSheetName += "Copy";

            var copiedCells = CreateUntrackedCopiesOfCellsInSheet(sheetName);
            UpdateIdentitiesOfCellsForNewSheet(copiedSheetName, copiedCells);
            AddAndTrackCells(copiedCells);
        }

        public void LoadAndAddCells()
        {
            if (!_persistenceManager.DirectoryExists(SheetsSaveDirectory)) return;
            foreach (var directory in _persistenceManager.GetDirectories(SheetsSaveDirectory)) LoadAndAddSheet(directory);
        }

        public void RenameSheet(string oldName, string newName)
        {
            var oldDirectory = Path.Combine(SheetsSaveDirectory, oldName);
            var newDirectory = Path.Combine(SheetsSaveDirectory, newName);
            _persistenceManager.MoveDirectory(oldDirectory, newDirectory);
        }

        public void SaveCell(CellModel cell)
        {
            var directory = Path.Combine(SheetsSaveDirectory, cell.SheetName);
            SaveCell(directory, cell);
        }

        public void SaveCell(string directory, CellModel cell)
        {
            var serialized = JsonSerializer.Serialize(cell);
            var path = Path.Combine(directory, cell.ID);
            _persistenceManager.SaveFile(path, serialized);
        }

        public void SaveCells()
        {
            foreach (var sheet in SheetTracker.Instance.Sheets) SaveSheet(sheet);
        }

        private static bool CanFunctionsBeMerged(List<PluginFunctionModel> functionsBeingImported, out string reason)
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

            static FunctionViewModel? GetExistingFunction(PluginFunctionModel function)
            {
                return PluginFunctionLoader.ObservableFunctions.FirstOrDefault(x => x.Model.Name == function.Name);
            }
        }

        private static void FixMergedCellsWithNewIdentities(IEnumerable<CellModel> cells, Dictionary<string, string> oldIdToNewIdMap)
        {
            foreach (var cell in cells.Where(cell => cell.MergedWith != string.Empty))
            {
                cell.MergedWith = oldIdToNewIdMap[cell.MergedWith];
            }
        }

        private static List<PluginFunctionModel> GetFunctionsFromTemplate(string templatesDirectory)
        {
            var functionsBeingImported = new List<PluginFunctionModel>();
            var functionsTemplatePath = Path.Combine(templatesDirectory, "Functions");
            foreach (var spaceDirectory in Directory.GetDirectories(functionsTemplatePath))
            {
                foreach (var file in Directory.GetFiles(spaceDirectory))
                {
                    functionsBeingImported.Add(PluginFunctionLoader.LoadFunction(file));
                }
            }
            return functionsBeingImported;
        }

        private static Dictionary<string, string> GiveCellsNewUniqueIndentities(string sheetName, IEnumerable<CellModel> cells)
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

        private static CellModel LoadAndAddCell(string file)
        {
            var cell = LoadCell(file);
            CellTracker.Instance.AddCell(cell, saveAfterAdding: false);
            return cell;
        }

        private static void LoadAndAddSheet(string directory)
        {
            foreach (var file in Directory.GetFiles(directory)) LoadAndAddCell(file);
        }

        private void SaveSheet(SheetModel sheet)
        {
            var directory = Path.Combine(SheetsSaveDirectory, sheet.Name);
            foreach (var cell in CellTracker.Instance.GetCellModelsForSheet(sheet.Name)) SaveCell(directory, cell);
        }
    }
}
