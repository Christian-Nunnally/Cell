using Cell.Model;

namespace Cell.ViewModel.Cells.Types
{
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

        public DateTime SelectedDate
        {
            get => DateTime.TryParse(Text, out var date) ? date : DateTime.Now;
            set
            {
                Text = value.ToString("yyyy-MM-dd");
                NotifyPropertyChanged(nameof(SelectedDate));
            }
        }

        private void ModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellModel.Text))
            {
                NotifyPropertyChanged(nameof(SelectedDate));
            }
        }
    }
}
