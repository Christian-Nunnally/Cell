using Cell.Model;

namespace Cell.ViewModel.Cells.Types
{
    /// <summary>
    /// Represents a cell that displays its row number and lines the left side of a sheet.
    /// </summary>
    public class RowCellViewModel : CellViewModel
    {
        /// <summary>
        /// Creates a new instance of <see cref="RowCellViewModel"/>.
        /// </summary>
        /// <param name="model">The underlying model for this cell.</param>
        /// <param name="sheet">The sheet this cell is visible on.</param>
        public RowCellViewModel(CellModel model, SheetViewModel sheet) : base(model, sheet)
        {
            NotifyPropertyChanged(nameof(Text));
            model.Location.PropertyChanged += ModelPropertyChanged;
        }

        /// <summary>
        /// Gets the text that should be displayed for the cell. For row cells this is the row number.
        /// </summary>
        public override string Text { get => Row.ToString(); set => base.Text = value; }

        private void ModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellLocationModel.Row)) NotifyPropertyChanged(nameof(Text));
        }
    }
}
