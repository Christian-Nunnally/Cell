using Cell.Data;
using Cell.Model;
using Cell.Persistence;
using System.Collections.ObjectModel;

namespace Cell.ViewModel
{
    public class CornerCellViewModel(CellModel model, SheetViewModel sheetViewModel) : SpecialCellViewModel(model, sheetViewModel)
    {
        public ObservableCollection<string> SheetNames => Cells.Instance.SheetNames;

        public List<string> PopulateFunctionNames => PluginFunctionLoader.Namespaces.TryGetValue("Populate", out var result) ? result.Values.Select(x => x.Model.Name).ToList() : [];

        public List<string> TriggerFunctionNames => PluginFunctionLoader.Namespaces.TryGetValue("Trigger", out var result) ? result.Values.Select(x => x.Model.Name).ToList() : [];

        public List<string> CollectionNames => UserCollectionLoader.CollectionNames.ToList();

        public override string BackgroundColorHex { get => "#2d2d30"; set => base.BackgroundColorHex = value; }
    }
}