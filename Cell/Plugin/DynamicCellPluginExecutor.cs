using Cell.Model;

namespace Cell.Plugin
{
    internal static class DynamicCellPluginExecutor
    {
        public static string CompileAndRunTextProvider(PluginContext pluginContext, CellModel cell)
        {
            string code = @"
                using Cell.Model;
                using Cell.Plugin;                

                namespace Plugin
                {
                    public class Program
                    {
                        public static string GetText(PluginContext context, CellModel model)
                        {
                            return model.Height.ToString();
                        }
                    }
                }
            ";

            var compiler = new RoslynCompiler("Plugin.Program", code, [typeof(Console)]);
            var compiled = compiler.Compile();

            return compiled?.GetMethod("GetText")?.Invoke(null, [pluginContext, cell]) as string ?? "Error during compile";
        }

        public static void CompileAndRunOnEditProvider(PluginContext pluginContext, CellModel cell)
        {
            string code = @"
                using Cell.Model;
                using Cell.Plugin;                

                namespace Plugin
                {
                    public class Program
                    {
                        public static void OnEdit(PluginContext context, CellModel model)
                        {
                            model.Height.ToString();
                        }
                    }
                }
            ";

            var compiler = new RoslynCompiler("Plugin.Program", code, [typeof(Console)]);
            var compiled = compiler.Compile();

            compiled?.GetMethod("OnEdit")?.Invoke(null, [pluginContext, cell]);
        }
    }
}
