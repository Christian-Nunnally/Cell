using Cell.Model;

namespace Cell.ViewModel
{
    public class CornerCellViewModel(CellModel model, SheetViewModel sheetViewModel) : SpecialCellViewModel(model, sheetViewModel)
    {
        public override string BackgroundColorHex { get => "#2d2d30"; set => base.BackgroundColorHex = value; }
    }
}