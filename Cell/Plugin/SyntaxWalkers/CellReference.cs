
using Cell.Model;

namespace Cell.Plugin.SyntaxWalkers
{
    public class CellReference
    {
        public string SheetName { get; set; } = "";

        public int Row { get; set; }

        public int Column { get; set; }

        public bool IsRowRelative { get; set; }

        public bool IsColumnRelative { get; set; }

        public bool IsRange { get; set; }

        public int RowRangeEnd { get; set; }

        public int ColumnRangeEnd { get; set; }

        public bool IsRowRelativeRangeEnd { get; set; }

        public bool IsColumnRelativeRangeEnd { get; set; }

        public int ResolveRow(CellModel cell) => IsRowRelative ? Row + cell.Row : Row;

        public int ResolveColumn(CellModel cell) => IsColumnRelative ? Column + cell.Column : Column;

        public int ResolveRowRangeEnd(CellModel cell) => IsRowRelativeRangeEnd ? RowRangeEnd + cell.Row : RowRangeEnd;

        public int ResolveColumnRangeEnd(CellModel cell) => IsColumnRelativeRangeEnd ? ColumnRangeEnd + cell.Column : ColumnRangeEnd;
    }
}
