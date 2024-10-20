using Cell.Core.Execution.Functions;
using Cell.Core.Execution.References;
using Cell.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace Cell.Core.Execution.References
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
        /// <summary>
        /// The name of the variable that contains the cell reference in a plugin function (usually "cell").
        /// </summary>
        public const string CellReferenceVariableName = "cell";

        /// <summary>
        /// The column of the reference, or starting column of a range.
        /// </summary>
        public CellRelativeValue<int> ColumnReference { get; set; } = new(0);

        /// <summary>
        /// The row of the reference, or starting row of a range.
        /// </summary>
        public CellRelativeValue<int> RowReference { get; set; } = new(0);

        /// <summary>
        /// The end column of a range location reference.
        /// </summary>
        public CellRelativeValue<int> ColumnRangeEndReference { get; set; } = new(0);

        /// <summary>
        /// The end row of a range location reference.
        /// </summary>
        public CellRelativeValue<int> RowRangeEndReference { get; set; } = new(0);

        /// <summary>
        /// The sheet reference of the location.
        /// </summary>
        public CellRelativeValue<string> SheetReference { get; set; } = new("");

        /// <summary>
        /// Gets or sets whether this reference is a range and should have a range end set.
        /// </summary>
        public bool IsRange { get; set; }

        private string SheetArgument => SheetReference.IsRelative ? CellReferenceVariableName : $"\"{SheetReference.Value}\"";

        /// <summary>
        /// Attempts to convert a <see cref="SyntaxNode"/> into a <see cref="LocationReference"/>.
        /// 
        /// If the syntax node has the correct structure, the reference is created and the method returns true. The structure should match what is returned by <see cref="CreateCodeForReference"/>.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="cellReference"></param>
        /// <returns></returns>
        public static bool TryCreateReferenceFromCode(SyntaxNode? node, out LocationReference cellReference)
        {
            cellReference = new LocationReference();
            if (!DoesNodeMatchCellReferenceSyntax(node, out var arguments)) return false;
            var sheetName = arguments[0].ToString();
            if (sheetName != CellReferenceVariableName && sheetName.StartsWith('"') && sheetName.EndsWith('"')) sheetName = sheetName[1..^1];
            if (sheetName == CellReferenceVariableName) cellReference.SheetReference.IsRelative = true;
            else cellReference.SheetReference.Value = sheetName;

            if (!TryParsePositionFromArgument(arguments[1], "Row", out var position, out var isRelative)) return false;
            cellReference.RowReference.Value = position;
            cellReference.RowReference.IsRelative = isRelative;

            if (!TryParsePositionFromArgument(arguments[2], "Column", out position, out isRelative)) return false;
            cellReference.ColumnReference.Value = position;
            cellReference.ColumnReference.IsRelative = isRelative;

            if (arguments.Count == 5)
            {
                cellReference.IsRange = true;
                if (!TryParsePositionFromArgument(arguments[3], "Row", out position, out isRelative)) return false;
                cellReference.RowRangeEndReference.Value = position;
                cellReference.RowRangeEndReference.IsRelative = isRelative;

                if (!TryParsePositionFromArgument(arguments[4], "Column", out position, out isRelative)) return false;
                cellReference.ColumnRangeEndReference.Value = position;
                cellReference.ColumnRangeEndReference.IsRelative = isRelative;
            }
            return true;
        }

        /// <summary>
        /// Creates the function call to get the reference from a <see cref="Context"/>.
        /// </summary>
        /// <returns>Text representation of a function call that gets the cell for this reference.</returns>
        public string CreateCodeForReference()
        {
            string cellLocationArguments = SheetArgument;
            cellLocationArguments += CreateRowColumnArgumentSyntax(RowReference.IsRelative, RowReference.Value, ColumnReference.IsRelative, ColumnReference.Value);
            if (IsRange)
            {
                cellLocationArguments += CreateRowColumnArgumentSyntax(RowRangeEndReference.IsRelative, RowRangeEndReference.Value, ColumnRangeEndReference.IsRelative, ColumnRangeEndReference.Value);
                return $"{Context.PluginContextArgumentName}.{nameof(Context.GetCellRange)}({cellLocationArguments})";
            }
            return $"{Context.PluginContextArgumentName}.{nameof(Context.GetCell)}({cellLocationArguments})";
        }

        /// <summary>
        /// Computes the cell or cells that the code generated by <see cref="CreateCodeForReference"/> would return when run on the given cell location.
        /// </summary>
        /// <param name="location">The location to resolve the relative locations from.</param>
        /// <returns>A list of location strings representing the locations this reference resolves to for the given cell.</returns>
        public IEnumerable<string> ResolveLocations(CellLocationModel location)
        {
            var sheetName = string.IsNullOrWhiteSpace(SheetReference.Value) ? location.SheetName : SheetReference.Value;
            var row = ResolveRow(location);
            var column = ResolveColumn(location);
            var locations = new List<string>();
            if (IsRange)
            {
                var rowRangeEnd = ResolveRowRangeEnd(location);
                var columnRangeEnd = ResolveColumnRangeEnd(location);
                for (var r = row; r <= rowRangeEnd; r++)
                {
                    for (var c = column; c <= columnRangeEnd; c++)
                    {
                        locations.Add(new CellLocationModel(sheetName, r, c).LocationString);
                    }
                }
            }
            else locations.Add(new CellLocationModel(sheetName, row, column).LocationString);
            return locations;
        }

        /// <summary>
        /// Creates a string representation of the reference that is agnostic to the cell.
        /// </summary>
        /// <returns>A user friendly representation of this reference, without being able to return an exact location because it depends on which cell is resovling this reference.</returns>
        public string ResolveUserFriendlyCellAgnosticName()
        {
            return CreateCodeForReference();
        }

        /// <summary>
        /// Creates a string representation of the reference for a specific cell.
        /// </summary>
        /// <param name="cell">The cell to compute the exact location of the reference from.</param>
        /// <returns>A user friendly representation of the cell.</returns>
        public string ResolveUserFriendlyNameForCell(CellModel cell)
        {
            return string.Join(',', ResolveLocations(cell.Location));
        }

        private static string CreateArgumentSyntax(bool isRelative, string rowOrColumn, int row)
        {
            return isRelative
                ? $", {CellReferenceVariableName}.Location.{rowOrColumn} + {row}"
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
            if (memberAccessExpressionSyntax.Expression is not IdentifierNameSyntax identifierName) return false;
            if (identifierName.Identifier.Text != Context.PluginContextArgumentName) return false;
            if (memberAccessExpressionSyntax.Name.Identifier.Text == nameof(Context.GetCell))
            {
                if (syntax.ArgumentList.Arguments.Count != 3) return false;
                arguments = syntax.ArgumentList.Arguments;
                return true;
            }
            else if (memberAccessExpressionSyntax.Name.Identifier.Text == nameof(Context.GetCellRange))
            {
                if (syntax.ArgumentList.Arguments.Count != 5) return false;
                arguments = syntax.ArgumentList.Arguments;
                return true;
            }
            return false;
        }

        private static bool TryParsePositionFromArgument(ArgumentSyntax argumentSyntax, string rowOrColumn, out int position, out bool isRelative)
        {
            isRelative = false;
            position = 0;
            var row = argumentSyntax.ToString();
            if (argumentSyntax.Expression is BinaryExpressionSyntax binaryExpression && binaryExpression.Left is MemberAccessExpressionSyntax memberAccessExpression)
            {
                var cellAccessString = memberAccessExpression.ToString();
                if (cellAccessString == $"{CellReferenceVariableName}.{nameof(CellModel.Location)}.{rowOrColumn}")
                {
                    row = binaryExpression.Right.ToString();
                    isRelative = true;
                }
            }
            if (!int.TryParse(row, out var rowValue)) return false;
            position = rowValue;
            return true;
        }

        private int ResolveColumn(CellLocationModel location) => ColumnReference.IsRelative ? ColumnReference.Value + location.Column : ColumnReference.Value;

        private int ResolveColumnRangeEnd(CellLocationModel location) => ColumnRangeEndReference.IsRelative ? ColumnRangeEndReference.Value + location.Column : ColumnRangeEndReference.Value;

        private int ResolveRow(CellLocationModel location) => RowReference.IsRelative ? RowReference.Value + location.Row : RowReference.Value;

        private int ResolveRowRangeEnd(CellLocationModel location) => RowRangeEndReference.IsRelative ? RowRangeEndReference.Value + location.Row : RowRangeEndReference.Value;
    }
}
