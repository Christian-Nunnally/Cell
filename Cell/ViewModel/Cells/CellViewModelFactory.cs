using Cell.Data;
using Cell.Model;

namespace Cell.ViewModel
{
    internal static class CellViewModelFactory
    {
        public static CellViewModel Create(CellModel model, SheetViewModel sheetViewModel)
        {
            return model.CellType switch
            {
                CellType.Label => new LabelCellViewModel(model, sheetViewModel),
                CellType.Checkbox => new CheckboxCellViewModel(model, sheetViewModel),
                CellType.Button => new ButtonCellViewModel(model, sheetViewModel),
                CellType.Textbox => new TextboxCellViewModel(model, sheetViewModel),
                CellType.Row => new RowCellViewModel(model, sheetViewModel),
                CellType.Column => new ColumnCellViewModel(model, sheetViewModel),
                CellType.Corner => new CornerCellViewModel(model, sheetViewModel),
                CellType.Dropdown => new DropdownCellViewModel(model, sheetViewModel),
                CellType.Progress => new ProgressCellViewModel(model, sheetViewModel),
                CellType.List => new ListCellViewModel(model, sheetViewModel),
                CellType.Graph => new GraphCellViewModel(model, sheetViewModel),
                _ => throw new System.Exception($"Unknown cell type '{model.CellType}'"),
            };
        }

        public static IEnumerable<CellViewModel> CreateCellViewModelsForSheet(SheetViewModel sheet)
        {
            return Cells.GetCellModelsForSheet(sheet.SheetName).Select(x => Create(x, sheet));
        }
    }
}
