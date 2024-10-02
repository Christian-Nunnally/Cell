﻿using Cell.Common;
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
        /// <summary>
        /// The name of the variable that contains the cell reference in a plugin function (usually "cell").
        /// </summary>
        public const string CellReferenceVariableName = "cell";
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

        /// <summary>
        /// Creates the function call to get the reference from a <see cref="Context"/>.
        /// </summary>
        /// <returns>Text representation of a function call that gets the cell for this reference.</returns>
        public string CreateCodeForReference()
        {
            string cellLocationArguments = SheetArgument;
            cellLocationArguments += CreateRowColumnArgumentSyntax(IsRowRelative, Row, IsColumnRelative, Column);
            if (IsRange)
            {
                cellLocationArguments += CreateRowColumnArgumentSyntax(IsRowRelativeRangeEnd, RowRangeEnd, IsColumnRelativeRangeEnd, ColumnRangeEnd);
                return $"{Context.PluginContextArgumentName}.{nameof(Context.GetCellRange)}({cellLocationArguments})";
            }
            return $"{Context.PluginContextArgumentName}.{nameof(Context.GetCell)}({cellLocationArguments})";
        }

        /// <summary>
        /// Computes the cell or cells that the code generated by <see cref="CreateCodeForReference"/> would return when run on the given cell.
        /// </summary>
        /// <param name="cell">The cell to resolve the locations from.</param>
        /// <returns>A list of location strings representing the locations this reference resolves to for the given cell.</returns>
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
            return string.Join(',', ResolveLocations(cell));
        }

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

        private int ResolveColumn(CellModel cell) => IsColumnRelative ? Column + cell.Column : Column;

        private int ResolveColumnRangeEnd(CellModel cell) => IsColumnRelativeRangeEnd ? ColumnRangeEnd + cell.Column : ColumnRangeEnd;

        private int ResolveRow(CellModel cell) => IsRowRelative ? Row + cell.Row : Row;

        private int ResolveRowRangeEnd(CellModel cell) => IsRowRelativeRangeEnd ? RowRangeEnd + cell.Row : RowRangeEnd;
    }
}
