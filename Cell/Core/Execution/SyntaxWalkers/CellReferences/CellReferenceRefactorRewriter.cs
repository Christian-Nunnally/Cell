using Cell.Core.Execution.References;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cell.Core.Execution.SyntaxWalkers.CellReferences
{
    /// <summary>
    /// A syntax rewriter that traverses a syntax tree and refactors cell references found in the tree with a given refactor function.
    /// </summary>
    /// <param name="refactorFunction"></param>
    public class CellReferenceRefactorRewriter(Func<LocationReference, LocationReference> refactorFunction) : CSharpSyntaxRewriter
    {
        private readonly Func<LocationReference, LocationReference> _refactorFunction = refactorFunction;
        /// <summary>
        /// Represents a <see cref="CSharpSyntaxRewriter"/> that descends an entire <see cref="CSharpSyntaxNode"/> graph
        /// visiting each CSharpSyntaxNode and its child SyntaxNodes and <see cref="SyntaxToken"/>s in depth-first order,
        /// and returns a new syntax tree with some nodes replaced.
        /// </summary>
        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            node = base.Visit(node);
            if (node == null) return node;
            if (LocationReference.TryCreateReferenceFromCode(node, out var cellReference))
            {
                var refactoredReference = _refactorFunction(cellReference);
                var codeForReference = refactoredReference.CreateCodeForReference();
                return SyntaxUtilities.CreateSyntaxNodePreservingTrivia(node, codeForReference);
            }
            return node;
        }
    }
}
