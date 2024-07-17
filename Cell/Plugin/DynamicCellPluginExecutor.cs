using Cell.Model;
using Cell.Persistence;

namespace Cell.Plugin
{
    internal static class DynamicCellPluginExecutor
    {
        public static CompileResult RunPopulate(PluginContext pluginContext, CellModel cell)
        {
            if (!PluginFunctionLoader.TryGetFunction(PluginFunctionLoader.PopulateFunctionsDirectoryName, cell.PopulateFunctionName, out var populateFunction)) return new CompileResult { Success = false, Result = "Populate function not found" };
            var method = populateFunction.CompiledMethod;
            if (populateFunction.CompileResult.Success)
            {
                var resultObject = method?.Invoke(null, [pluginContext, cell]);
                return new CompileResult { Success = true, Result = resultObject?.ToString() ?? "" };
            }
            return populateFunction.CompileResult;
        }

        public static CompileResult RunTrigger(PluginContext pluginContext, CellModel cell)
        {
            if (!PluginFunctionLoader.TryGetFunction(PluginFunctionLoader.TriggerFunctionsDirectoryName, cell.TriggerFunctionName, out var triggerFunction)) return new CompileResult { Success = false, Result = "Trigger function not found" };
            var method = triggerFunction.CompiledMethod;
            if (triggerFunction.CompileResult.Success)
            {
                method?.Invoke(null, [pluginContext, cell]);
                return new CompileResult { Success = true, Result = string.Empty };
            }
            return triggerFunction.CompileResult;
        }
    }
}
