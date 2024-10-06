using Microsoft.CodeAnalysis.CSharp;

namespace Cell.Execution.SyntaxWalkers
{
    /// <summary>
    /// A transformer that can transform a syntax tree between two different syntax trees.
    /// </summary>
    /// <param name="rewriteTo">The syntax rewriter that knows how to transform from A to B.</param>
    /// <param name="rewriteFrom">The syntax rewriter that knows how to transform from B to A.</param>
    public class SyntaxTransformer(CSharpSyntaxRewriter rewriteTo, CSharpSyntaxRewriter rewriteFrom)
    {
        private readonly CSharpSyntaxRewriter _rewriterFrom = rewriteFrom;
        private readonly CSharpSyntaxRewriter _rewriterTo = rewriteTo;
        /// <summary>
        /// Transform the code from B to A.
        /// </summary>
        /// <param name="code">The code to transform.</param>
        /// <returns>The transformed code.</returns>
        public string TransformFrom(string code) => Transform(code, _rewriterFrom);

        /// <summary>
        /// Transform the code from A to B.
        /// </summary>
        /// <param name="code">The code to transform.</param>
        /// <returns>The transformed code.</returns>
        public string TransformTo(string code) => Transform(code, _rewriterTo);

        private static string Transform(string code, CSharpSyntaxRewriter rewriter)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var root = syntaxTree.GetRoot();
            var newRoot = rewriter.Visit(root);
            return newRoot.ToFullString();
        }
    }
}
