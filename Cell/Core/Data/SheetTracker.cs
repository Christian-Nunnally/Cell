using Cell.Model;
using Cell.Core.Persistence;
using Cell.ViewModel.Application;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using Cell.Core.Execution.Functions;

namespace Cell.Core.Data
{
    /// <summary>
    /// The tracker for all the sheets in a project.
    /// </summary>
    public class SheetTracker
    {
        private const string TemplatesSaveDirectory = "Templates";
        private readonly CellLoader _cellLoader;
        private readonly PersistedDirectory _persistedDirectory;
        private readonly PluginFunctionLoader _pluginFunctionLoader;
        private readonly UserCollectionLoader _userCollectionLoader;
        /// <summary>
        /// Creates a new instance of <see cref="SheetTracker"/>.
        /// </summary>
        /// <param name="persistedDirectory">The project directory.</param>
        /// <param name="cellLoader">The cell loader used during import/export.</param>
        /// <param name="cellTracker">The cell tracker used to get the cells in the sheet.</param>
        /// <param name="pluginFunctionLoader">The function loader used for importing/exporting functions.</param>
        /// <param name="userCollectionLoader">The collection loader used for importing/exporting collections.</param>
        public SheetTracker(PersistedDirectory persistedDirectory, CellLoader cellLoader, CellTracker cellTracker, PluginFunctionLoader pluginFunctionLoader, UserCollectionLoader userCollectionLoader)
        {
            _userCollectionLoader = userCollectionLoader;
            _pluginFunctionLoader = pluginFunctionLoader;
            _persistedDirectory = persistedDirectory;
            CellTracker = cellTracker;
            _cellLoader = cellLoader;
            cellTracker.CellAdded += CellAddedToTracker;
            cellTracker.CellRemoved += CellRemovedFromTracker;
            Sheets.CollectionChanged += SheetsCollectionChanged;
        }

        /// <summary>
        /// The cell tracker used to get and create cells in the sheet.
        /// </summary>
        public CellTracker CellTracker { get; }

        /// <summary>
        /// The sheets in the project, ordered by their order.
        /// </summary>
        public ObservableCollection<SheetModel> OrderedSheets { get; } = [];

        /// <summary>
        /// The sheets in the project.
        /// </summary>
        public ObservableCollection<SheetModel> Sheets { get; } = [];

        /// <summary>
        /// Adds the list of cells to the project.
        /// </summary>
        /// <param name="cellsToAdd">The cells to add.</param>
        public void AddAndSaveCells(IEnumerable<CellModel> cellsToAdd)
        {
            foreach (var cell in cellsToAdd)
            {
                CellTracker.AddCell(cell, saveAfterAdding: true);
            }
        }

        /// <summary>
        /// Copies all cells from one sheet and nullifies thier names to facilitate copying them to a new sheet.
        /// </summary>
        /// <param name="sheetName">The name of the sheet to copy the cells of.</param>
        /// <returns>A List of copied cells.</returns>
        public List<CellModel> CreateUntrackedCopiesOfCellsInSheet(string sheetName)
        {
            var copiedCells = CellTracker.GetCellModelsForSheet(sheetName).Select(c => c.Copy()).ToList();
            foreach (var copiedCell in copiedCells)
            {
                copiedCell.Location.SheetName = "";
            }

            return copiedCells;
        }

        /// <summary>
        /// Exports a sheet to the projects template directory.
        /// </summary>
        /// <param name="sheetName">The name of the sheet to export.</param>
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
        }

