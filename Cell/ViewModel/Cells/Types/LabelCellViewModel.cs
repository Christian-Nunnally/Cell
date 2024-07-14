using Cell.Model;

namespace Cell.ViewModel
{
    public class LabelCellViewModel(CellModel model, SheetViewModel sheetViewModel) : CellViewModel(model, sheetViewModel)
    {
        override public string Text
        {
            get => Value;
            set
            {
                Value = value;
                base.Text = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        override public string Value
        {
            get => base.Value;
            set
            {
                base.Value = value;
                OnPropertyChanged(nameof(Text));
            }
        }
    }
}