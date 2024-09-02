using Microsoft.CodeAnalysis.CSharp;

namespace Cell.Execution.SyntaxWalkers
{
    public class SyntaxTransformer(CSharpSyntaxRewriter rewriteTo, CSharpSyntaxRewriter rewriteFrom)
    {
        private readonly CSharpSyntaxRewriter _rewriterFrom = rewriteFrom;
        private readonly CSharpSyntaxRewriter _rewriterTo = rewriteTo;
        public string TransformFrom(string code) => Transform(code, _rewriterFrom);

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
