using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cell.Core.Execution.SyntaxWalkers.UserCollections
{
    /// <summary>
    /// Traverse the syntax tree and replace user collection references with the appropriate code.
    /// </summary>
    public class CollectionReferenceToCodeSyntaxRewriter : CSharpSyntaxRewriter
    {
        private readonly IReadOnlyList<string> _collectionNames;

        /// <summary>
        /// Create a new instance of <see cref="CollectionReferenceToCodeSyntaxRewriter"/>.
        /// </summary>
        /// <param name="collectionNames">The valid list of collection names.</param>
        public CollectionReferenceToCodeSyntaxRewriter(IReadOnlyList<string> collectionNames)
        {
            _collectionNames = collectionNames;
        }
        /// <summary>
        /// Represents a <see cref="CSharpSyntaxVisitor"/> that descends an entire <see cref="CSharpSyntaxNode"/> graph
        /// visiting each CSharpSyntaxNode and its child SyntaxNodes and <see cref="SyntaxToken"/>s in depth-first order.
        /// </summary>
        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            node = base.Visit(node);

            if (node is IdentifierNameSyntax identifierSyntax)
            {
                var variableName = identifierSyntax.Identifier.ToString();
                if (IsCollectionName(variableName))
                {
                    var code = $"c.GetUserList(\"{variableName}\")";
                    return SyntaxUtilities.CreateSyntaxNodePreservingTrivia(node, code);
                }
            }
            return node;
        }

        private bool IsCollectionName(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && _collectionNames.Contains(input);
        }
    }
}
