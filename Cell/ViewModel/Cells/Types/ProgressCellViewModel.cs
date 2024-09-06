using Cell.Model;

namespace Cell.ViewModel.Cells.Types
{
    public class ProgressCellViewModel : CellViewModel
    {
        public ProgressCellViewModel(CellModel model, SheetViewModel sheetViewModel) : base(model, sheetViewModel)
        {
            model.PropertyChanged += ModelPropertyChanged;
        }

        public bool IsVerticalOrientation
        {
            get => Model.GetBooleanProperty(nameof(IsVerticalOrientation));
            set
            {
                Model.SetBooleanProperty(nameof(IsVerticalOrientation), value);
                NotifyPropertyChanged(nameof(IsVerticalOrientation), nameof(ProgressBarWidth), nameof(ProgressBarHeight));
            }
        }

        public double ProgressBarHeight => !IsVerticalOrientation ? Height : Model.Value * (Height - Margin.Top - Margin.Bottom - 6);

        public double ProgressBarWidth => IsVerticalOrientation ? Width : Model.Value * (Width - Margin.Left - Margin.Right - 6);

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
