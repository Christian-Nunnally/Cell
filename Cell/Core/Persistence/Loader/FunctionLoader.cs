using Cell.Core.Common;
using Cell.Core.Data;
using Cell.Core.Execution.Functions;
using Cell.Model;
using System.ComponentModel;
using System.IO;
using System.Text.Json;

namespace Cell.Core.Persistence
{
    /// <summary>
    /// Tracks and loads plugin functions.
    /// </summary>
    public class FunctionLoader
    {
        private readonly PersistedDirectory _functionsDirectory;
        private readonly FunctionTracker _functionTracker;

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

        private void FunctionTrackerFunctionRemoved(CellFunction function)
        {
            DeleteCellFunction(function);
            function.Model.PropertyChanged -= FunctionModelPropertyChanged;
        }

        private void FunctionTrackerFunctionAdded(CellFunction function)
        {
            function.Model.PropertyChanged += FunctionModelPropertyChanged;
            SaveCellFunction(function.Model.ReturnType, function.Model);
        }

        private void FunctionModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var function = (CellFunctionModel)sender!;
            SaveCellFunction(function.ReturnType, function);
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
        public void LoadCellFunctions()
        {
            foreach (var namespacePath in _functionsDirectory.GetDirectories())
            {
                foreach (var file in _functionsDirectory.GetFiles(namespacePath))
                {
                    CellFunctionModel? model = LoadFunction(file);
                    if (model == null) continue;
                    var function = new CellFunction(model);
                    var space = Path.GetFileName(namespacePath);
                    _functionTracker.AddCellFunctionToNamespace(space, function);
                }
            }
        }

        /// <summary>
        /// Loads a cell function from the given file in the directory.
        /// </summary>
        /// <param name="file">The function file to load.</param>
        /// <returns>The loaded function model.</returns>
        /// <exception cref="CellError">If the function was not able to be loaded.</exception>
        public CellFunctionModel LoadFunction(string file)
        {
            var text = _functionsDirectory.LoadFile(file) ?? throw new CellError($"Unable to load function from {file}");
            return JsonSerializer.Deserialize<CellFunctionModel>(text) ?? throw new CellError($"Unable to load function from {file}");
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
    }
}
