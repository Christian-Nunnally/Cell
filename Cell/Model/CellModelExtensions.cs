using Cell.Data;

namespace Cell.Model
{
    internal static class CellModelExtensions
    {
        public static int CellsMergedToRight(this CellModel model)
        {
            var count = 0;
            while (!string.IsNullOrWhiteSpace(model.MergedWith) && Cells.GetCell(model.SheetName, model.Row, model.Column + 1 + count)?.MergedWith == model.MergedWith) count++;
            return count;
        }

        public static int CellsMergedBelow(this CellModel model)
        {
            var count = 0;
            while (!string.IsNullOrWhiteSpace(model.MergedWith) && Cells.GetCell(model.SheetName, model.Row + 1 + count, model.Column)?.MergedWith == model.MergedWith) count++;
            return count;
        }
    }
}
