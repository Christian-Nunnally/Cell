using Cell.Model;
using Cell.Persistence;
using System.Collections;

namespace Cell.Plugin
{
    internal static class DynamicCellPluginExecutor
    {
        public static List<string> Logs { get; } = [];

        public static void Log(string functionName, string sheet, int row, int column, CompileResult compileResult, bool isTrigger)
        {
            var logStart = isTrigger ? $"Trigger: {functionName}" : $"Populate: {functionName}";
            Logs.Add($"{logStart} - {sheet} - {row} - {column} - {compileResult.Success} - {compileResult.Result}");
        }

        public static CompileResult RunPopulate(PluginContext pluginContext, CellModel cell)
        {
            if (!PluginFunctionLoader.TryGetFunction("object", cell.PopulateFunctionName, out var populateFunction)) return new CompileResult { Success = false, Result = "Populate function not found" };
            var method = populateFunction.CompiledMethod;
            if (populateFunction.CompileResult.Success)
            {
                var resultObject = method?.Invoke(null, [pluginContext, cell]);
                var resultString = ConvertReturnedObjectToString(resultObject);
                var result = new CompileResult { Success = true, Result = resultString };
                Log(cell.TriggerFunctionName, cell.SheetName, cell.Row, cell.Column, result, false);
                return result;
            }
            Log(cell.TriggerFunctionName, cell.SheetName, cell.Row, cell.Column, populateFunction.CompileResult, false);
            return populateFunction.CompileResult;
        }

        public static CompileResult RunTrigger(PluginContext pluginContext, CellModel cell)
        {
            if (!PluginFunctionLoader.TryGetFunction("void", cell.TriggerFunctionName, out var triggerFunction)) return new CompileResult { Success = false, Result = "Trigger function not found" };
            var method = triggerFunction.CompiledMethod;
            if (triggerFunction.CompileResult.Success)
            {
                method?.Invoke(null, [pluginContext, cell]);
                var result = new CompileResult { Success = true, Result = string.Empty };
                Log(cell.TriggerFunctionName, cell.SheetName, cell.Row, cell.Column, result, true);
                return result;
            }
            Log(cell.TriggerFunctionName, cell.SheetName, cell.Row, cell.Column, triggerFunction.CompileResult, true);
            return triggerFunction.CompileResult;
        }

        internal static int? RunSortFilter(PluginContext pluginContext, string functionName)
        {
            if (!PluginFunctionLoader.TryGetFunction("object", functionName, out var populateFunction)) return 0;
            var method = populateFunction.CompiledMethod;
            if (populateFunction.CompileResult.Success)
            {
                var resultObject = method?.Invoke(null, [pluginContext, null]);
                return ConvertReturnedObjectToSortFilterResult(resultObject);
            }
            return 0;
        }

        private static int? ConvertReturnedObjectToSortFilterResult(object? resultObject)
        {
            if (resultObject is null) return null;
            if (int.TryParse(resultObject.ToString(), out var resultInt)) return resultInt;
            return null;
        }

        private static string? ConvertReturnedObjectToString(object? resultObject)
        {
            if (resultObject is null) return null;
            if (resultObject is IEnumerable resultEnumerable && resultObject is not string)
            {
                var resultString = "";
                foreach (var item in resultEnumerable)
                {
                    resultString += item?.ToString() + ",";
                }
                if (resultString.Length > 0) return resultString[..^1];
                return resultString;
            }
            return resultObject?.ToString() ?? "";
        }
    }
}
