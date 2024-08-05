using Cell.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;

namespace Cell.Plugin.SyntaxWalkers
{
    public partial class CellReferenceToCodeSyntaxRewriter(CellModel cell) : CSharpSyntaxRewriter
    {
        private readonly CellModel cell = cell;

        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            node = base.Visit(node);

            if (node is IdentifierNameSyntax identifierSyntax)
            {
                var variableName = identifierSyntax.Identifier.ToString();
                var splitVariableName = variableName.Split('_');

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
                if (splitVariableName.Length > 1)
                {
                    var possibleSheetName = splitVariableName[0];
                    if (!IsCellLocation(possibleSheetName) && possibleSheetName != "R" && possibleSheetName != "C" && possibleSheetName != "B")
                    {
                        sheetName = $"\"{possibleSheetName}\"";
                        splitVariableName = splitVariableName[1..];
                    }
                }

                var rangeIndex = splitVariableName.ToList().IndexOf("Range");

                var rangeArguments = "";
                if (rangeIndex >= 0)
                {
                    var rangeVariableName = splitVariableName[(rangeIndex+1)..];
                    var rangeRelativitySymbol = "";
                    var rangeCellName = rangeVariableName[0];
                    if (rangeVariableName.Length == 2)
                    {
                        rangeRelativitySymbol = rangeVariableName[0];
                        rangeCellName = rangeVariableName[1];
                    }

                    if (IsCellLocation(rangeCellName))
                    {
                        rangeArguments = CalculateArgumentStringFromCellLocation(rangeCellName, rangeRelativitySymbol);
                    }

                    splitVariableName = splitVariableName[..rangeIndex];
                }

                var relativitySymbol = "";
                var cellLocationName = splitVariableName[0];
                if (splitVariableName.Length == 2)
                {
                    relativitySymbol = splitVariableName[0];
                    cellLocationName = splitVariableName[1];
                }

                if (IsCellLocation(cellLocationName))
                {
                    string cellLocationArguments = CalculateArgumentStringFromCellLocation(cellLocationName, relativitySymbol);
                    return SyntaxFactory.ParseExpression($"c.GetCell({sheetName}{cellLocationArguments}{rangeArguments})");
                }
            }
            return node;
        }

        private string CalculateArgumentStringFromCellLocation(string cellLocationName, string relativitySymbol)
        {
            (var row, var column) = GetCellLocationFromVariable(cellLocationName);
            return relativitySymbol switch
            {
                "" => $", cell.Row + {row - cell.Row}, cell.Column + {column - cell.Column}",
                "C" => $", cell.Row + {row - cell.Row}, {column}",
                "R" => $", {row}, cell.Column + {column - cell.Column}",
                "B" => $", {row}, {column}",
                _ => throw new InvalidOperationException("Only 'B', 'C', 'R' and '' are valid relativity types for a cell reference"),
            };
        }

        public static bool IsCellLocation(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && IsCellLocationString().IsMatch(input);
        }

        private static (int Row, int Column) GetCellLocationFromVariable(string variableName)
        {
            string columnPart = GetColumnFromCellLocationString().Match(variableName).Value;
            string rowPart = GetRowFromCellLocationString().Match(variableName).Value;
            return (int.Parse(rowPart), ColumnToIndex(columnPart));
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

        [GeneratedRegex(@"^[A-Za-z]+[0-9]+$")]
        private static partial Regex IsCellLocationString();

        [GeneratedRegex(@"^[A-Za-z]+")]
        private static partial Regex GetColumnFromCellLocationString();

        [GeneratedRegex(@"\d+$")]
        private static partial Regex GetRowFromCellLocationString();
    }
}

