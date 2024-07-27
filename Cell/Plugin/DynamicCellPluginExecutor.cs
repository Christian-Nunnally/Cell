using Cell.Model;
using Cell.Persistence;

namespace Cell.Plugin
{
    internal static class DynamicCellPluginExecutor
    {
        public static List<string> Logs { get; } = [];

        public static CompileResult RunPopulate(PluginContext pluginContext, CellModel cell)
        {
            if (!PluginFunctionLoader.TryGetFunction("object", cell.PopulateFunctionName, out var populateFunction)) return new CompileResult { Success = false, Result = "Populate function not found" };
            var method = populateFunction.CompiledMethod;
            if (populateFunction.CompileResult.Success)
            {
                var resultObject = method?.Invoke(null, [pluginContext, cell]);
                var result = new CompileResult { Success = true, Result = resultObject?.ToString() ?? "" };
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

        public static void Log(string functionName, string sheet, int row, int column, CompileResult compileResult, bool isTrigger)
        {
            var logStart = isTrigger ? $"Trigger: {functionName}" : $"Populate: {functionName}";
            Logs.Add($"{logStart} - {sheet} - {row} - {column} - {compileResult.Success} - {compileResult.Result}");
        }
    }
}
