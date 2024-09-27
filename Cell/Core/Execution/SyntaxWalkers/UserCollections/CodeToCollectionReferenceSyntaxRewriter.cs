using Cell.Execution.References;
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
                if (collectionReference is ConstantCollectionReference constantCollectionReference)
                {
                    return SyntaxUtilities.CreateSyntaxNodePreservingTrivia(node, constantCollectionReference.ConstantCollectionName);
                }
            }
            return node;
        }
    }
}
