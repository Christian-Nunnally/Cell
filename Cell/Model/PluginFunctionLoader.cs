using System.IO;
using System.Text.Json;

namespace Cell.Model
{
    public static class PluginFunctionLoader
    {
        public static Dictionary<string, PluginFunction> GetTextPluginCodes { get; set; } = [];

        public static Dictionary<string, PluginFunction> OnEditPluginCodes { get; set; } = [];

        public static void LoadPlugins()
        {
            var path = Path.Combine(CellLoader.DefaultSaveLocation, "GetTextFunctions");
            if (Directory.Exists(path))
            {
                foreach (var file in Directory.GetFiles(path))
                {
                    var pluginFunction = JsonSerializer.Deserialize<PluginFunction>(File.ReadAllText(file));
                    if (pluginFunction == null) continue;
                    OnEditPluginCodes.Add(pluginFunction.Name, pluginFunction);
                }
            }
            path = Path.Combine(CellLoader.DefaultSaveLocation, "OnEditFunctions");
            if (Directory.Exists(path))
            {
                foreach (var file in Directory.GetFiles(path))
                {
                    var pluginFunction = JsonSerializer.Deserialize<PluginFunction>(File.ReadAllText(file));
                    if (pluginFunction == null) continue;
                    OnEditPluginCodes.Add(pluginFunction.Name, pluginFunction);
                }
            }
        }

        public static void SavePlugins()
        {
            var path = Path.Combine(CellLoader.DefaultSaveLocation, "OnEditFunctions");
            Directory.CreateDirectory(path);
            foreach (var getTextPluginCode in OnEditPluginCodes.Values)
            {
                File.WriteAllText(Path.Combine(path, getTextPluginCode.Name), JsonSerializer.Serialize(getTextPluginCode));
            }
            path = Path.Combine(CellLoader.DefaultSaveLocation, "GetTextFunctions");
            Directory.CreateDirectory(path);
            foreach (var getTextPluginCode in GetTextPluginCodes.Values)
            {
                File.WriteAllText(Path.Combine(path, getTextPluginCode.Name), JsonSerializer.Serialize(getTextPluginCode));
            }
        }

        internal static string GetTextPluginFunctionCodeIfAvailable(string getTextFunctionName)
        {
            if (GetTextPluginCodes.ContainsKey(getTextFunctionName))
            {
                return GetTextPluginCodes[getTextFunctionName].Code;
            }
            return string.Empty;
        }

        internal static void SetTextPluginFunctionCodeIfAvailable(string getTextFunctionName, string value)
        {
            if (GetTextPluginCodes.TryGetValue(getTextFunctionName, out PluginFunction? pluginFunction))
            {
                pluginFunction.Code = value;
            }
        }

        public static void CreateGetTextPluginFunction(string functionName)
        {
            if (GetTextPluginCodes.ContainsKey(functionName)) return;
            var pluginFunction = new PluginFunction
            {
                Name = functionName,
                Code = "return \"Hello world\"",
            };
            GetTextPluginCodes.Add(pluginFunction.Name, pluginFunction);
        }

        internal static string GetOnEditPluginFunctionCodeIfAvailable(string getTextFunctionName)
        {
            if (OnEditPluginCodes.ContainsKey(getTextFunctionName))
            {
                return OnEditPluginCodes[getTextFunctionName].Code;
            }
            return string.Empty;
        }

        internal static void SetOnEditPluginFunctionCodeIfAvailable(string getTextFunctionName, string value)
        {
            if (OnEditPluginCodes.TryGetValue(getTextFunctionName, out PluginFunction? pluginFunction))
            {
                pluginFunction.Code = value;
            }
        }

        public static void CreateOnEditPluginFunction(string functionName)
        {
            if (OnEditPluginCodes.ContainsKey(functionName)) return;
            var pluginFunction = new PluginFunction
            {
                Name = functionName,
                Code = "return \"Hello world\"",
            };
            OnEditPluginCodes.Add(pluginFunction.Name, pluginFunction);
        }
    }
}
