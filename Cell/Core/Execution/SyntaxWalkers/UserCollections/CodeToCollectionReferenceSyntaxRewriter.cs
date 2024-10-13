using Cell.Core.Execution.References;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cell.Core.Execution.SyntaxWalkers.UserCollections
{
    /// <summary>
    /// Traverse the syntax tree and replace references like c.GetUserList() with just the name of the collection.
    /// </summary>
    public class CodeToCollectionReferenceSyntaxRewriter : CSharpSyntaxRewriter
    {
        /// <summary>
        /// Represents a <see cref="CSharpSyntaxVisitor"/> that descends an entire <see cref="CSharpSyntaxNode"/> graph
        /// visiting each CSharpSyntaxNode and its child SyntaxNodes and <see cref="SyntaxToken"/>s in depth-first order.
        /// </summary>
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
