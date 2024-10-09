using Cell.Model;
using System.ComponentModel;

namespace Cell.ViewModel.Cells.Types
{
    /// <summary>
    /// A cell view model for a cell that displays a date.
    /// </summary>
    public class DateCellViewModel : CellViewModel
    {
        /// <summary>
        /// Creates a new instance of <see cref="DateCellViewModel"/>.
        /// </summary>
        /// <param name="model">The underlying model for this cell.</param>
        /// <param name="sheet">The sheet this cell is visible on.</param>
        public DateCellViewModel(CellModel model, SheetViewModel sheet) : base(model, sheet)
        {
            model.PropertyChanged += ModelPropertyChanged;
        }

        /// <summary>
        /// Parses the text of the cell as a date for binding to a date picker.
        /// </summary>
        public DateTime SelectedDate
        {
            get => DateTime.TryParse(Text, out var date) ? date : DateTime.Now;
            set
            {
                Text = value.ToString("yyyy-MM-dd");
            }
        }

        private void ModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellModel.Text))
            {
                NotifyPropertyChanged(nameof(SelectedDate));
            }
        }
    }
}
