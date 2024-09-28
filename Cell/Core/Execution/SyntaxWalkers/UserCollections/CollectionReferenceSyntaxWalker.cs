using Cell.Execution.References;
using Cell.Model;
using Cell.ViewModel.Execution;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace Cell.Execution.SyntaxWalkers.UserCollections
{
    public partial class CollectionReferenceSyntaxWalker : CSharpSyntaxWalker
    {
        public readonly List<ICollectionReference> CollectionReferences = [];
        public static bool TryGetCollectionReferenceFromNode(SyntaxNode? node, [MaybeNullWhen(false)] out ICollectionReference collectionReference)
        {
            collectionReference = ConstantCollectionReference.Null;

            if (node is not InvocationExpressionSyntax syntax) return false;
            if (syntax.Expression is not MemberAccessExpressionSyntax memberAccessExpressionSyntax) return false;
            if (memberAccessExpressionSyntax.Name.Identifier.Text != "GetUserList") return false;
            if (memberAccessExpressionSyntax.Expression is not IdentifierNameSyntax identifierName) return false;
            if (identifierName.Identifier.Text != "c") return false;
            if (syntax.ArgumentList.Arguments.Count != 1) return false;
            var argument = syntax.ArgumentList.Arguments[0];

            if (argument.Expression is LiteralExpressionSyntax literalExpressionSyntax)
            {
                var collectionName = literalExpressionSyntax.Token.ValueText;
                collectionReference = new ConstantCollectionReference(collectionName);
                return true;
            }

            var collectionReferenceSyntax = argument.Expression.ToString();
            var codeWithReturn = $"return {collectionReferenceSyntax};";
            var functionModel = new CellFunctionModel("collectionReference", codeWithReturn, "object");
            var function = new CellFunction(functionModel);
            collectionReference = new DynamicCollectionReference(function);
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
