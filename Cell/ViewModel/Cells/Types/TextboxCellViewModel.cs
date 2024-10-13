using Cell.Execution;
using Cell.Model;
using System.ComponentModel;

namespace Cell.ViewModel.Cells.Types
{
    /// <summary>
    /// A cell view model that displays a textbox.
    /// </summary>
    public class TextboxCellViewModel : CellViewModel
    {

        /// <summary>
        /// Creates a new instance of <see cref="TextboxCellViewModel"/>.
        /// </summary>
        /// <param name="model">The underlying model for this cell.</param>
        /// <param name="sheet">The sheet this cell is visible on.</param>
        public TextboxCellViewModel(CellModel model, SheetViewModel sheet) : base(model, sheet)
        {
            Model.PropertyChanged += ModelPropertyChanged;
        }

        private void ModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Text))
            {
                NotifyPropertyChanged(nameof(TextboxText));
            }
        }

        /// <summary>
        /// Gets the number of selected cells for the view so the textbox knows whether to focus or not.
        /// </summary>
        /// <returns>The number of cells selected by the owning sheets selector.</returns>
        public int GetNumberOfSelectedCells() => _sheetViewModel.CellSelector.SelectedCells.Count;

        /// <summary>
        /// Gets or sets the text in the user editable text box.
        /// </summary>
        public string TextboxText
        {
            get => Text;
            set
            {
                var oldValue = Text;
                if (oldValue == value) return;
                _sheetViewModel.CellTriggerManager.CellTriggered(Model, new EditContext(nameof(Text), oldValue, value));
                Text = value;
            }
        }
    }
}
