using Cell.Model;

namespace Cell.ViewModel
{
    public class CheckboxCellViewModel : CellViewModel
    {
        public CheckboxCellViewModel(CellModel model, SheetViewModel sheetViewModel) : base(model, sheetViewModel)
        {
        }

        public bool IsChecked { get; set; }
    }
}