using Cell.Execution.References;
using Cell.Model;
using Cell.ViewModel.Execution;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace Cell.Execution.SyntaxWalkers.UserCollections
{
    /// <summary>
    /// Traverses the syntax tree and collects all collection references.
    /// </summary>
    public partial class CollectionReferenceSyntaxWalker : CSharpSyntaxWalker
    {
        /// <summary>
        /// The collection references found in the syntax tree.
        /// </summary>
        public readonly List<ICollectionReference> CollectionReferences = [];

        /// <summary>
        /// Attempts to get a collection reference from a syntax node, if the syntax node represents a valid collection reference.
        /// </summary>
        /// <param name="node">The sytntax node to check.</param>
        /// <param name="collectionReference">The resulting collection reference, if any.</param>
        /// <returns>True if the syntax node was a collection reference.</returns>
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

        /// <summary>
        /// Represents a <see cref="CSharpSyntaxVisitor"/> that descends an entire <see cref="CSharpSyntaxNode"/> graph
        /// visiting each CSharpSyntaxNode and its child SyntaxNodes and <see cref="SyntaxToken"/>s in depth-first order.
        /// </summary>
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
