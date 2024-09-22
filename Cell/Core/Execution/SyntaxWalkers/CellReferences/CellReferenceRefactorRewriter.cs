using Cell.Execution.References;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cell.Execution.SyntaxWalkers.CellReferences
{
    public class CellReferenceRefactorRewriter(Func<LocationReference, LocationReference> refactorFunction) : CSharpSyntaxRewriter
    {
        private readonly Func<LocationReference, LocationReference> _refactorFunction = refactorFunction;
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
