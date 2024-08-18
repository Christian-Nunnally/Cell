using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cell.Plugin.SyntaxWalkers
{
    public class CollectionReferenceRenameRewriter(string oldName, string newName) : CSharpSyntaxRewriter
    {
        private readonly string _newName = newName;
        private readonly string _oldName = oldName;
        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            node = base.Visit(node);
            if (node == null) return node;

            if (CollectionReferenceSyntaxWalker.TryGetCollectionReferenceFromNode(node, out var collectionReference))
            {
                return SyntaxFactory.ParseExpression(node.ToFullString().Replace($"\"{_oldName}\"", $"\"{_newName}\""));
            }

            return node;
        }
    }
}
