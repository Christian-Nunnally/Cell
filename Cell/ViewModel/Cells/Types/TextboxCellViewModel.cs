using Cell.Model;
using System.Windows.Controls;

namespace Cell.ViewModel
{
    public class TextboxCellViewModel(CellModel model, SheetViewModel sheetViewModel) : CellViewModel(model, sheetViewModel)
    {
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
                    if (_sheetViewModel.SelectedCellViewModels.Count < 2)
                    {
                        _textBox.Focus();
                        _textBox.SelectAll();
                    }
                }
            }
        }

        internal void SetTextBox(TextBox textBox)
        {
            _textBox = textBox;
        }
    }
}
