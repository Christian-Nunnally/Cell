using Cell.Model;

namespace Cell.ViewModel
{
    public class ColumnCellViewModel : CellViewModel
    {
        public ColumnCellViewModel(CellModel model, SheetViewModel sheetViewModel) : base(model, sheetViewModel)
        {
        }

        public override double Width
        {
            get => base.Width;
            set
            {
                if (value < 5) return;
                if (value > 500) return;
                if (base.Width == value) return;
                var oldWidth = base.Width;
                base.Width = value;
                sheetViewModel.ResizeColumn(this, oldWidth, value);
            }
        }

        public static CellModel CreateColumnCellModel(double x, double y, double width, double height, string sheet, string text = "")
        {
            var model = CreateCellModel(x, y, width, height, sheet, text);
            model.CellType = "Column";
            return model;
        }
    }
}