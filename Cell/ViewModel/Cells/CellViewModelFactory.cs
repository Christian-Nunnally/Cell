using Cell.Model;
using Cell.ViewModel.Cells.Types;

namespace Cell.ViewModel.Cells
{
    /// <summary>
    /// A factory for creating cell view models.
    /// </summary>
    public static class CellViewModelFactory
    {
        /// <summary>
        /// Creates a new cell view model based on the given model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="sheetViewModel">The sheet view model that will own the created cell view model.</param>
        /// <returns>The newly created cell view model.</returns>
        /// <exception cref="Exception">When no valid cell view model can be created for the given model.</exception>
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
                CellType.Date => new DateCellViewModel(model, sheetViewModel),
                _ => throw new Exception($"Unknown cell type '{model.CellType}'"),
            };
        }
    }
}
