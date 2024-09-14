using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cell.Execution.SyntaxWalkers.UserCollections
{
    public class CodeToCollectionReferenceSyntaxRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            node = base.Visit(node);

            if (CollectionReferenceSyntaxWalker.TryGetCollectionReferenceFromNode(node, out var collectionReference))
            {
                return SyntaxUtilities.CreateSyntaxNodePreservingTrivia(node, collectionReference);
            }

            return node;
        }
    }
}
