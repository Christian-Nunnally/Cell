using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cell.Core.Execution.SyntaxWalkers.UserCollections
{
    /// <summary>
    /// A syntax rewriter that renames a collection references like c.GetUserList("name") to c.GetUserList("newName").
    /// </summary>
    /// <param name="oldName">The old collection name to look for references to.</param>
    /// <param name="newName">The new collection name to rename the references to.</param>
    public class CollectionReferenceRenameRewriter(string oldName, string newName) : CSharpSyntaxRewriter
    {
        private readonly string _newName = newName;
        private readonly string _oldName = oldName;

        /// <summary>
        /// Represents a <see cref="CSharpSyntaxVisitor"/> that descends an entire <see cref="CSharpSyntaxNode"/> graph
        /// visiting each CSharpSyntaxNode and its child SyntaxNodes and <see cref="SyntaxToken"/>s in depth-first order.
        /// </summary>
        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            node = base.Visit(node);
            if (node is null) return node;

            if (CollectionReferenceSyntaxWalker.TryGetCollectionReferenceFromNode(node, out var _))
            {
                var code = node.ToFullString().Replace($"\"{_oldName}\"", $"\"{_newName}\"");
                return SyntaxUtilities.CreateSyntaxNodePreservingTrivia(node, code);
            }

            return node;
        }
    }
}
