using Cell.Model;
using System.Windows.Controls;

namespace Cell.ViewModel
{
    public class DateCellViewModel : CellViewModel
    {
        DatePicker picker = new DatePicker();

        public DateCellViewModel(CellModel model, SheetViewModel sheetViewModel) : base(model, sheetViewModel)
        {
            model.PropertyChanged += ModelPropertyChanged;
        }

        private void ModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellModel.Text))
            {
                NotifyPropertyChanged(nameof(SelectedDate));
            }
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
    }
}