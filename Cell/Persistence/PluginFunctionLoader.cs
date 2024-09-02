using Cell.Common;
using Cell.Model;
using Cell.ViewModel.Application;
using Cell.ViewModel.Execution;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;

namespace Cell.Persistence
{
    public class PluginFunctionLoader
    {
        public const string FunctionsDirectoryName = "Functions";
        private readonly PersistenceManager _persistanceManager;

        public static Dictionary<string, Dictionary<string, FunctionViewModel>> Namespaces { get; set; } = [];

        public PluginFunctionLoader(PersistenceManager persistenceManager)
        {
            _persistanceManager = persistenceManager;
        }

        public ObservableCollection<FunctionViewModel> ObservableFunctions { get; private set; } = [];

        public void AddPluginFunctionToNamespace(string space, FunctionViewModel function)
        {
            if (Namespaces.TryGetValue(space, out var namespaceFunctions)) namespaceFunctions.Add(function.Model.Name, function);
            else Namespaces.Add(space, new Dictionary<string, FunctionViewModel> { { function.Model.Name, function } });
            ObservableFunctions.Add(function);
            function.Model.PropertyChanged += OnPluginFunctionPropertyChanged;
        }

        public FunctionViewModel CreateFunction(string space, string name, string code)
        {
            if (space.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) throw new InvalidOperationException("Invalid space name for function, can not contain characters that are invalid in a file name.");
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) throw new InvalidOperationException("Invalid space name for function, can not contain characters that are invalid in a file name.");
            var model = new PluginFunctionModel(name, code, space);
            var function = new FunctionViewModel(model);
            AddPluginFunctionToNamespace(space, function);
            SavePluginFunction("", space, function.Model);
            return function;
        }

        public PluginFunctionModel LoadFunction(string file)
        {
            var text = _persistanceManager.LoadFile(file) ?? throw new CellError($"Unable to load function from {file}");
            return JsonSerializer.Deserialize<PluginFunctionModel>(text) ?? throw new CellError($"Unable to load function from {file}");
        }

        public void LoadPlugins()
        {
            if (_persistanceManager.DirectoryExists(FunctionsDirectoryName))
            {
                foreach (var namespacePath in _persistanceManager.GetDirectories(FunctionsDirectoryName))
                {
                    foreach (var file in _persistanceManager.GetFiles(namespacePath))
                    {
                        PluginFunctionModel? model = LoadFunction(file);
                        if (model == null) continue;
                        var function = new FunctionViewModel(model);
                        var space = Path.GetFileName(namespacePath);
                        AddPluginFunctionToNamespace(space, function);
                    }
                }
            }
        }

        public static void SavePluginFunction(string directory, string space, PluginFunctionModel function)
        {
            if (string.IsNullOrWhiteSpace(function.Name)) return;
            directory = string.IsNullOrEmpty(directory) ? Path.Combine(FunctionsDirectoryName, space): Path.Combine(directory, FunctionsDirectoryName, space);
            var path = Path.Combine(directory, function.Name);
            var serializedContent = JsonSerializer.Serialize(function);
            ApplicationViewModel.Instance.PersistenceManager.SaveFile(path, serializedContent);
        }

        public static void SavePlugins()
        {
            foreach (var namespaceFunctions in Namespaces)
            {
                foreach (var function in namespaceFunctions.Value.Values)
                {
                    var space = namespaceFunctions.Key;
                    SavePluginFunction("", space, function.Model);
                }
            }
        }

        internal void DeleteFunction(FunctionViewModel function)
        {
            if (Namespaces.TryGetValue(function.Model.ReturnType, out var namespaceFunctions))
            {
                function.Model.PropertyChanged -= OnPluginFunctionPropertyChanged;
                namespaceFunctions.Remove(function.Model.Name);
                ObservableFunctions.Remove(function);

                if (string.IsNullOrEmpty(function.Model.Name)) return;
                var path = Path.Combine(FunctionsDirectoryName, function.Model.ReturnType, function.Model.Name);
                _persistanceManager.DeleteFile(path);
            }
        }

        internal FunctionViewModel GetOrCreateFunction(string space, string name)
        {
            if (TryGetFunction(space, name, out var function)) return function;
            return CreateFunction(space, name, string.Empty);
        }

        internal static bool TryGetFunction(string space, string name, [MaybeNullWhen(false)] out FunctionViewModel function)
        {
            if (Namespaces.TryGetValue(space, out var namespaceFunctions))
            {
                if (namespaceFunctions.TryGetValue(name, out var value))
                {
                    function = value;
                    return true;
                }
            }
            function = null;
            return false;
        }

        private static void OnPluginFunctionPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not PluginFunctionModel function) return;
            SavePluginFunction("", function.ReturnType, function);
        }
    }
}
