using Cell.Model;
using Cell.ViewModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cell.Plugin.SyntaxRewriters
{
    public partial class CodeToCellReferenceSyntaxRewriter(CellModel cell) : CSharpSyntaxRewriter
    {
        private readonly CellModel cell = cell;

        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            node = base.Visit(node);
            if (CellReferenceSyntaxWalker.TryGetCellReferenceFromNode(node, out var sheetName, out var rowValue, out var columnValue, out var isRowRelative, out var isColumnRelative))
            {
                var sheetPrefix = string.IsNullOrEmpty(sheetName) ? string.Empty : $"{sheetName}_";

                string? relativitySymbol = null;
                if (!isRowRelative && !isColumnRelative)
                {
                    relativitySymbol = "B_";
                }
                if (!isRowRelative)
                {
                    relativitySymbol ??= "R_";
                }
                else
                {
                    rowValue += cell.Row;
                }
                if (!isColumnRelative)
                {
                    relativitySymbol ??= "C_";
                }
                else
                {
                    columnValue += cell.Column;
                }
                relativitySymbol ??= string.Empty;


                var cellLocation = $"{sheetPrefix}{relativitySymbol}{ColumnCellViewModel.GetColumnName(columnValue)}{rowValue}";
                return SyntaxFactory.ParseExpression(cellLocation);
            }
            return node;
        }
    }
}

