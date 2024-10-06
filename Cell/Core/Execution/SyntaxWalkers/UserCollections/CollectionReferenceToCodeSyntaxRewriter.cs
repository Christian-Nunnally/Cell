using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cell.Execution.SyntaxWalkers.UserCollections
{
    /// <summary>
    /// Traverse the syntax tree and replace user collection references with the appropriate code.
    /// </summary>
    /// <param name="collectionNameToDataTypeMap"></param>
    public class CollectionReferenceToCodeSyntaxRewriter(IReadOnlyDictionary<string, string> collectionNameToDataTypeMap) : CSharpSyntaxRewriter
    {
        private readonly IReadOnlyDictionary<string, string> _collectionNameToDataTypeMap = collectionNameToDataTypeMap;
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
                    var dataType = _collectionNameToDataTypeMap[variableName];
                    var code = $"c.GetUserList<{dataType}>(\"{variableName}\")";
                    return SyntaxUtilities.CreateSyntaxNodePreservingTrivia(node, code);
                }
            }
            return node;
        }

        private bool IsCollectionName(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && _collectionNameToDataTypeMap.ContainsKey(input);
        }
    }
}
