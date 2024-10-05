using Cell.Execution.SyntaxWalkers;
using Cell.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;

namespace Cell.Plugin.SyntaxWalkers
{
    public partial class CellReferenceToCodeSyntaxRewriter(CellLocationModel location) : CSharpSyntaxRewriter
    {
        private readonly CellLocationModel _location = location;
        public static bool IsCellLocation(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && IsCellLocationString().IsMatch(input);
        }

        [GeneratedRegex(@"^[A-Z]+[0-9]+$")]
        public static partial Regex IsCellLocationString();

        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            node = base.Visit(node);

            if (node is IdentifierNameSyntax identifierSyntax)
            {
                var variableName = identifierSyntax.Identifier.ToString();
                var cellReference = variableName.Split('_');

                // Possible formats:
                // "SheetName_C4"
                // "SheetName_B_C4"
                // "SheetName_C_C4"
                // "SheetName_R_C4"
                // "SheetName_R_C4_Range_C5"
                // "SheetName_R_C4_Range_B_C5"
                // "SheetName_R_C4_Range_C_C5"
                // "SheetName_R_C4_Range_R_C5"
                // "R_C4_Range_R_C5"

                var sheetName = "cell";
                var hasSheetReference = cellReference.Length > 1 && !IsRelativitySymbol(cellReference[0]) && !IsCellLocation(cellReference[0]);
                if (hasSheetReference)
                {
                    sheetName = $"\"{cellReference[0]}\"";
                    cellReference = cellReference[1..];
                }

                var rangeIndex = cellReference.ToList().IndexOf("Range");
                var isRangeReference = rangeIndex >= 0;
                var rangeArguments = "";
                if (isRangeReference)
                {
                    var endOfRangePartOfCellReference = cellReference[(rangeIndex + 1)..];
                    rangeArguments = GetArgumentStringFromCellReference(endOfRangePartOfCellReference);
                    if (string.IsNullOrEmpty(rangeArguments)) return node;
                    cellReference = cellReference[..rangeIndex];
                }

                string cellLocationArguments = GetArgumentStringFromCellReference(cellReference);
                if (string.IsNullOrEmpty(cellLocationArguments)) return node;
                var code = $"c.GetCell({sheetName}{cellLocationArguments}{rangeArguments})";
                return SyntaxUtilities.CreateSyntaxNodePreservingTrivia(node, code);
            }
            return node;
        }

        private static int ColumnToIndex(string column)
        {
            column = column.ToUpper();
            int index = 0;
            foreach (char c in column)
            {
                index = index * 26 + (c - 'A' + 1);
            }
            return index;
        }

        private static (int Row, int Column) GetCellLocationFromVariable(string variableName)
        {
            string columnPart = GetColumnFromCellLocationString().Match(variableName).Value;
            string rowPart = GetRowFromCellLocationString().Match(variableName).Value;
            return (int.Parse(rowPart), ColumnToIndex(columnPart));
        }

        [GeneratedRegex(@"^[A-Z]+")]
        private static partial Regex GetColumnFromCellLocationString();

        private static (string RelativitySymbol, string CellReferenceName) GetRelativitySymbolAndCellLocationNameFromCellReference(string[] cellReference)
        {
            var relativitySymbol = "";
            var cellLocationName = cellReference[0];
            if (cellReference.Length == 2)
            {
                relativitySymbol = cellReference[0];
                cellLocationName = cellReference[1];
            }
            return (relativitySymbol, cellLocationName);
        }

        [GeneratedRegex(@"\d+$")]
        private static partial Regex GetRowFromCellLocationString();

        private static bool IsRelativitySymbol(string symbol) => symbol == "R" || symbol == "C" || symbol == "B";

        private string CalculateArgumentsFromLocation(string relativitySymbol, int rowOffset, int columnOffset)
        {
            return relativitySymbol switch
            {
                "" => $", cell.Row + {rowOffset - _location.Row}, cell.Column + {columnOffset - _location.Column}",
                "C" => $", cell.Row + {rowOffset - _location.Row}, {columnOffset}",
                "R" => $", {rowOffset}, cell.Column + {columnOffset - _location.Column}",
                "B" => $", {rowOffset}, {columnOffset}",
                _ => throw new InvalidOperationException("Only 'B', 'C', 'R' and '' are valid relativity types for a cell reference"),
            };
        }

        private string CalculateArgumentStringFromCellLocation(string cellLocationName, string relativitySymbol)
        {
            (var row, var column) = GetCellLocationFromVariable(cellLocationName);
            return CalculateArgumentsFromLocation(relativitySymbol, row, column);
        }

        /// <param name="cellReference">Looks like (A1, B_A1, R_A1, C_A1)</param>
        private string GetArgumentStringFromCellReference(string[] cellReference)
        {
            var (rangeRelativitySymbol, rangeCellLocationName) = GetRelativitySymbolAndCellLocationNameFromCellReference(cellReference);
            if (IsCellLocation(rangeCellLocationName))
            {
                return CalculateArgumentStringFromCellLocation(rangeCellLocationName, rangeRelativitySymbol);
            }

            return string.Empty;
        }
    }
}
