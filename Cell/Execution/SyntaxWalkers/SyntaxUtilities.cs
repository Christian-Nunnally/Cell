using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace Cell.Execution.SyntaxWalkers
{
    internal class SyntaxUtilities
    {
        internal static SyntaxNode CreateSyntaxNodePreservingTrivia(SyntaxNode? nodeToGetTriviaFrom, string code)
        {
            var result = SyntaxFactory.ParseExpression(code);
            if (nodeToGetTriviaFrom == null) return result;
            var nodeWithLeadingTrivia = nodeToGetTriviaFrom.HasLeadingTrivia ? result.WithLeadingTrivia(nodeToGetTriviaFrom.GetLeadingTrivia()) : (SyntaxNode)result;
            var nodeWithTrailingTrivia = nodeToGetTriviaFrom.HasTrailingTrivia ? nodeWithLeadingTrivia.WithTrailingTrivia(nodeToGetTriviaFrom.GetTrailingTrivia()) : nodeWithLeadingTrivia;
            return nodeWithTrailingTrivia;
        }
    }
}
