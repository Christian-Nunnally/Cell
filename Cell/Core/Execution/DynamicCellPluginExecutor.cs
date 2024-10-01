using Cell.Common;
using Cell.Model;
using Cell.Persistence;
using System.Collections;

namespace Cell.Execution
{
    public static class DynamicCellPluginExecutor
    {
        public static void Log(string functionName, string sheet, int row, int column, CompileResult compileResult, bool isTrigger)
        {
            var logStart = isTrigger ? $"Trigger: {functionName}" : $"Populate: {functionName}";
            Logger.Instance.Log($"{logStart} - {sheet} - {row} - {column} - {compileResult.WasSuccess} - {compileResult.ExecutionResult}");
        }

        public static int? RunSortFilter(PluginFunctionLoader pluginFunctionLoader, Context pluginContext, string functionName)
        {
            if (!pluginFunctionLoader.TryGetFunction("object", functionName, out var populateFunction)) return 0;
            var method = populateFunction.CompiledMethod;
            if (populateFunction.CompileResult.WasSuccess)
            {
                var resultObject = method?.Invoke(null, [pluginContext, null]);
                return ConvertReturnedObjectToSortFilterResult(resultObject);
            }
            return 0;
        }

        public static CompileResult RunTrigger(PluginFunctionLoader pluginFunctionLoader, Context pluginContext, CellModel cell)
        {
            if (!pluginFunctionLoader.TryGetFunction("void", cell.TriggerFunctionName, out var triggerFunction)) return new CompileResult { WasSuccess = false, ExecutionResult = "Trigger function not found" };
            Log(cell.TriggerFunctionName, cell.SheetName, cell.Row, cell.Column, triggerFunction.CompileResult, true);
            return triggerFunction.Run(pluginContext, cell);
        }

        private static int? ConvertReturnedObjectToSortFilterResult(object? resultObject)
        {
            if (resultObject is null) return null;
            if (int.TryParse(resultObject.ToString(), out var resultInt)) return resultInt;
            return null;
        }
    }
}
