using Cell.ViewModel.Application;
using Cell.ViewModel.Cells;
using Cell.ViewModel.Cells.Types.Special;

namespace Cell.ViewModel
{
    public class CellLayout
    {
        private readonly IEnumerable<CellViewModel> _cells;
        private readonly List<CellViewModel> _columns;
        private readonly CellViewModel _corner;
        private readonly List<CellViewModel> _rows;
        public CellLayout(IEnumerable<CellViewModel> cells)
        {
            _cells = cells;
            _corner = _cells.OfType<CornerCellViewModel>().First();
            _rows = [.. _cells.OfType<RowCellViewModel>().OrderBy(x => x.Row)];
            _columns = [.. _cells.OfType<ColumnCellViewModel>().OrderBy(x => x.Column)];
            _rows.Insert(0, _corner);
            _columns.Insert(0, _corner);
        }

        public void UpdateLayout()
        {
            LayoutRowCells();
            LayoutColumnCells();
            LayoutOtherCells();
        }

        private static double ComputeMergedCellsHeight(CellViewModel cell)
        {
            bool isHiddenByMerge = cell.Model.MergedWith != cell.ID;
            if (isHiddenByMerge) return 0;

            var result = 0.0;
            var currentRow = cell.Row;
            var currentCell = ApplicationViewModel.Instance.CellTracker.GetCell(cell.Model.SheetName, currentRow, cell.Column);
            while (currentCell?.MergedWith == cell.ID)
            {
                result += ApplicationViewModel.Instance.CellTracker.GetCell(cell.Model.SheetName, currentRow, 0)?.Height ?? 0;
                currentCell = ApplicationViewModel.Instance.CellTracker.GetCell(cell.Model.SheetName, ++currentRow, cell.Column);
            }
            return result;
        }

        private static double ComputeMergedCellsWidth(CellViewModel cell)
        {
            bool isHiddenByMerge = cell.Model.MergedWith != cell.ID;
            if (isHiddenByMerge) return 0;

            var result = 0.0;
            var currentColumn = cell.Column;
            var currentCell = ApplicationViewModel.Instance.CellTracker.GetCell(cell.Model.SheetName, cell.Row, currentColumn);
            while (currentCell?.MergedWith == cell.ID)
            {
                result += ApplicationViewModel.Instance.CellTracker.GetCell(cell.Model.SheetName, 0, currentColumn)?.Width ?? 0;
                currentCell = ApplicationViewModel.Instance.CellTracker.GetCell(cell.Model.SheetName, cell.Row, ++currentColumn);
            }
            return result;
        }

        private void LayoutCell(CellViewModel cell)
        {
            var rowCell = _rows[cell.Row];
            var columnCell = _columns[cell.Column];
            cell.Width = columnCell?.Width ?? cell.Width;
            cell.Height = rowCell?.Height ?? cell.Height;
            cell.X = columnCell?.X ?? cell.X;
            cell.Y = rowCell?.Y ?? cell.Y;
        }

        private void LayoutCell(CellViewModel cell, double totalWidth, double totalHeight)
        {
            var rowCell = _rows[cell.Row];
            var columnCell = _columns[cell.Column];
            cell.Width = totalWidth;
            cell.Height = totalHeight;
            cell.X = columnCell?.X ?? cell.X;
            cell.Y = rowCell?.Y ?? cell.Y;
        }

        private void LayoutColumnCells()
        {
            var lastCell = _corner;
            foreach (var columnCellViewModel in _columns.Skip(1))
            {
                columnCellViewModel.X = lastCell.X + lastCell.Width;
                columnCellViewModel.Y = 0;
                lastCell = columnCellViewModel;
            }
        }

        private void LayoutMergedCell(CellViewModel cellModel)
        {
            var height = ComputeMergedCellsHeight(cellModel);
            var width = ComputeMergedCellsWidth(cellModel);
            LayoutCell(cellModel, width, height);
        }

        private void LayoutOtherCells()
        {
            foreach (var cell in _cells)
            {
                if (IsRowOrColumn(cell)) continue;
                if (IsMergedCell(cell)) LayoutMergedCell(cell);
                else LayoutCell(cell);
            }

            static bool IsRowOrColumn(CellViewModel cell) => cell.Row == 0 || cell.Column == 0;
            static bool IsMergedCell(CellViewModel cell) => !string.IsNullOrEmpty(cell.Model.MergedWith);
        }

        private void LayoutRowCells()
        {
            var lastCell = _corner;
            foreach (var cellViewModel in _rows.Skip(1))
            {
                RowCellViewModel rowCellViewModel = (RowCellViewModel)cellViewModel;
                rowCellViewModel.X = 0;
                rowCellViewModel.Y = lastCell.Y + lastCell.Height;
                lastCell = rowCellViewModel;
            }
        }
    }
}
