using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using Cell.Common;
using Cell.Model;
using Cell.ViewModel;

namespace Cell.Persistence
{
    public static class PluginFunctionLoader
    {
        public const string FunctionsDirectoryName = "Functions";

        public static ObservableCollection<PluginFunctionViewModel> ObservableFunctions { get; private set; } = [];

        public static Dictionary<string, Dictionary<string, PluginFunctionViewModel>> Namespaces { get; set; } = [];

        public static void LoadPlugins()
        {
            var functionsPath = Path.Combine(PersistenceManager.SaveLocation, FunctionsDirectoryName);
            if (Directory.Exists(functionsPath))
            {
                foreach (var namespacePath in Directory.GetDirectories(functionsPath))
                {
                    foreach (var file in Directory.GetFiles(namespacePath))
                    {
                        PluginFunctionModel? model = LoadFunction(file);
                        if (model == null) continue;
                        var function = new PluginFunctionViewModel(model);
                        var space = Path.GetFileName(namespacePath);
                        AddPluginFunctionToNamespace(space, function);
                    }
                }
            }
        }

        public static PluginFunctionModel LoadFunction(string file)
        {
            return JsonSerializer.Deserialize<PluginFunctionModel>(File.ReadAllText(file)) ?? throw new CellError($"Unable to load function from {file}");
        }

        public static void AddPluginFunctionToNamespace(string space, PluginFunctionViewModel function)
        {
            if (Namespaces.TryGetValue(space, out var namespaceFunctions)) namespaceFunctions.Add(function.Model.Name, function);
            else Namespaces.Add(space, new Dictionary<string, PluginFunctionViewModel> { { function.Model.Name, function } });
            ObservableFunctions.Add(function);
            function.Model.PropertyChanged += OnPluginFunctionPropertyChanged;
        }

        private static void OnPluginFunctionPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not PluginFunctionModel function) return;
            SavePluginFunction(PersistenceManager.SaveLocation, function.ReturnType, function);
        }

        public static void SavePlugins()
        {
            foreach (var namespaceFunctions in Namespaces)
            {
                foreach (var function in namespaceFunctions.Value.Values)
                {
                    var space = namespaceFunctions.Key;
                    SavePluginFunction(PersistenceManager.SaveLocation, space, function.Model);
                }
            }
        }

        public static void SavePluginFunction(string directory, string space, PluginFunctionModel function)
        {
            if (string.IsNullOrEmpty(function.Name)) return;
            directory = Path.Combine(directory, FunctionsDirectoryName, space);
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, function.Name);
            var serializedContent = JsonSerializer.Serialize(function);
            File.WriteAllText(path, serializedContent);
        }

        internal static PluginFunctionViewModel GetOrCreateFunction(string space, string name)
        {
            if (TryGetFunction(space, name, out var function)) return function;
            return CreateFunction(space, name, string.Empty);
        }

        internal static bool TryGetFunction(string space, string name, [MaybeNullWhen(false)] out PluginFunctionViewModel function)
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

        public static PluginFunctionViewModel CreateFunction(string space, string name, string code)
        {
            if (space.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) throw new InvalidOperationException("Invalid space name for function, can not contain characters that are invalid in a file name.");
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) throw new InvalidOperationException("Invalid space name for function, can not contain characters that are invalid in a file name.");
            var model = new PluginFunctionModel(name, code, space);
            var function = new PluginFunctionViewModel(model);
            AddPluginFunctionToNamespace(space, function);
            SavePluginFunction(PersistenceManager.SaveLocation, space, function.Model);
            return function;
        }

        internal static void DeleteFunction(PluginFunctionViewModel function)
        {
            if (Namespaces.TryGetValue(function.Model.ReturnType, out var namespaceFunctions))
            {
                function.Model.PropertyChanged -= OnPluginFunctionPropertyChanged;
                namespaceFunctions.Remove(function.Model.Name);
                ObservableFunctions.Remove(function);

                if (string.IsNullOrEmpty(function.Model.Name)) return;
                var directory = Path.Combine(PersistenceManager.SaveLocation, FunctionsDirectoryName, function.Model.ReturnType);
                Directory.CreateDirectory(directory);
                var path = Path.Combine(directory, function.Model.Name);
                File.Delete(path);
            }
        }
    }
}
