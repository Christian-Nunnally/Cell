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
            Logger.Log($"{logStart} - {sheet} - {row} - {column} - {compileResult.WasSuccess} - {compileResult.ExecutionResult}");
        }

        public static CompileResult RunPopulate(PluginFunctionLoader pluginFunctionLoader, PluginContext pluginContext, CellModel cell)
        {
            if (!pluginFunctionLoader.TryGetFunction("object", cell.PopulateFunctionName, out var populateFunction)) return new CompileResult { WasSuccess = false, ExecutionResult = "Populate function not found" };
            Log(cell.TriggerFunctionName, cell.SheetName, cell.Row, cell.Column, populateFunction.CompileResult, false);
            var result = populateFunction.Run(pluginContext, cell);
            result = ConvertReturnedObjectToString(result);
            return result;
        }

        public static CompileResult RunTrigger(PluginFunctionLoader pluginFunctionLoader, PluginContext pluginContext, CellModel cell)
        {
            if (!pluginFunctionLoader.TryGetFunction("void", cell.TriggerFunctionName, out var triggerFunction)) return new CompileResult { WasSuccess = false, ExecutionResult = "Trigger function not found" };
            Log(cell.TriggerFunctionName, cell.SheetName, cell.Row, cell.Column, triggerFunction.CompileResult, true);
            return triggerFunction.Run(pluginContext, cell);
        }

        public static int? RunSortFilter(PluginFunctionLoader pluginFunctionLoader, PluginContext pluginContext, string functionName)
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

        private static int? ConvertReturnedObjectToSortFilterResult(object? resultObject)
        {
            if (resultObject is null) return null;
            if (int.TryParse(resultObject.ToString(), out var resultInt)) return resultInt;
            return null;
        }

        private static CompileResult ConvertReturnedObjectToString(CompileResult compileResult)
        {
            if (!compileResult.WasSuccess) return compileResult;
            if (compileResult.ReturnedObject is null) return compileResult;
            if (compileResult.ReturnedObject is IEnumerable resultEnumerable && compileResult.ReturnedObject is not string)
            {
                var resultString = "";
                foreach (var item in resultEnumerable)
                {
                    resultString += item?.ToString() + ",";
                }
                if (resultString.Length > 0) resultString = resultString[..^1];
                compileResult.ExecutionResult = resultString;
            }
            compileResult.ExecutionResult = compileResult.ReturnedObject.ToString() ?? "";
            return compileResult;
        }
    }
}
