using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cell.Execution.SyntaxWalkers.UserCollections
{
    public class CollectionReferenceRenameRewriter(string oldName, string newName) : CSharpSyntaxRewriter
    {
        private readonly string _newName = newName;
        private readonly string _oldName = oldName;
        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            node = base.Visit(node);
            if (node == null) return node;

            if (CollectionReferenceSyntaxWalker.TryGetCollectionReferenceFromNode(node, out var _))
            {
                var code = node.ToFullString().Replace($"\"{_oldName}\"", $"\"{_newName}\"");
                return SyntaxUtilities.CreateSyntaxNodePreservingTrivia(node, code);
            }

            return node;
        }
    }
}
