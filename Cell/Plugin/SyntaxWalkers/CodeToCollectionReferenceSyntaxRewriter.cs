using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cell.Plugin.SyntaxWalkers
{
    public class CodeToCollectionReferenceSyntaxRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            node = base.Visit(node);

            if (CollectionReferenceSyntaxWalker.TryGetCollectionReferenceFromNode(node, out var collectionReference))
            {
                return SyntaxFactory.ParseExpression(collectionReference);
            }

            return node;
        }
    }
}
