using Cell.Data;
using Cell.Model;
using Cell.Persistence;

namespace Cell.ViewModel
{
    public class CornerCellViewModel(CellModel model, SheetViewModel sheetViewModel) : SpecialCellViewModel(model, sheetViewModel)
    {
        public List<string> SheetNames => Cells.GetSheetNames();

        public List<string> PopulateFunctionNames => PluginFunctionLoader.Namespaces.TryGetValue("Populate", out var result) ? result.Values.Select(x => x.Name).ToList() : [];

        public List<string> TriggerFunctionNames => PluginFunctionLoader.Namespaces.TryGetValue("Trigger", out var result) ? result.Values.Select(x => x.Name).ToList() : [];

        public List<string> CollectionNames => UserCollectionLoader.CollectionNames.ToList();

        public override string BackgroundColorHex { get => "#2d2d30"; set => base.BackgroundColorHex = value; }
    }
}