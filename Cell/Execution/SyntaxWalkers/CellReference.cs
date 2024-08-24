using Cell.Model;
using Cell.ViewModel.Execution;

namespace Cell.Execution.SyntaxWalkers
{
    public class CellReference
    {
        public int Column { get; set; }

        public int ColumnRangeEnd { get; set; }

        public bool IsColumnRelative { get; set; }

        public bool IsColumnRelativeRangeEnd { get; set; }

        public bool IsRange { get; set; }

        public bool IsRowRelative { get; set; }

        public bool IsRowRelativeRangeEnd { get; set; }

        public int Row { get; set; }

        public int RowRangeEnd { get; set; }

        public string SheetName { get; set; } = "";

        public int ResolveColumn(CellModel cell) => IsColumnRelative ? Column + cell.Column : Column;

        public int ResolveColumnRangeEnd(CellModel cell) => IsColumnRelativeRangeEnd ? ColumnRangeEnd + cell.Column : ColumnRangeEnd;

        public int ResolveRow(CellModel cell) => IsRowRelative ? Row + cell.Row : Row;

        public int ResolveRowRangeEnd(CellModel cell) => IsRowRelativeRangeEnd ? RowRangeEnd + cell.Row : RowRangeEnd;

        public string GetCodeForReference()
        {
            var sheetArgument = string.IsNullOrWhiteSpace(SheetName)
                ? "cell"
                : $"\"{SheetName}\"";
            var cellLocationArguments = IsRowRelative
                ? $", cell.Row + {Row}"
                : $", {Row}";
            cellLocationArguments += IsColumnRelative
                ? $", cell.Column + {Column}"
                : $", {Column}";

            if (IsRange)
            {
                cellLocationArguments += IsRowRelativeRangeEnd
                    ? $", cell.Row + {RowRangeEnd}"
                    : $", {RowRangeEnd}";

                cellLocationArguments += IsColumnRelativeRangeEnd
                    ? $", cell.Column + {ColumnRangeEnd}"
                    : $", {ColumnRangeEnd}";
            }
            return $"{PluginContext.PluginContextArgumentName}.{nameof(PluginContext.GetCell)}({sheetArgument}{cellLocationArguments})";
        }
    }
}
