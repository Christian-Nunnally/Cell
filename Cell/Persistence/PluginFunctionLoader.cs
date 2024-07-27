using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using Cell.Model;

namespace Cell.Persistence
{
    public static class PluginFunctionLoader
    {
        public const string FunctionsDirectoryName = "Functions";

        public static Dictionary<string, Dictionary<string, PluginFunction>> Namespaces { get; set; } = [];

        public static void LoadPlugins()
        {
            var functionsPath = Path.Combine(PersistenceManager.SaveLocation, FunctionsDirectoryName);
            if (Directory.Exists(functionsPath))
            {
                foreach (var namespacePath in Directory.GetDirectories(functionsPath))
                {
                    foreach (var file in Directory.GetFiles(namespacePath))
                    {
                        var function = JsonSerializer.Deserialize<PluginFunction>(File.ReadAllText(file));
                        if (function == null) continue;
                        var space = Path.GetFileName(namespacePath);
                        AddPluginFunctionToNamespace(space, function);
                    }
                }
            }
        }

        private static void AddPluginFunctionToNamespace(string space, PluginFunction function)
        {
            if (Namespaces.TryGetValue(space, out var namespaceFunctions)) namespaceFunctions.Add(function.Name, function);
            else Namespaces.Add(space, new Dictionary<string, PluginFunction> { { function.Name, function } });
            function.PropertyChanged += OnPluginFunctionPropertyChanged;
        }

        private static void OnPluginFunctionPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not PluginFunction function) return;
            SavePluginFunction(function.ReturnType, function);
        }

        public static void SavePlugins()
        {
            foreach (var namespaceFunctions in Namespaces)
            {
                foreach (var function in namespaceFunctions.Value.Values)
                {
                    var space = namespaceFunctions.Key;
                    SavePluginFunction(space, function);
                }
            }
        }

        public static void SavePluginFunction(string space, PluginFunction function)
        {
            if (string.IsNullOrEmpty(function.Name)) return;
            var directory = Path.Combine(PersistenceManager.SaveLocation, FunctionsDirectoryName, space);
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, function.Name);
            var serializedContent = JsonSerializer.Serialize(function);
            File.WriteAllText(path, serializedContent);
        }

        internal static PluginFunction GetOrCreateFunction(string space, string name)
        {
            if (TryGetFunction(space, name, out var function)) return function;
            return CreateFunction(space, name, string.Empty);
        }

        internal static bool TryGetFunction(string space, string name, [MaybeNullWhen(false)] out PluginFunction function)
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

        public static PluginFunction CreateFunction(string space, string name, string code)
        {
            if (space.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) throw new InvalidOperationException("Invalid space name for function, can not contain characters that are invalid in a file name.");
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) throw new InvalidOperationException("Invalid space name for function, can not contain characters that are invalid in a file name.");
            var function = new PluginFunction(name, code, space);
            AddPluginFunctionToNamespace(space, function);
            SavePluginFunction(space, function);
            return function;
        }
    }
}
