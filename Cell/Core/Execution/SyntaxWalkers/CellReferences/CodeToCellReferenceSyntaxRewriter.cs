using Cell.Core.Execution.References;
using Cell.Model;
using Cell.ViewModel.Cells.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cell.Core.Execution.SyntaxWalkers.CellReferences
{
    /// <summary>
    /// Syntax rewriter that converts code cell references like "c.GetCell().;" to user friendly cell references like "A1.".
    /// </summary>
    /// <param name="location">The location to resolve relative location references based on.</param>
    public partial class CodeToCellReferenceSyntaxRewriter(CellLocationModel location) : CSharpSyntaxRewriter
    {
        private readonly CellLocationModel _location = location;
        /// <summary>
        /// Converts a <see cref="LocationReference"/> to a user friendly cell reference like "A1" or "B_H5" or "SheetName_C_AA56".
        /// </summary>
        /// <param name="locationReference">The location reference to convert.</param>
        /// <returns>The user friendly cell reference like "A1".</returns>
        public string GetUserFriendlyCellReferenceText(LocationReference locationReference)
        {
            var sheetPrefix = string.IsNullOrEmpty(locationReference.SheetReference.Value) ? string.Empty : $"{locationReference.SheetReference.Value}_";

            string? relativitySymbol = null;
            if (!locationReference.RowReference.IsRelative && !locationReference.ColumnReference.IsRelative)
            {
                relativitySymbol = "B_";
            }
            if (!locationReference.RowReference.IsRelative)
            {
                relativitySymbol ??= "R_";
            }
            else
            {
                locationReference.RowReference.Value += _location.Row;
            }
            if (!locationReference.ColumnReference.IsRelative)
            {
                relativitySymbol ??= "C_";
            }
            else
            {
                locationReference.ColumnReference.Value += _location.Column;
            }
            relativitySymbol ??= string.Empty;

            string rangePart = string.Empty;
            string? rangeRelativitySymbol = null;
            if (locationReference.IsRange)
            {
                if (!locationReference.RowRangeEndReference.IsRelative && !locationReference.ColumnRangeEndReference.IsRelative)
                {
                    rangeRelativitySymbol = "B_";
                }
                if (!locationReference.RowRangeEndReference.IsRelative)
                {
                    rangeRelativitySymbol ??= "R_";
                }
                else
                {
                    locationReference.RowRangeEndReference.Value += _location.Row;
                }
                if (!locationReference.ColumnRangeEndReference.IsRelative)
                {
                    rangeRelativitySymbol ??= "C_";
                }
                else
                {
                    locationReference.ColumnRangeEndReference.Value += _location.Column;
                }
                rangeRelativitySymbol ??= string.Empty;

                rangePart = $"_Range_{rangeRelativitySymbol}{ColumnCellViewModel.GetColumnName(locationReference.ColumnRangeEndReference.Value)}{locationReference.RowRangeEndReference.Value}";
            }

            var cellLocation = $"{sheetPrefix}{relativitySymbol}{ColumnCellViewModel.GetColumnName(locationReference.ColumnReference.Value)}{locationReference.RowReference.Value}{rangePart}";
            return cellLocation;
        }

        /// <summary>
        /// Represents a <see cref="CSharpSyntaxRewriter"/> that descends an entire <see cref="CSharpSyntaxNode"/> graph
        /// visiting each CSharpSyntaxNode and its child SyntaxNodes and <see cref="SyntaxToken"/>s in depth-first order,
        /// </summary>
        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            node = base.Visit(node);
            if (LocationReference.TryCreateReferenceFromCode(node, out var cellReference))
            {
                string cellLocation = GetUserFriendlyCellReferenceText(cellReference);
                return SyntaxUtilities.CreateSyntaxNodePreservingTrivia(node, cellLocation);
            }
            return node;
        }
    }
}
