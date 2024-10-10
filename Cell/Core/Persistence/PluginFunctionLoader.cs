using Cell.Common;
using Cell.Model;
using Cell.ViewModel.Execution;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;

namespace Cell.Persistence
{
    /// <summary>
    /// Tracks and loads plugin functions.
    /// </summary>
    public class PluginFunctionLoader
    {
        private const string FunctionsDirectoryName = "Functions";
        private readonly PersistedDirectory _persistanceManager;
        /// <summary>
        /// Creates a new instance of the <see cref="PluginFunctionLoader"/> class.
        /// </summary>
        /// <param name="persistedDirectory">The project directory to load the functions from.</param>
        public PluginFunctionLoader(PersistedDirectory persistedDirectory)
        {
            _persistanceManager = persistedDirectory;
        }

        /// <summary>
        /// An observable collection of all the loaded cell functions.
        /// </summary>
        public ObservableCollection<CellFunction> CellFunctions { get; private set; } = [];

        private Dictionary<string, Dictionary<string, CellFunction>> Namespaces { get; set; } = [];

        /// <summary>
        /// Starts tracking a function.
        /// </summary>
        /// <param name="space">The namespace of the function.</param>
        /// <param name="function">The function.</param>
        public void AddCellFunctionToNamespace(string space, CellFunction function)
        {
            if (Namespaces.TryGetValue(space, out var namespaceFunctions)) namespaceFunctions.Add(function.Model.Name, function);
            else Namespaces.Add(space, new Dictionary<string, CellFunction> { { function.Model.Name, function } });
            CellFunctions.Add(function);
            function.Model.PropertyChanged += OnCellFunctionModelPropertyChanged;
        }

        /// <summary>
        /// Creates a cell function in the given namespace with the given name.
        /// </summary>
        /// <param name="space">The namespace to create the function in.</param>
        /// <param name="name">The name to give the function.</param>
        /// <param name="code">(optional) The initial code the function should have.</param>
        /// <returns>The newly created function.</returns>
        /// <exception cref="InvalidOperationException">If the name or space are not valid for a function.</exception>
        public CellFunction CreateCellFunction(string space, string name, string code = "")
        {
            if (space.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) throw new InvalidOperationException("Invalid space name for function, can not contain characters that are invalid in a file name.");
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) throw new InvalidOperationException("Invalid space name for function, can not contain characters that are invalid in a file name.");
            var model = new CellFunctionModel(name, code, space);
            var function = new CellFunction(model);
            AddCellFunctionToNamespace(space, function);
            SaveCellFunction("", space, function.Model);
            return function;
        }

        /// <summary>
        /// Deletes a cell function from the project, which means it will be deleted from disk.
        /// </summary>
        /// <param name="function">The function to delete.</param>
        public void DeleteCellFunction(CellFunction function)
        {
            if (Namespaces.TryGetValue(function.Model.ReturnType, out var namespaceFunctions))
            {
                function.Model.PropertyChanged -= OnCellFunctionModelPropertyChanged;
                namespaceFunctions.Remove(function.Model.Name);
                CellFunctions.Remove(function);

                if (string.IsNullOrEmpty(function.Model.Name)) return;
                var path = Path.Combine(FunctionsDirectoryName, function.Model.ReturnType, function.Model.Name);
                _persistanceManager.DeleteFile(path);
            }
        }

        /// <summary>
        /// Gets a cell function in the given namespace with the given name if it exists, otherwise creates it.
        /// </summary>
        /// <param name="space">The namespace for the function.</param>
        /// <param name="name">The name of the function.</param>
        /// <returns>The existing or newly created function.</returns>
        public CellFunction GetOrCreateFunction(string space, string name)
        {
            if (TryGetCellFunction(space, name, out var function)) return function;
            return CreateCellFunction(space, name);
        }

        /// <summary>
        /// Loads all the cell functions from the project directory.
        /// </summary>
        public void LoadCellFunctions()
        {
            if (_persistanceManager.DirectoryExists(FunctionsDirectoryName))
            {
                foreach (var namespacePath in _persistanceManager.GetDirectories(FunctionsDirectoryName))
                {
                    foreach (var file in _persistanceManager.GetFiles(namespacePath))
                    {
                        CellFunctionModel? model = LoadFunction(file);
                        if (model == null) continue;
                        var function = new CellFunction(model);
                        var space = Path.GetFileName(namespacePath);
                        AddCellFunctionToNamespace(space, function);
                    }
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
            var text = _persistanceManager.LoadFile(file) ?? throw new CellError($"Unable to load function from {file}");
            return JsonSerializer.Deserialize<CellFunctionModel>(text) ?? throw new CellError($"Unable to load function from {file}");
        }

        /// <summary>
        /// Saves a cell function to the directory.
        /// </summary>
        /// <param name="directory">The relative path to save to.</param>
        /// <param name="space">The namespace to save the function in.</param>
        /// <param name="function">The function to save.</param>
        public void SaveCellFunction(string directory, string space, CellFunctionModel function)
        {
            if (string.IsNullOrWhiteSpace(function.Name)) return;
            directory = string.IsNullOrEmpty(directory) ? Path.Combine(FunctionsDirectoryName, space) : Path.Combine(directory, FunctionsDirectoryName, space);
            var path = Path.Combine(directory, function.Name);
            var serializedContent = JsonSerializer.Serialize(function);
            _persistanceManager.SaveFile(path, serializedContent);
        }

        /// <summary>
        /// Attempts to get a cell function from the given namespace and name.
        /// </summary>
        /// <param name="space">The namespace to get the function from.</param>
        /// <param name="name">The name of the function to get.</param>
        /// <param name="function">The function, if it exists.</param>
        /// <returns>True if the function exists.</returns>
        public bool TryGetCellFunction(string space, string name, [MaybeNullWhen(false)] out CellFunction function)
        {
            function = null;
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(space)) return false;
            if (Namespaces.TryGetValue(space, out var namespaceFunctions))
            {
                if (namespaceFunctions.TryGetValue(name, out var value))
                {
                    function = value;
                    return true;
                }
            }
            return false;
        }

        private void OnCellFunctionModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var function = (CellFunctionModel)sender!;
            SaveCellFunction("", function.ReturnType, function);
        }
    }
}
