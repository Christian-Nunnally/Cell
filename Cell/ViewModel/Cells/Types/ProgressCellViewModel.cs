using Cell.Model;
using System.ComponentModel;

namespace Cell.ViewModel.Cells.Types
{
    public class ProgressCellViewModel : CellViewModel
    {
        public ProgressCellViewModel(CellModel model, SheetViewModel sheetViewModel) : base(model, sheetViewModel)
        {
            model.PropertyChanged += ModelPropertyChanged;
            model.Style.PropertyChanged += ModelStylePropertyChanged;
        }

        private void ModelStylePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(IsVerticalOrientation));
        }

        public bool IsVerticalOrientation => Model.Style.HorizontalAlignment == System.Windows.HorizontalAlignment.Left;

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
