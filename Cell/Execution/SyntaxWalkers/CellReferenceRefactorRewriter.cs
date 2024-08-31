using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cell.Execution.SyntaxWalkers
{
    public class CellReferenceRefactorRewriter(Func<CellReference, CellReference> refactorFunction) : CSharpSyntaxRewriter
    {
        private readonly Func<CellReference, CellReference> _refactorFunction = refactorFunction;
        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            node = base.Visit(node);
            if (node == null) return node;
            if (CellReference.TryCreateReferenceFromCode(node, out var cellReference))
            {
                var refactoredReference = _refactorFunction(cellReference);
                var codeForReference = refactoredReference.CreateCodeForReference();
                return SyntaxFactory.ParseExpression(codeForReference);
            }
            return node;
        }
    }
}
