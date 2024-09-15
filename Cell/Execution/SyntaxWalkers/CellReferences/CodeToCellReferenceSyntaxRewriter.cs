using Cell.Model;
using Cell.ViewModel.Cells.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cell.Execution.SyntaxWalkers.CellReferences
{
    public partial class CodeToCellReferenceSyntaxRewriter(CellModel cell) : CSharpSyntaxRewriter
    {
        private readonly CellModel cell = cell;
        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            node = base.Visit(node);
            if (CellReference.TryCreateReferenceFromCode(node, out var cellReference))
            {
                var sheetPrefix = string.IsNullOrEmpty(cellReference.SheetName) ? string.Empty : $"{cellReference.SheetName}_";

                string? relativitySymbol = null;
                if (!cellReference.IsRowRelative && !cellReference.IsColumnRelative)
                {
                    relativitySymbol = "B_";
                }
                if (!cellReference.IsRowRelative)
                {
                    relativitySymbol ??= "R_";
                }
                else
                {
                    cellReference.Row += cell.Row;
                }
                if (!cellReference.IsColumnRelative)
                {
                    relativitySymbol ??= "C_";
                }
                else
                {
                    cellReference.Column += cell.Column;
                }
                relativitySymbol ??= string.Empty;

                string rangePart = string.Empty;
                string? rangeRelativitySymbol = null;
                if (cellReference.IsRange)
                {
                    if (!cellReference.IsRowRelativeRangeEnd && !cellReference.IsColumnRelativeRangeEnd)
                    {
                        rangeRelativitySymbol = "B_";
                    }
                    if (!cellReference.IsRowRelativeRangeEnd)
                    {
                        rangeRelativitySymbol ??= "R_";
                    }
                    else
                    {
                        cellReference.RowRangeEnd += cell.Row;
                    }
                    if (!cellReference.IsColumnRelativeRangeEnd)
                    {
                        rangeRelativitySymbol ??= "C_";
                    }
                    else
                    {
                        cellReference.ColumnRangeEnd += cell.Column;
                    }
                    rangeRelativitySymbol ??= string.Empty;

                    rangePart = $"_Range_{rangeRelativitySymbol}{ColumnCellViewModel.GetColumnName(cellReference.ColumnRangeEnd)}{cellReference.RowRangeEnd}";
                }

                var cellLocation = $"{sheetPrefix}{relativitySymbol}{ColumnCellViewModel.GetColumnName(cellReference.Column)}{cellReference.Row}{rangePart}";
                return SyntaxUtilities.CreateSyntaxNodePreservingTrivia(node, cellLocation);
            }
            return node;
        }
    }
}
