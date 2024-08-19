using Cell.Model;
using System.Windows.Controls;

namespace Cell.ViewModel.Cells.Types
{
    public class DateCellViewModel : CellViewModel
    {
        internal DatePicker picker = new();
        public DateCellViewModel(CellModel model, SheetViewModel sheetViewModel) : base(model, sheetViewModel)
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
