using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace Cell.Execution.SyntaxWalkers
{
    public partial class CollectionReferenceSyntaxWalker : CSharpSyntaxWalker
    {
        public readonly List<string> CollectionReferences = [];
        public static bool TryGetCollectionReferenceFromNode(SyntaxNode? node, [MaybeNullWhen(false)] out string collectionReference)
        {
            collectionReference = "";
            if (node is not InvocationExpressionSyntax syntax) return false;
            if (syntax.Expression is not MemberAccessExpressionSyntax memberAccessExpressionSyntax) return false;
            if (memberAccessExpressionSyntax.Name.Identifier.Text != "GetUserList") return false;
            if (memberAccessExpressionSyntax.Expression is not IdentifierNameSyntax identifierName) return false;
            if (identifierName.Identifier.Text != "c") return false;
            if (syntax.ArgumentList.Arguments.Count != 1) return false;
            var argument = syntax.ArgumentList.Arguments[0];
            if (argument.Expression is not LiteralExpressionSyntax literalExpressionSyntax) return false;
            collectionReference = literalExpressionSyntax.Token.ValueText;
            return true;
        }

        public override void Visit(SyntaxNode? node)
        {
            base.Visit(node);
            if (TryGetCollectionReferenceFromNode(node, out var collectionReference))
            {
                CollectionReferences.Add(collectionReference);
            }
        }
    }
}