        /// <summary>
        /// Imports a sheet from the projects template directory into the live project.
        /// </summary>
        /// <param name="templateName">The template name to import.</param>
        /// <param name="sheetName">The name of the sheet to import.</param>
        /// <param name="skipExistingCollectionsDuringImport">Whether to skip importing collections whos name already exists in the project.</param>
        public void ImportSheetTemplate(string templateName, string sheetName, bool skipExistingCollectionsDuringImport)
        {
            var templatesDirectory = Path.Combine(TemplatesSaveDirectory);
            if (!_persistedDirectory.DirectoryExists(templatesDirectory)) return;
            var templatePath = Path.Combine(templatesDirectory, templateName);
            var cellsPath = Path.Combine(templatePath, "Cells");
            if (!_persistedDirectory.DirectoryExists(cellsPath)) return;

            var cellsToAdd = _cellLoader.LoadSheet(cellsPath);
            UpdateIdentitiesOfCellsForNewSheet(sheetName, cellsToAdd);

            var functionsBeingImported = GetFunctionsFromTemplate(templatePath);
            if (!CanFunctionsBeMerged(functionsBeingImported, out var reason))
            {
                DialogFactory.ShowDialog("Import canceled", reason);
                return;
            }

            var collectionsBeingImported = new List<string>();
            var collectionsDirectory = Path.Combine(templatePath, "Collections");
            foreach (var collectionDirectory in _persistedDirectory.GetDirectories(collectionsDirectory))
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
                    DialogFactory.ShowDialog("Import canceled", $"A collection with the name '{collectionBeingImported}' already exists. Please rename that collection before importing or enable 'Skip Existing Collections'.");
                    return;
                }
            }

            AddAndSaveCells(cellsToAdd);

            foreach (var functionModel in functionsBeingImported)
            {
                var function = new CellFunction(functionModel);
                _pluginFunctionLoader.AddCellFunctionToNamespace(functionModel.ReturnType, function);
                _pluginFunctionLoader.SaveCellFunction("", functionModel.ReturnType, functionModel);
            }
        }

        /// <summary>
        /// Renames a sheet.
        /// </summary>
        /// <param name="oldSheetName">The old sheet name.</param>
        /// <param name="newSheetName">The new sheet name.</param>
        public void RenameSheet(string oldSheetName, string newSheetName)
        {
            CellTracker.RenameSheet(oldSheetName, newSheetName);
            CellTracker.GetCellModelsForSheet(oldSheetName).ForEach(x => x.Location.SheetName = newSheetName);
        }

        /// <summary>
        /// Gives cells new IDs and updates merged cells with the new IDs to facilitate copying existing cells from a sheet.
        /// </summary>
        /// <param name="sheetName">The name of the sheet to give all the cell.</param>
        /// <param name="cellsToAdd">The cells to give now IDs.</param>
        public void UpdateIdentitiesOfCellsForNewSheet(string sheetName, IEnumerable<CellModel> cellsToAdd)
        {
            var oldIdToNewIdMap = GiveCellsNewUniqueIndentities(sheetName, cellsToAdd);
            FixMergedCellsWithNewIdentities(cellsToAdd, oldIdToNewIdMap);
        }

        private bool CanFunctionsBeMerged(List<CellFunctionModel> functionsBeingImported, out string reason)
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

            CellFunction? GetExistingFunction(CellFunctionModel function)
            {
                return _pluginFunctionLoader.CellFunctions.FirstOrDefault(x => x.Model.Name == function.Name);
            }
        }

        private void CellAddedToTracker(CellModel model)
        {
            var sheet = Sheets.FirstOrDefault(x => x.Name == model.Location.SheetName);
            if (sheet == null)
            {
                sheet = new SheetModel(model.Location.SheetName);
                Sheets.Add(sheet);
            }

            sheet.Cells.Add(model);

            if (model.CellType == CellType.Corner)
            {
                sheet.CornerCell = model;
            }
        }

        private void CellRemovedFromTracker(CellModel model)
        {
            var sheet = Sheets.First(x => x.Name == model.Location.SheetName);
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

        private void FixMergedCellsWithNewIdentities(IEnumerable<CellModel> cells, Dictionary<string, string> oldIdToNewIdMap)
        {
            foreach (var cell in cells.Where(cell => cell.MergedWith != string.Empty))
            {
                cell.MergedWith = oldIdToNewIdMap[cell.MergedWith];
            }
        }

        private List<CellFunctionModel> GetFunctionsFromTemplate(string templatesDirectory)
        {
            var functionsBeingImported = new List<CellFunctionModel>();
            var functionsTemplatePath = Path.Combine(templatesDirectory, "Functions");
            foreach (var spaceDirectory in _persistedDirectory.GetDirectories(functionsTemplatePath))
            {
                var paths = _persistedDirectory.GetFiles(spaceDirectory);
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
                cell.Location.SheetName = sheetName;
            }
            return oldIdToNewIdMap;
        }

        private void RefreshOrderedSheetsList()
        {
            OrderedSheets.Clear();
            foreach (var sheet in Sheets.OrderBy(x => x.Order))
            {
                OrderedSheets.Add(sheet);
            }
        }

        private void SheetPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SheetModel.Order))
            {
                RefreshOrderedSheetsList();
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
    }
}
