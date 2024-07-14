using Cell.Model;

namespace Cell.ViewModel
{
    public class ProgressCellViewModel : CellViewModel
    {
        public ProgressCellViewModel(CellModel model, SheetViewModel sheetViewModel) : base(model, sheetViewModel)
        {
            model.PropertyChanged += ModelPropertyChanged;
        }

        private void ModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellModel.Text))
            {
                OnPropertyChanged(nameof(ProgressBarWidth));
                OnPropertyChanged(nameof(ProgressBarHeight));
            }
        }

        public double ValueAsDouble => double.TryParse(Text, out var doubleValue) ? doubleValue : 0;

        public double ProgressBarWidth => ValueAsDouble * Width;
        public double ProgressBarHeight => Height;

        public override string Text 
        { 
            get => base.Text; 
            set 
            { 
                base.Text = value; 
                OnPropertyChanged(nameof(ProgressBarWidth));
                OnPropertyChanged(nameof(ProgressBarHeight)); 
            }
        }
    }
}