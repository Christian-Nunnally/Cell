using Microsoft.CodeAnalysis.CSharp;

namespace Cell.Plugin.SyntaxWalkers
{
    public class SyntaxTransformer
    {
        private readonly CSharpSyntaxRewriter _rewriterFrom;
        private readonly CSharpSyntaxRewriter _rewriterTo;
        public SyntaxTransformer(CSharpSyntaxRewriter rewriteTo, CSharpSyntaxRewriter rewriteFrom)
        {
            _rewriterTo = rewriteTo;
            _rewriterFrom = rewriteFrom;
        }

        public string TransformFrom(string code) => Transform(code, _rewriterFrom);

        public string TransformTo(string code) => Transform(code, _rewriterTo);

        private string Transform(string code, CSharpSyntaxRewriter rewriter)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var root = syntaxTree.GetRoot();
            var newRoot = rewriter.Visit(root);
            return newRoot.ToFullString();
        }
    }
}
