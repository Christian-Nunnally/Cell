using Cell.Common;
using Cell.Data;
using Cell.Model;
using Cell.View.ToolWindow;
using Cell.ViewModel;
using System.IO;
using System.Text.Json;

namespace Cell.Persistence
{
    public class CellLoader(string saveDirectory)
    {
        private const string SheetsSaveDirectory = "Sheets";
        private const string TemplatesSaveDirectory = "Templates";
        private readonly string _saveDirectory = saveDirectory;

        public void LoadAndAddCells()
        {
            var sheetsPath = Path.Combine(_saveDirectory, SheetsSaveDirectory);
            if (!Directory.Exists(sheetsPath)) return;
            foreach (var directory in Directory.GetDirectories(sheetsPath)) LoadAndAddSheet(directory);
        }

        private static void LoadAndAddSheet(string directory)
        {
            foreach (var file in Directory.GetFiles(directory)) LoadAndAddCell(file);
        }

        public static IEnumerable<CellModel> LoadSheet(string directory)
        {
            var result = new List<CellModel>();
            foreach (var file in Directory.GetFiles(directory)) result.Add(LoadAndAddCell(file));
            return result;
        }

        public void SaveCells()
        {
            foreach (var sheet in Cells.Instance.SheetNames) SaveSheet(sheet);
        }

        private void SaveSheet(string sheet)
        {
            foreach (var cell in Cells.Instance.GetCellModelsForSheet(sheet)) SaveCell(cell);
        }

        public void RenameSheet(string oldName, string newName)
        {
            var oldDirectory = Path.Combine(_saveDirectory, SheetsSaveDirectory, oldName);
            var newDirectory = Path.Combine(_saveDirectory, SheetsSaveDirectory, newName);
            Directory.Move(oldDirectory, newDirectory);
        }

        public void SaveCell(CellModel cell)
        {
            var directory = Path.Combine(_saveDirectory, SheetsSaveDirectory, cell.SheetName);
            Directory.CreateDirectory(directory);
            var serialized = JsonSerializer.Serialize(cell);
            var path = Path.Combine(directory, cell.ID);
            File.WriteAllText(path, serialized);
        }

        public static CellModel LoadCell(string file)
        {
            var text = File.ReadAllText(file) ?? throw new CellError($"Loading file failed at {file}"); ;
            var cell = JsonSerializer.Deserialize<CellModel>(text) ?? throw new CellError($"Deserialization failed for {File.ReadAllText(file)} at {file}");
            return cell;
        }

        private static CellModel LoadAndAddCell(string file)
        {
            var cell = LoadCell(file);
            Cells.Instance.AddCell(cell, saveAfterAdding: false);
            return cell;
        }

        public void DeleteCell(CellModel cellModel)
        {
            var directory = Path.Combine(_saveDirectory, SheetsSaveDirectory, cellModel.SheetName);
            if (!Directory.Exists(directory)) return;
            var path = Path.Combine(directory, cellModel.ID);
            File.Delete(path);
        }

        public void ExportSheetTemplate(string sheetName)
        {
            var copiedCells = Cells.Instance.GetCellModelsForSheet(sheetName).Select(c => c.Copy());
            foreach (var copiedCell in copiedCells)
            {
                copiedCell.SheetName = "";
            }
            var directory = Path.Combine(_saveDirectory, TemplatesSaveDirectory, sheetName);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            foreach (var copiedCell in copiedCells)
            {
                var serialized = JsonSerializer.Serialize(copiedCell);
                var cellPath = Path.Combine(directory, copiedCell.ID);
                File.WriteAllText(cellPath, serialized);
            }

            foreach (var function in PluginFunctionLoader.ObservableFunctions.Where(x => x.CellsThatUseFunction.Any(c => c.SheetName == sheetName)))
            {
                PluginFunctionLoader.SavePluginFunction(directory, function.Model.ReturnType, function.Model);
            }
        }

        public void ImportSheetTemplate(string templateName, string sheetName)
        {
            var templatesDirectory = Path.Combine(_saveDirectory, TemplatesSaveDirectory);
            if (!Directory.Exists(templatesDirectory)) return;
            var templatePath = Path.Combine(templatesDirectory, templateName);
            if (!Directory.Exists(templatePath)) return;

            var cellsToAdd = LoadSheet(templatePath);
            var oldIdToNewIdMap = GiveCellsNewUniqueIndentities(sheetName, cellsToAdd);
            FixMergedCellsWithNewIdentities(cellsToAdd, oldIdToNewIdMap);

            var functionsBeingImported = GetFunctionsFromTemplate(templatesDirectory);
            if (!CanFunctionsBeMerged(functionsBeingImported, out var reason))
            {
               DialogWindow.ShowDialog("Import canceled", reason);
                return;
            }

            foreach (var cell in cellsToAdd)
            {
                Cells.Instance.AddCell(cell, saveAfterAdding: true);
            }

            foreach (var functionModel in functionsBeingImported)
            {
                var function = new PluginFunctionViewModel(functionModel);
                PluginFunctionLoader.AddPluginFunctionToNamespace(functionModel.ReturnType, function);
                PluginFunctionLoader.SavePluginFunction(_saveDirectory, functionModel.ReturnType, functionModel);
            }
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

            static PluginFunctionViewModel? GetExistingFunction(PluginFunctionModel function)
            {
                return PluginFunctionLoader.ObservableFunctions.FirstOrDefault(x => x.Model.Name == function.Name);
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

        private static void FixMergedCellsWithNewIdentities(IEnumerable<CellModel> cells, Dictionary<string, string> oldIdToNewIdMap)
        {
            foreach (var cell in cells.Where(cell => cell.MergedWith != string.Empty))
            {
                cell.MergedWith = oldIdToNewIdMap[cell.MergedWith];
            }
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
    }
}
