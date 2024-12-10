using Cell.Core.Execution.Functions;
using Cell.Model;
using System.ComponentModel;

namespace Cell.ViewModel.Cells.Types
{
    /// <summary>
    /// Extensions for <see cref="CheckboxCellViewModel"/>.
    /// </summary>
    public static class CheckboxCellModelExtensions
    {
        /// <summary>
        /// Sets the custom IsChecked property to true for this cell.
        /// </summary>
        /// <param name="cell">The cell to set the property on.</param>
        public static void Check(this CellModel cell) => cell.Check(true);

        /// <summary>
        /// Sets the custom IsChecked property to the given value for this cell.
        /// </summary>
        /// <param name="cell">The cell to set the property on.</param>
        /// <param name="check">True or false</param>
        public static void Check(this CellModel cell, bool check)
        {
            cell.Text = check.ToString();
        }

        /// <summary>
        /// Gets whether the custom IsChecked property for this cell is true.
        /// </summary>
        /// <param name="cell">The cell to set the property on.</param>
        public static bool IsChecked(this CellModel cell)
        {
            if (bool.TryParse(cell.Text, out var isChecked)) return isChecked;
            return false;
        }

        /// <summary>
        /// Sets the custom IsChecked property to false for this cell.
        /// </summary>
        /// <param name="cell">The cell to set the property on.</param>
        public static void Uncheck(this CellModel cell) => cell.Check(false);
    }

    /// <summary>
    /// View model for a cell that contains a checkbox.
    /// </summary>
    public class CheckboxCellViewModel : CellViewModel
    {
        /// <summary>
        /// Creates a new instance of <see cref="CheckboxCellViewModel"/>.
        /// </summary>
        /// <param name="model">The underlying model for this cell.</param>
        /// <param name="sheet">The sheet this cell is visible on.</param>
        public CheckboxCellViewModel(CellModel model, SheetViewModel sheet) : base(model, sheet)
        {
            model.PropertyChanged += ModelPropertyChanged;
        }

        /// <summary>
        /// Gets or sets whether the checkbox is checked.
        /// </summary>
        public bool IsChecked
        {
            get => Model.IsChecked();
            set
            {
                var oldValue = IsChecked;
                if (oldValue == value) return;
                _sheetViewModel.CellTriggerManager.CellTriggered(Model, new EditContext(nameof(IsChecked), oldValue.ToString(), value.ToString()));
                Model.Check(value);
            }
        }

        private void ModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellModel.Text))
            {
                NotifyPropertyChanged(nameof(IsChecked));
            }
        }
    }
}
