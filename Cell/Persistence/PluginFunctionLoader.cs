using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using Cell.Model;

namespace Cell.Persistence
{
    public static class PluginFunctionLoader
    {
        public const string PopulateFunctionsDirectoryName = "Populate";
        public const string TriggerFunctionsDirectoryName = "Trigger";

        public static Dictionary<string, PluginFunction> PopulateFunctions { get; set; } = [];

        public static Dictionary<string, PluginFunction> TriggerFunctions { get; set; } = [];

        public static void LoadPlugins()
        {
            var path = Path.Combine(PersistenceManager.SaveLocation, PopulateFunctionsDirectoryName);
            if (Directory.Exists(path))
            {
                foreach (var file in Directory.GetFiles(path))
                {
                    var function = JsonSerializer.Deserialize<PluginFunction>(File.ReadAllText(file));
                    if (function == null) continue;
                    PopulateFunctions.Add(function.Name, function);
                }
            }
            path = Path.Combine(PersistenceManager.SaveLocation, TriggerFunctionsDirectoryName);
            if (Directory.Exists(path))
            {
                foreach (var file in Directory.GetFiles(path))
                {
                    var function = JsonSerializer.Deserialize<PluginFunction>(File.ReadAllText(file));
                    if (function == null) continue;
                    TriggerFunctions.Add(function.Name, function);
                }
            }
        }

        public static void SavePlugins()
        {
            foreach (var getTextPluginCode in PopulateFunctions.Values)
            {
                SavePluginFunction(getTextPluginCode, PopulateFunctionsDirectoryName);
            }
            foreach (var getTextPluginCode in TriggerFunctions.Values)
            {
                SavePluginFunction(getTextPluginCode, TriggerFunctionsDirectoryName);
            }
        }

        public static void SavePluginFunction(PluginFunction function, string subdirectoryName)
        {
            var path = Path.Combine(PersistenceManager.SaveLocation, subdirectoryName);
            Directory.CreateDirectory(path);
            File.WriteAllText(Path.Combine(path, function.Name), JsonSerializer.Serialize(function));
        }

        internal static bool TryGetPopulateFunction(string name, [MaybeNullWhen(false)] out PluginFunction function)
        {
            if (PopulateFunctions.TryGetValue(name, out PluginFunction? value))
            {
                function = value;
                return true;
            }
            function = null;
            return false;
        }

        internal static void SetPopulateFunction(string name, string value, bool createIfDoesNotExist)
        {
            if (PopulateFunctions.TryGetValue(name, out PluginFunction? function))
            {
                function.Code = value;
                function.FindAndRefreshDependencies();
                SavePluginFunction(function, PopulateFunctionsDirectoryName);
            }
            else if (createIfDoesNotExist)
            {
                CreatePopulateFunction(name, value);
            }
        }

        public static void CreatePopulateFunction(string name, string code = "return \"Hello world\"")
        {
            if (PopulateFunctions.ContainsKey(name)) return;
            var function = new PluginFunction
            {
                Name = name,
                Code = code,
                IsTrigger = false,
            };
            PopulateFunctions.Add(function.Name, function);
        }

        internal static bool TryGetTriggerFunction(string name, [MaybeNullWhen(false)] out PluginFunction function)
        {
            if (TriggerFunctions.TryGetValue(name, out PluginFunction? value))
            {
                function = value;
                return true;
            }
            function = null;
            return false;
        }

        internal static void TrySetTriggerFunction(string name, string value, bool createIfDoesNotExist)
        {
            if (TriggerFunctions.TryGetValue(name, out PluginFunction? function))
            {
                function.Code = value;
                SavePluginFunction(function, TriggerFunctionsDirectoryName);
            }
            else if (createIfDoesNotExist)
            {
                CreateTriggerFunction(name, value);
            }
        }

        public static void CreateTriggerFunction(string name, string code = "")
        {
            if (TriggerFunctions.ContainsKey(name)) return;
            var function = new PluginFunction
            {
                Name = name,
                Code = code,
                IsTrigger = true,
            };
            TriggerFunctions.Add(function.Name, function);
            SavePluginFunction(function, TriggerFunctionsDirectoryName);
        }
    }
}
