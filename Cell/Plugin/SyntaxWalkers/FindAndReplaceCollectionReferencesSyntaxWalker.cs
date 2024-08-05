using Cell.Persistence;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cell.Plugin.SyntaxWalkers
{
    public class FindAndReplaceCollectionReferencesSyntaxWalker : CSharpSyntaxRewriter
    {
        public readonly List<string> CollectionReferences = [];

        public CompileResult Result { get; private set; } = new CompileResult { Success = true };

        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            node = base.Visit(node);

            if (node is IdentifierNameSyntax identifierSyntax)
            {
                var variableName = identifierSyntax.Identifier.ToString();
                if (IsCollectionName(variableName))
                {
                    CollectionReferences.Add(variableName);
                    var dataType = UserCollectionLoader.GetDataTypeStringForCollection(variableName);
                    if (string.IsNullOrWhiteSpace(dataType)) Result = new CompileResult { Success = false, Result = $"Datatype for Collection {variableName} does not exist or cells have not loaded yet." };
                    return SyntaxFactory.ParseExpression($"c.GetUserList<{dataType}>(\"{variableName}\")");
                }
            }
            return node;
        }

        public static bool IsCollectionName(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && UserCollectionLoader.CollectionNames.Contains(input);
        }
    }
}

