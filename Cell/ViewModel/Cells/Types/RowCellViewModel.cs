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
            model.PropertyChanged += ModelPropertyChanged;
        }

        /// <summary>
        /// The height of the row.
        /// </summary>
        public override double Height
        {
            get => base.Height;
            set
            {
                const int MinHeight = 5;
                const int MaxHeight = 2000;
                value = Math.Min(MaxHeight, Math.Max(MinHeight, value));
                if (base.Height == value) return;
                base.Height = value;
                _sheetViewModel.UpdateLayout();
            }
        }

        /// <summary>
        /// Gets the text that should be displayed for the cell. For row cells this is the row number.
        /// </summary>
        public override string Text { get => Row.ToString(); set => base.Text = value; }

        private void ModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellModel.Row)) NotifyPropertyChanged(nameof(Text));
        }
    }
}
