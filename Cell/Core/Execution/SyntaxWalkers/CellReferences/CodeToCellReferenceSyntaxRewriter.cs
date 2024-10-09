using Cell.Execution.References;
using Cell.Model;
using Cell.ViewModel.Cells.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cell.Execution.SyntaxWalkers.CellReferences
{
    /// <summary>
    /// Syntax rewriter that converts code cell references like "c.GetCell().;" to user friendly cell references like "A1.".
    /// </summary>
    /// <param name="location">The location to resolve relative location references based on.</param>
    public partial class CodeToCellReferenceSyntaxRewriter(CellLocationModel location) : CSharpSyntaxRewriter
    {
        private readonly CellLocationModel _location = location;
        /// <summary>
        /// Represents a <see cref="CSharpSyntaxRewriter"/> that descends an entire <see cref="CSharpSyntaxNode"/> graph
        /// visiting each CSharpSyntaxNode and its child SyntaxNodes and <see cref="SyntaxToken"/>s in depth-first order,
        /// </summary>
        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            node = base.Visit(node);
            if (LocationReference.TryCreateReferenceFromCode(node, out var cellReference))
            {
                var sheetPrefix = string.IsNullOrEmpty(cellReference.SheetReference.Value) ? string.Empty : $"{cellReference.SheetReference.Value}_";

                string? relativitySymbol = null;
                if (!cellReference.RowReference.IsRelative && !cellReference.ColumnReference.IsRelative)
                {
                    relativitySymbol = "B_";
                }
                if (!cellReference.RowReference.IsRelative)
                {
                    relativitySymbol ??= "R_";
                }
                else
                {
                    cellReference.RowReference.Value += _location.Row;
                }
                if (!cellReference.ColumnReference.IsRelative)
                {
                    relativitySymbol ??= "C_";
                }
                else
                {
                    cellReference.ColumnReference.Value += _location.Column;
                }
                relativitySymbol ??= string.Empty;

                string rangePart = string.Empty;
                string? rangeRelativitySymbol = null;
                if (cellReference.IsRange)
                {
                    if (!cellReference.RowRangeEndReference.IsRelative && !cellReference.ColumnRangeEndReference.IsRelative)
                    {
                        rangeRelativitySymbol = "B_";
                    }
                    if (!cellReference.RowRangeEndReference.IsRelative)
                    {
                        rangeRelativitySymbol ??= "R_";
                    }
                    else
                    {
                        cellReference.RowRangeEndReference.Value += _location.Row;
                    }
                    if (!cellReference.ColumnRangeEndReference.IsRelative)
                    {
                        rangeRelativitySymbol ??= "C_";
                    }
                    else
                    {
                        cellReference.ColumnRangeEndReference.Value += _location.Column;
                    }
                    rangeRelativitySymbol ??= string.Empty;

                    rangePart = $"_Range_{rangeRelativitySymbol}{ColumnCellViewModel.GetColumnName(cellReference.ColumnRangeEndReference.Value)}{cellReference.RowRangeEndReference.Value}";
                }

                var cellLocation = $"{sheetPrefix}{relativitySymbol}{ColumnCellViewModel.GetColumnName(cellReference.ColumnReference.Value)}{cellReference.RowReference.Value}{rangePart}";
                return SyntaxUtilities.CreateSyntaxNodePreservingTrivia(node, cellLocation);
            }
            return node;
        }
    }
}
