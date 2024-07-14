using Cell.Model;
using Cell.Persistence;
using Microsoft.CodeAnalysis;
using System.Reflection;

namespace Cell.Plugin
{
    internal static class DynamicCellPluginExecutor
    {
        public static CompileResult CompileAndRunPopulate(PluginContext pluginContext, CellModel cell)
        {
            if (!PluginFunctionLoader.TryGetPopulateFunction(cell.PopulateFunctionName, out var populateFunction)) return new CompileResult { Success = false, Result = "Populate function not found" };
            try
            {
                var method = CompileMethod(populateFunction.SyntaxTree, "Populate");
                var resultObject = method.Invoke(null, [pluginContext, cell]);
                return new CompileResult { Success = true, Result = resultObject?.ToString() ?? "" };
            }
            catch (Exception e)
            {
                return new CompileResult { Success = false, Result = e.Message };
            }
        }

        public static CompileResult CompileAndRunTrigger(PluginContext pluginContext, CellModel cell)
        {
            if (!PluginFunctionLoader.TryGetTriggerFunction(cell.TriggerFunctionName, out var triggerFunction)) return new CompileResult { Success = false, Result = "Trigger function not found" };
            try
            {
                var method = CompileMethod(triggerFunction.SyntaxTree, "Trigger");
                method.Invoke(null, [pluginContext, cell]);
                return new CompileResult { Success = true, Result = string.Empty };
            }
            catch (Exception e)
            {
                return new CompileResult { Success = false, Result = e.Message };
            }
        }

        private static MethodInfo CompileMethod(SyntaxTree syntax, string methodName)
        {
            var compiler = new RoslynCompiler("Plugin.Program", syntax, [typeof(Console)]);
            var compiled = compiler.Compile() ?? throw new Exception("Error during compile - compiled object is null");
            return compiled.GetMethod(methodName) ?? throw new Exception("Error during compile - compiled object is null");
        }
    }
}
