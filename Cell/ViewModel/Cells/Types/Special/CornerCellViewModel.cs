using Cell.Model;
using Cell.View.Skin;
using System.Windows.Media;

namespace Cell.ViewModel.Cells.Types.Special
{
    public class CornerCellViewModel(CellModel model, SheetViewModel sheetViewModel) : SpecialCellViewModel(model, sheetViewModel)
    {
        public override Brush BackgroundColor => new SolidColorBrush(ColorConstants.ToolWindowHeaderColorConstant);

        public override SolidColorBrush BorderColor => new(ColorConstants.ToolWindowHeaderColorConstant);
    }
}
