using Cell.Model;

namespace Cell.ViewModel.Cells.Types
{
    /// <summary>
    /// Represents a cell that displays the column name and is in the top row of the sheet.
    /// </summary>
    public class ColumnCellViewModel : CellViewModel
    {
        /// <summary>
        /// Creates a new instance of <see cref="ColumnCellViewModel"/>.
        /// </summary>
        /// <param name="model">The underlying model for this cell.</param>
        /// <param name="sheet">The sheet this cell is visible on.</param>
        public ColumnCellViewModel(CellModel model, SheetViewModel sheet) : base(model, sheet)
        {
            NotifyPropertyChanged(nameof(Text));
            model.Location.PropertyChanged += ModelLocationPropertyChanged;
        }

        /// <summary>
        /// Gets the column name of the cell, like A for the first column, B for the second, etc.
        /// </summary>
        public override string Text { get => GetColumnName(Column); set => base.Text = value; }

        /// <summary>
        /// Gets the column name of the cell, like A for the first column, B for the second, etc.
        /// </summary>
        /// <param name="columnNumber">The number index of the column.</param>
        /// <returns>The user friendly name of the column.</returns>
        public static string GetColumnName(int columnNumber)
        {
            if (columnNumber < 1) return "=";
            string columnName = "";
            while (columnNumber > 0)
            {
                int modulo = (columnNumber - 1) % 26;
                columnName = Convert.ToChar('A' + modulo) + columnName;
                columnNumber = (columnNumber - modulo) / 26;
            }
            return columnName;
        }

        private void ModelLocationPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellLocationModel.Column)) NotifyPropertyChanged(nameof(Text));
        }
    }
}
