using Cell.Model;
using System.ComponentModel;

namespace Cell.ViewModel.Cells.Types
{
    /// <summary>
    /// A view model for a cell that displays a progress bar.
    /// </summary>
    public class ProgressCellViewModel : CellViewModel
    {
        /// <summary>
        /// Creates a new instance of <see cref="ProgressCellViewModel"/>.
        /// </summary>
        /// <param name="model">The underlying model for this cell.</param>
        /// <param name="sheet">The sheet this cell is visible on.</param>
        public ProgressCellViewModel(CellModel model, SheetViewModel sheet) : base(model, sheet)
        {
            model.PropertyChanged += ModelPropertyChanged;
            model.Style.PropertyChanged += ModelStylePropertyChanged;
        }

        /// <summary>
        /// Gets whether the progress bar is oriented vertically.
        /// </summary>
        public bool IsVerticalOrientation => Model.Style.HorizontalAlignment == System.Windows.HorizontalAlignment.Left;

        /// <summary>
        /// Gets the height of the progress bar in the UI.
        /// </summary>
        public double ProgressBarHeight => !IsVerticalOrientation ? Height : Model.Value * (Height - Margin.Top - Margin.Bottom - 6);

        /// <summary>
        /// Gets the width of the progress bar in the UI.
        /// </summary>
        public double ProgressBarWidth => IsVerticalOrientation ? Width : Model.Value * (Width - Margin.Left - Margin.Right - 6);

        private void ModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellModel.Text))
            {
                NotifyPropertyChanged(nameof(ProgressBarWidth));
                NotifyPropertyChanged(nameof(ProgressBarHeight));
            }
        }

        private void ModelStylePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(IsVerticalOrientation));
        }
    }
}
