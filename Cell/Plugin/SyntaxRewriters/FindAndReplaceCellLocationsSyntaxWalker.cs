using Cell.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;

namespace Cell.Plugin.SyntaxRewriters
{
    public partial class FindAndReplaceCellLocationsSyntaxRewriter : CSharpSyntaxRewriter
    {
        public readonly List<CellLocation> LocationReferences = [];

        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            node = base.Visit(node);

            if (node is IdentifierNameSyntax identifierSyntax)
            {
                var variableName = identifierSyntax.Identifier.ToString();
                var splitVariableName = variableName.Split('_');
                var sheetName = "cell";
                if (splitVariableName.Length == 2)
                {
                    sheetName = $"\"{splitVariableName[0]}\"";
                    variableName = splitVariableName[1];
                }
                if (IsCellLocation(variableName))
                {
                    (var row, var column) = GetCellLocationFromVariable(variableName);
                    LocationReferences.Add(new CellLocation(sheetName == "cell" ? string.Empty : sheetName, row, column));

                    return SyntaxFactory.ParseExpression($"c.GetCell({sheetName}, {row}, {column})");
                }
            }
            return node;
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

