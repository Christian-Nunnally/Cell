using Cell.Model;
using System.Windows;
using System.Windows.Controls;

namespace Cell.ViewModel.Cells.Types
{
    public class TextboxCellViewModel(CellModel model, SheetViewModel sheetViewModel) : CellViewModel(model, sheetViewModel)
    {
        private static TextBox? _focusedTextBox;
        private TextBox? _textBox;
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
