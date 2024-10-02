using Cell.Model;
using System.Windows;
using System.Windows.Controls;

namespace Cell.ViewModel.Cells.Types
{
    public class TextboxCellViewModel : CellViewModel
    {
        private static TextBox? _focusedTextBox;
        // TODO: move to view.
        private TextBox? _textBox;
        /// <summary>
        /// Creates a new instance of <see cref="TextboxCellViewModel"/>.
        /// </summary>
        /// <param name="model">The underlying model for this cell.</param>
        /// <param name="sheet">The sheet this cell is visible on.</param>
        public TextboxCellViewModel(CellModel model, SheetViewModel sheet) : base(model, sheet)
        {
        }

        public override bool IsSelected
        {
            get => base.IsSelected;
            set
            {
                base.IsSelected = value;
                if (_textBox is null) return;
                if (value)
                {
                    if (_sheetViewModel.CellSelector.SelectedCells.Count < 2)
                    {
                        _textBox.Focus();
                        _textBox.SelectAll();
                        _focusedTextBox = _textBox;
                    }
                    else
                    {
                        if (_focusedTextBox != null)
                        {
                            _focusedTextBox.SelectionStart = _textBox.Text.Length; // Move caret to end
                            _focusedTextBox.SelectionLength = 0; // Deselect all text
                            Window.GetWindow(_focusedTextBox).Focus();
                            _focusedTextBox = null;
                        }
                    }
                }
            }
        }

        public void SetTextBox(TextBox textBox)
        {
            _textBox = textBox;
        }
    }
}
