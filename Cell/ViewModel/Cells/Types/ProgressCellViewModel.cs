using Cell.Model;

namespace Cell.ViewModel.Cells.Types
{
    public class ProgressCellViewModel : CellViewModel
    {
        public ProgressCellViewModel(CellModel model, SheetViewModel sheetViewModel) : base(model, sheetViewModel)
        {
            model.PropertyChanged += ModelPropertyChanged;
        }

        public double ProgressBarHeight => Height;

        public double ProgressBarWidth => ValueAsDouble * Width;

        public override string Text
        {
            get => base.Text;
            set
            {
                base.Text = value;
                NotifyPropertyChanged(nameof(ProgressBarWidth));
                NotifyPropertyChanged(nameof(ProgressBarHeight));
            }
        }

        public double ValueAsDouble => double.TryParse(Text, out var doubleValue) ? doubleValue : 0;

        private void ModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellModel.Text))
            {
                NotifyPropertyChanged(nameof(ProgressBarWidth));
                NotifyPropertyChanged(nameof(ProgressBarHeight));
            }
        }
    }
}
