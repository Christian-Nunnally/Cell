using Cell.Persistence;

namespace Cell.ViewModel
{
    public class CellLayout
    {
        private readonly CellViewModel _corner;
        private readonly List<CellViewModel> _rows;
        private readonly List<CellViewModel> _columns;
        private readonly IEnumerable<CellViewModel> _cells;

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

        private void LayoutRowCells()
        {
            var lastCell = _corner;
            foreach (var rowCellViewModel in _rows.Skip(1))
            {
                rowCellViewModel.X = 0;
                rowCellViewModel.Y = lastCell.Y + lastCell.Height;
                lastCell = rowCellViewModel;
            }
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

        private void LayoutOtherCells()
        {
            foreach (var cell in _cells)
            {
                if (IsRowOrColumn(cell)) continue;
                if (!string.IsNullOrEmpty(cell.Model.MergedWith))
                {
                    LayoutMergedCell(cell);
                }
                else
                {
                    LayoutCell(cell);
                }
            }

            static bool IsRowOrColumn(CellViewModel cellModel) => cellModel.Row == 0 || cellModel.Column == 0;
        }

        private void LayoutMergedCell(CellViewModel cellModel)
        {
            var totalWidth = 0.0;
            var totalHeight = 0.0;
            if (cellModel.Model.MergedWith == cellModel.ID)
            {
                var currentRow = cellModel.Row;
                var currentColumn = cellModel.Column;
                var currentCell = Cells.GetCell(cellModel.Model.SheetName, currentRow, cellModel.Column);
                while (currentCell != null && currentCell.MergedWith == cellModel.ID)
                {
                    totalHeight += Cells.GetCell(cellModel.Model.SheetName, currentRow, 0)?.Height ?? 0;
                    currentCell = Cells.GetCell(cellModel.Model.SheetName, ++currentRow, cellModel.Column);
                }
                currentCell = Cells.GetCell(cellModel.Model.SheetName, cellModel.Row, currentColumn);
                while (currentCell != null && currentCell.MergedWith == cellModel.ID)
                {
                    totalWidth += Cells.GetCell(cellModel.Model.SheetName, 0, currentColumn)?.Width ?? 0;
                    currentCell = Cells.GetCell(cellModel.Model.SheetName, cellModel.Row, ++currentColumn);
                }
            }
            LayoutCell(cellModel, totalWidth, totalHeight);
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
    }
}
