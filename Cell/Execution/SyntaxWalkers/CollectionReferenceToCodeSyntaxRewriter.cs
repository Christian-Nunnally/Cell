using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cell.Execution.SyntaxWalkers
{
    public class CollectionReferenceToCodeSyntaxRewriter(Func<string, string> getDataTypeFromCollectionNameFunction, Predicate<string> isCollectionPredicate) : CSharpSyntaxRewriter
    {
        public readonly List<string> CollectionReferences = [];
        private readonly Func<string, string> _getDataTypeFromCollectionNameFunction = getDataTypeFromCollectionNameFunction;
        private readonly Predicate<string> _isCollectionPredicate = isCollectionPredicate;
        public CompileResult Result { get; private set; } = new CompileResult { Success = true };

        public bool IsCollectionName(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && _isCollectionPredicate.Invoke(input);
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
                    var dataType = _getDataTypeFromCollectionNameFunction(variableName);
                    if (string.IsNullOrWhiteSpace(dataType)) Result = new CompileResult { Success = false, Result = $"Datatype for Collection {variableName} does not exist or cells have not loaded yet." };
                    var expression = SyntaxFactory.ParseExpression($"c.GetUserList<{dataType}>(\"{variableName}\")");
                    return expression;
                }
            }
            return node;
        }
    }
}
