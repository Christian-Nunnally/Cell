using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cell.Execution.SyntaxWalkers.UserCollections
{
    public class CollectionReferenceToCodeSyntaxRewriter(IReadOnlyDictionary<string, string> collectionNameToDataTypeMap) : CSharpSyntaxRewriter
    {
        public readonly List<string> CollectionReferences = [];
        private readonly IReadOnlyDictionary<string, string> _collectionNameToDataTypeMap = collectionNameToDataTypeMap;
        public CompileResult Result { get; private set; } = new CompileResult { WasSuccess = true };

        public bool IsCollectionName(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && _collectionNameToDataTypeMap.ContainsKey(input);
        }

        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            node = base.Visit(node);

            if (node is IdentifierNameSyntax identifierSyntax)
            {
                var variableName = identifierSyntax.Identifier.ToString();
                if (IsCollectionName(variableName))
                {
                    CollectionReferences.Add(variableName);
                    var dataType = _collectionNameToDataTypeMap[variableName];
                    if (string.IsNullOrWhiteSpace(dataType)) Result = new CompileResult { WasSuccess = false, ExecutionResult = $"Datatype for Collection {variableName} does not exist or cells have not loaded yet." };
                    var code = $"c.GetUserList<{dataType}>(\"{variableName}\")";
                    return SyntaxUtilities.CreateSyntaxNodePreservingTrivia(node, code);
                }
            }
            return node;
        }
    }
}
