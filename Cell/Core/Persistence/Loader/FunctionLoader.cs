using Cell.Core.Common;
using Cell.Core.Data.Tracker;
using Cell.Core.Execution.Functions;
using Cell.Model;
using System.ComponentModel;
using System.IO;
using System.Text.Json;

namespace Cell.Core.Persistence.Loader
{
    /// <summary>
    /// Tracks and loads plugin functions.
    /// </summary>
    public class FunctionLoader
    {
        private readonly PersistedDirectory _functionsDirectory;
        private readonly FunctionTracker _functionTracker;
        private bool _shouldSaveAddedFunctions = true;
        /// <summary>
        /// Creates a new instance of the <see cref="FunctionLoader"/> class.
        /// </summary>
        /// <param name="functionsDirectory">A directory to load and store functions to.</param>
        /// <param name="functionTracker">A tracker to add loaded functions to.</param>
        public FunctionLoader(PersistedDirectory functionsDirectory, FunctionTracker functionTracker)
        {
            _functionsDirectory = functionsDirectory;
            _functionTracker = functionTracker;
            _functionTracker.FunctionAdded += FunctionTrackerFunctionAdded;
            _functionTracker.FunctionRemoved -= FunctionTrackerFunctionRemoved;
        }

        /// <summary>
        /// Deletes a cell function from the project, which means it will be deleted from disk.
        /// </summary>
        /// <param name="function">The function to delete.</param>
        public void DeleteCellFunction(CellFunction function)
        {
            var model = function.Model;
            if (string.IsNullOrEmpty(model.Name)) return;
            var path = Path.Combine(model.ReturnType, model.Name);
            _functionsDirectory.DeleteFile(path);
        }

        /// <summary>
        /// Loads all the cell functions from the project directory.
        /// </summary>
        public async Task LoadCellFunctionsAsync()
        {
            foreach (var namespacePath in _functionsDirectory.GetDirectories())
            {
                foreach (var file in _functionsDirectory.GetFiles(namespacePath))
                {
                    CellFunctionModel? model = await LoadFunctionAsync(file);
                    if (model is null) continue;
                    var function = new CellFunction(model);
                    var space = Path.GetFileName(namespacePath);
                    _shouldSaveAddedFunctions = false;
                    _functionTracker.AddCellFunctionToNamespace(space, function);
                    _shouldSaveAddedFunctions = true;
                }
            }
        }

        /// <summary>
        /// Saves a cell function to the directory.
        /// </summary>
        /// <param name="space">The namespace to save the function in.</param>
        /// <param name="function">The function to save.</param>
        public void SaveCellFunction(string space, CellFunctionModel function)
        {
            if (string.IsNullOrWhiteSpace(function.Name)) return;
            var path = Path.Combine(space, function.Name);
            var serializedContent = JsonSerializer.Serialize(function);
            _functionsDirectory.SaveFile(path, serializedContent);
        }

        private void FunctionModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var function = (CellFunctionModel)sender!;
            SaveCellFunction(function.ReturnType, function);
        }

        private void FunctionTrackerFunctionAdded(CellFunction function)
        {
            function.Model.PropertyChanged += FunctionModelPropertyChanged;
            if (_shouldSaveAddedFunctions) SaveCellFunction(function.Model.ReturnType, function.Model);
        }

        private void FunctionTrackerFunctionRemoved(CellFunction function)
        {
            function.Model.PropertyChanged -= FunctionModelPropertyChanged;
            DeleteCellFunction(function);
        }

        private async Task<CellFunctionModel> LoadFunctionAsync(string file)
        {
            var text = await _functionsDirectory.LoadFileAsync(file) ?? throw new CellError($"Unable to load function from {file}");
            return JsonSerializer.Deserialize<CellFunctionModel>(text) ?? throw new CellError($"Unable to load function from {file}");
        }
    }
}
