using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cell.Core.Execution.SyntaxWalkers
{
    /// <summary>
    /// A utility class for working with syntax nodes.
    /// </summary>
    public class SyntaxUtilities
    {
        /// <summary>
        /// Create a new syntax node from the given code, but make it include the trivia from the given node.
        /// </summary>
        /// <param name="nodeToGetTriviaFrom">The node to copy the trivia off of to the result.</param>
        /// <param name="code">The code to create the SyntaxNode from.</param>
        /// <returns>A syntax node representing the given code, with whitespace sourced from the given node.</returns>
        public static SyntaxNode CreateSyntaxNodePreservingTrivia(SyntaxNode? nodeToGetTriviaFrom, string code)
        {
            var result = SyntaxFactory.ParseExpression(code);
            if (nodeToGetTriviaFrom == null) return result;
            var nodeWithLeadingTrivia = nodeToGetTriviaFrom.HasLeadingTrivia ? result.WithLeadingTrivia(nodeToGetTriviaFrom.GetLeadingTrivia()) : (SyntaxNode)result;
            var nodeWithTrailingTrivia = nodeToGetTriviaFrom.HasTrailingTrivia ? nodeWithLeadingTrivia.WithTrailingTrivia(nodeToGetTriviaFrom.GetTrailingTrivia()) : nodeWithLeadingTrivia;
            return nodeWithTrailingTrivia;
        }
    }
}
