using Cell.Common;
using Cell.Core.Execution.References;
using Cell.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace Cell.Execution.References
{
    /// <summary>
    /// A class that represents a reference to a location or range represented by two locations.
    /// 
    /// A location is a sheet, row, and column.
    /// 
    /// The locations can be relative or absolute.
    /// </summary>
    public class LocationReference : IReferenceFromCell
    {
        public const string CellReferenceVariableName = "cell";
        public const string GetCellFunctionName = nameof(PluginContext.GetCell);
        public int Column { get; set; }

        public int ColumnRangeEnd { get; set; }

        public bool IsColumnRelative { get; set; }

        public bool IsColumnRelativeRangeEnd { get; set; }

        public bool IsRange { get; set; }

        public bool IsRowRelative { get; set; }

        public bool IsRowRelativeRangeEnd { get; set; }

        public bool IsSheetRelative { get; set; }

        public int Row { get; set; }

        public int RowRangeEnd { get; set; }

        public string SheetName { get; set; } = "";

        private string SheetArgument => IsSheetRelative ? CellReferenceVariableName : $"\"{SheetName}\"";

        public static bool TryCreateReferenceFromCode(SyntaxNode? node, out LocationReference cellReference)
        {
            cellReference = new LocationReference();
            if (!DoesNodeMatchCellReferenceSyntax(node, out var arguments)) return false;
            var sheetName = arguments[0].ToString();
            if (sheetName != CellReferenceVariableName && sheetName.StartsWith('"') && sheetName.EndsWith('"')) sheetName = sheetName[1..^1];
            if (sheetName == CellReferenceVariableName) cellReference.IsSheetRelative = true;
            else cellReference.SheetName = sheetName;

            if (!TryParsePositionFromArgument(arguments[1], "Row", out var position, out var isRelative)) return false;
            cellReference.Row = position;
            cellReference.IsRowRelative = isRelative;

            if (!TryParsePositionFromArgument(arguments[2], "Column", out position, out isRelative)) return false;
            cellReference.Column = position;
            cellReference.IsColumnRelative = isRelative;

            if (arguments.Count == 5)
            {
                cellReference.IsRange = true;
                if (!TryParsePositionFromArgument(arguments[3], "Row", out position, out isRelative)) return false;
                cellReference.RowRangeEnd = position;
                cellReference.IsRowRelativeRangeEnd = isRelative;

                if (!TryParsePositionFromArgument(arguments[4], "Column", out position, out isRelative)) return false;
                cellReference.ColumnRangeEnd = position;
                cellReference.IsColumnRelativeRangeEnd = isRelative;
            }
            return true;
        }

        public string CreateCodeForReference()
        {
            string cellLocationArguments = SheetArgument;
            cellLocationArguments += CreateRowColumnArgumentSyntax(IsRowRelative, Row, IsColumnRelative, Column);
            if (IsRange) cellLocationArguments += CreateRowColumnArgumentSyntax(IsRowRelativeRangeEnd, RowRangeEnd, IsColumnRelativeRangeEnd, ColumnRangeEnd);
            return $"{PluginContext.PluginContextArgumentName}.{GetCellFunctionName}({cellLocationArguments})";
        }

        public int ResolveColumn(CellModel cell) => IsColumnRelative ? Column + cell.Column : Column;

        public int ResolveColumnRangeEnd(CellModel cell) => IsColumnRelativeRangeEnd ? ColumnRangeEnd + cell.Column : ColumnRangeEnd;

        public IEnumerable<string> ResolveLocations(CellModel cell)
        {
            var sheetName = string.IsNullOrWhiteSpace(SheetName) ? cell.SheetName : SheetName;
            var row = ResolveRow(cell);
            var column = ResolveColumn(cell);
            var locations = new List<string>();
            if (IsRange)
            {
                var rowRangeEnd = ResolveRowRangeEnd(cell);
                var columnRangeEnd = ResolveColumnRangeEnd(cell);
                for (var r = row; r <= rowRangeEnd; r++)
                {
                    for (var c = column; c <= columnRangeEnd; c++)
                    {
                        locations.Add(Utilities.GetUnqiueLocationString(sheetName, r, c));
                    }
                }
            }
            else locations.Add(Utilities.GetUnqiueLocationString(sheetName, row, column));
            return locations;
        }

        public int ResolveRow(CellModel cell) => IsRowRelative ? Row + cell.Row : Row;

        public int ResolveRowRangeEnd(CellModel cell) => IsRowRelativeRangeEnd ? RowRangeEnd + cell.Row : RowRangeEnd;

        private static string CreateArgumentSyntax(bool isRelative, string rowOrColumn, int row)
        {
            return isRelative
                ? $", {CellReferenceVariableName}.{rowOrColumn} + {row}"
                : $", {row}";
        }

        private static string CreateRowColumnArgumentSyntax(bool isRowRelative, int row, bool isColumnRelative, int column)
        {
            var rowArgument = CreateArgumentSyntax(isRowRelative, "Row", row);
            var columnArgument = CreateArgumentSyntax(isColumnRelative, "Column", column);
            return $"{rowArgument}{columnArgument}";
        }

        private static bool DoesNodeMatchCellReferenceSyntax(SyntaxNode? node, [MaybeNullWhen(false)] out SeparatedSyntaxList<ArgumentSyntax> arguments)
        {
            arguments = SyntaxFactory.SeparatedList<ArgumentSyntax>();
            if (node is not InvocationExpressionSyntax syntax) return false;
            if (syntax.Expression is not MemberAccessExpressionSyntax memberAccessExpressionSyntax) return false;
            if (memberAccessExpressionSyntax.Name.Identifier.Text != GetCellFunctionName) return false;
            if (memberAccessExpressionSyntax.Expression is not IdentifierNameSyntax identifierName) return false;
            if (identifierName.Identifier.Text != PluginContext.PluginContextArgumentName) return false;
            if (!(syntax.ArgumentList.Arguments.Count == 3 || syntax.ArgumentList.Arguments.Count == 5)) return false;
            arguments = syntax.ArgumentList.Arguments;
            return true;
        }

        private static bool TryParsePositionFromArgument(ArgumentSyntax argumentSyntax, string rowOrColumn, out int position, out bool isRelative)
        {
            isRelative = false;
            position = 0;
            var row = argumentSyntax.ToString();
            if (argumentSyntax.Expression is BinaryExpressionSyntax binaryExpression && binaryExpression.Left is MemberAccessExpressionSyntax memberAccessExpression)
            {
                var cellAccessString = memberAccessExpression.ToString();
                if (cellAccessString == $"{CellReferenceVariableName}.{rowOrColumn}")
                {
                    row = binaryExpression.Right.ToString();
                    isRelative = true;
                }
            }
            if (!int.TryParse(row, out var rowValue)) return false;
            position = rowValue;
            return true;
        }

        public string ResolveUserFriendlyNameForCell(CellModel cell)
        {
            return string.Join(',', ResolveLocations(cell));
        }

        public string ResolveUserFriendlyCellAgnosticName()
        {
            return CreateCodeForReference();
        }
    }
}
