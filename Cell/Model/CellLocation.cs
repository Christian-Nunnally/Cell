
namespace Cell.Model
{
    public class CellLocation(string sheetName, int row, int column, bool isRowRelative, bool isColumnRelative)
    {
        public string SheetName { get; set; } = sheetName;

        public int Row { get; set; } = row;

        public int Column { get; set; } = column;

        public bool IsRowRelative { get; set; } = isRowRelative;

        public bool IsColumnRelative { get; set; } = isColumnRelative;

        public int ResolveRow(CellModel cell) => IsRowRelative ? Row + cell.Row : Row;

        public int ResolveColumn(CellModel cell) => IsColumnRelative ? Column + cell.Column : Column;
    }
}
