using Cell.Core.Data.Tracker;
using Cell.ViewModel.Cells;
using Cell.ViewModel.Cells.Types;

namespace Cell.ViewModel
{
    /// <summary>
    /// Layouts the cells in a sheet.
    /// </summary>
    public class CellLayout
    {
        private readonly bool _canLayout = true;
        private readonly IEnumerable<CellViewModel> _cells;
        private readonly CellTracker _cellTracker;
        private readonly List<CellViewModel> _columns;
        private readonly CellViewModel? _corner;
        private readonly List<CellViewModel> _rows;
        /// <summary>
        /// Creates a new instance of <see cref="CellLayout"/>.
        /// </summary>
        /// <param name="cells">The cell view models to perform a layout on.</param>
        /// <param name="cellTracker">The cell tracker to get cell models from.</param>
        public CellLayout(IEnumerable<CellViewModel> cells, CellTracker cellTracker)
        {
            _cellTracker = cellTracker;
            _cells = cells;
            _corner = _cells.OfType<CornerCellViewModel>().FirstOrDefault();
            if (_corner == null) _canLayout = false;
            _rows = [.. _cells.OfType<RowCellViewModel>().OrderBy(x => x.Row)];
            _columns = [.. _cells.OfType<ColumnCellViewModel>().OrderBy(x => x.Column)];
            if (_canLayout)
            {
                _rows.Insert(0, _corner!);
                _columns.Insert(0, _corner!);
            }
        }

        /// <summary>
        /// Gets the total height of all cells after layout.
        /// </summary>
        public double LayoutHeight { get; private set; }

        /// <summary>
        /// Gets the total width of all cells after layout.
        /// </summary>
        public double LayoutWidth { get; private set; }

        /// <summary>
        /// Performs the layout of the cells.
        /// </summary>
        public void UpdateLayout()
        {
            if (!_canLayout) return;
            LayoutRowCells();
            LayoutColumnCells();
            LayoutOtherCells();
        }

        private double ComputeMergedCellsHeight(CellViewModel cell)
        {
            bool isHiddenByMerge = cell.Model.MergedWith != cell.ID;
            if (isHiddenByMerge) return 0;

            var result = 0.0;
            var currentRow = cell.Row;
            var currentCell = _cellTracker.GetCell(cell.Model.Location.SheetName, currentRow, cell.Column);
            while (currentCell?.MergedWith == cell.ID)
            {
                result += _cellTracker.GetCell(cell.Model.Location.SheetName, currentRow, 0)?.Height ?? 0;
                currentCell = _cellTracker.GetCell(cell.Model.Location.SheetName, ++currentRow, cell.Column);
            }
            return result;
        }

        private double ComputeMergedCellsWidth(CellViewModel cell)
        {
            bool isHiddenByMerge = cell.Model.MergedWith != cell.ID;
            if (isHiddenByMerge) return 0;

            var result = 0.0;
            var currentColumn = cell.Column;
            var currentCell = _cellTracker.GetCell(cell.Model.Location.SheetName, cell.Row, currentColumn);
            while (currentCell?.MergedWith == cell.ID)
            {
                result += _cellTracker.GetCell(cell.Model.Location.SheetName, 0, currentColumn)?.Width ?? 0;
                currentCell = _cellTracker.GetCell(cell.Model.Location.SheetName, cell.Row, ++currentColumn);
            }
            return result;
        }

        private void LayoutCell(CellViewModel cell)
        {
            if (_rows.Count <= cell.Row || _columns.Count <= cell.Column) return;
            var rowCell = _rows[cell.Row];
            var columnCell = _columns[cell.Column];
            cell.Width = columnCell?.Width ?? cell.Width;
            cell.Height = rowCell?.Height ?? cell.Height;
            cell.X = columnCell?.X ?? cell.X;
            cell.Y = rowCell?.Y ?? cell.Y;
        }

        private void LayoutCell(CellViewModel cell, double totalWidth, double totalHeight)
        {
            if (_rows.Count <= cell.Row || _columns.Count <= cell.Column) return;
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
            if (lastCell == null) return;
            foreach (var columnCellViewModel in _columns.Skip(1))
            {
                columnCellViewModel.X = lastCell.X + lastCell.Width;
                columnCellViewModel.Y = 0;
                columnCellViewModel.Height = lastCell.Height;
                lastCell = columnCellViewModel;
            }
            LayoutWidth = lastCell.X + lastCell.Width;
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
            if (lastCell == null) return;
            foreach (var cellViewModel in _rows.Skip(1))
            {
                RowCellViewModel rowCellViewModel = (RowCellViewModel)cellViewModel;
                rowCellViewModel.X = 0;
                rowCellViewModel.Y = lastCell.Y + lastCell.Height;
                rowCellViewModel.Width = lastCell.Width;
                lastCell = rowCellViewModel;
            }
            LayoutHeight = lastCell.Y + lastCell.Height;
        }
    }
}
