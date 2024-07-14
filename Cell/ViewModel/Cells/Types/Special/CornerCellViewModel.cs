using Cell.Model;

namespace Cell.ViewModel
{
    public class CornerCellViewModel : SpecialCellViewModel
    {
        public CornerCellViewModel(CellModel model, SheetViewModel sheetViewModel) : base(model, sheetViewModel)
        {
        }

        public override string BackgroundColorHex { get => "#2d2d30"; set => base.BackgroundColorHex = value; }
    }
}