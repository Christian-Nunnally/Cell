using Cell.Model;

namespace Cell.ViewModel.Cells.Types
{
    /// <summary>
    /// A cell that displays a dropdown list of items.
    /// 
    /// The user can select an item and it will be displayed in the cell.
    /// </summary>
    public class DropdownCellViewModel : CollectionCellViewModel
    {
        private readonly CellModel _cellModel;

        /// <summary>
        /// Creates a new instance of <see cref="DropdownCellViewModel"/>.
        /// </summary>
        /// <param name="model">The underlying model for this cell.</param>
        /// <param name="sheet">The sheet this cell is visible on.</param>
        public DropdownCellViewModel(CellModel model, SheetViewModel sheet) : base(model, sheet)
        {
            _cellModel = model;
            Collection.Add(Text);
        }

        internal void DropdownOpened()
        {
            if (!string.IsNullOrEmpty(_cellModel.PopulateFunctionName)) _sheetViewModel.CellPopulateManager.RunPopulateForCell(_cellModel);
            NotifyPropertyChanged(nameof(SelectedItem));
        }
    }
}
