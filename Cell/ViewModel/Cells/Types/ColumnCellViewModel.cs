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
            model.PropertyChanged += ModelPropertyChanged;
        }

        public override string Text { get => GetColumnName(Column); set => base.Text = value; }

        public override double Width
        {
            get => base.Width;
            set
            {
                if (value < 5) return;
                if (value > 500) return;
                if (base.Width == value) return;
                base.Width = value;
                _sheetViewModel.UpdateLayout();
            }
        }

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

        private void ModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellModel.Column)) NotifyPropertyChanged(nameof(Text));
        }
    }
}
