
namespace Cell.Plugin
{
    public abstract class CellTextProvider
    {
        public abstract string GetText(PluginContext context, Model.CellModel cellModel);
    }
}
