using Cell.Persistence;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cell.Plugin.SyntaxRewriters
{
    public class FindAndReplaceCollectionReferencesSyntaxWalker : CSharpSyntaxRewriter
    {
        public readonly List<string> CollectionReferences = [];

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

