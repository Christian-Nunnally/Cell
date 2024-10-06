using Cell.Model;

namespace Cell.ViewModel.Cells.Types
{
    /// <summary>
    /// A view model for a cell that displays a list of items.
    /// </summary>
    public class ListCellViewModel : CollectionCellViewModel
    {
        /// <summary>
        /// Creates a new instance of <see cref="ListCellViewModel"/>.
        /// </summary>
        /// <param name="model">The underlying model for this cell.</param>
        /// <param name="sheet">The sheet this cell is visible on.</param>
        public ListCellViewModel(CellModel model, SheetViewModel sheet) : base(model, sheet)
        {
        }

        public object? SelectedItem
        {
            get => Model.Properties[nameof(SelectedItem)];
            set
            {
                Model.Properties[nameof(SelectedItem)] = value?.ToString() ?? "";
                NotifyPropertyChanged(nameof(SelectedItem));
            }
        }
    }
}
