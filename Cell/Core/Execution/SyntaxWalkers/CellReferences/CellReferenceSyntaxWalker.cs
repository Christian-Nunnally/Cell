using Cell.Core.Execution.References;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cell.Core.Execution.SyntaxWalkers.CellReferences
{
    /// <summary>
    /// A syntax walker that traverses a syntax tree and collects all cell references found in the tree.
    /// </summary>
    public partial class CellReferenceSyntaxWalker : CSharpSyntaxWalker
    {
        /// <summary>
        /// The list of location references found in the syntax tree during the last visit.
        /// </summary>
        public readonly List<LocationReference> LocationReferences = [];
        /// <summary>
        /// Represents a <see cref="CSharpSyntaxRewriter"/> that descends an entire <see cref="CSharpSyntaxNode"/> graph
        /// visiting each CSharpSyntaxNode and its child SyntaxNodes and <see cref="SyntaxToken"/>s in depth-first order,
        /// </summary>
        public override void Visit(SyntaxNode? node)
        {
            base.Visit(node);
            if (LocationReference.TryCreateReferenceFromCode(node, out var cellReference))
            {
                LocationReferences.Add(cellReference);
            }
        }
    }
}
