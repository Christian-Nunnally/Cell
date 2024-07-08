using Cell.Model;

namespace Cell.ViewModel
{
    public class RowCellViewModel : CellViewModel
    {
        public RowCellViewModel(CellModel model, SheetViewModel sheetViewModel) : base(model, sheetViewModel)
        {
        }

        public override double Height
        {
            get => base.Height;
            set
            {
                if (value < 5) return;
                if (value > 500) return;
                if (base.Height == value) return;
                var oldHeight = base.Height;
                base.Height = value;
                sheetViewModel.ResizeRow(this, oldHeight, value);
            }
        }

        public static CellModel CreateRowCellModel(double x, double y, double width, double height, string sheet, string text = "")
        {
            var model = CreateCellModel(x, y, width, height, sheet, text);
            model.CellType = "Row";
            return model;
        }
    }
}