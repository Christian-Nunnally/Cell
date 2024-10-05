using Cell.Model;

namespace Cell.ViewModel.Cells.Types
{
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
            get => Model.GetCustomProperty(nameof(SelectedItem));
            set
            {
                Model.SetCustomProperty(nameof(SelectedItem), value?.ToString() ?? "");
                NotifyPropertyChanged(nameof(SelectedItem));
            }
        }
    }
}
