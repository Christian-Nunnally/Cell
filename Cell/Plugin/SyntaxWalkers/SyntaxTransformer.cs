using Microsoft.CodeAnalysis.CSharp;

namespace Cell.Plugin.SyntaxWalkers
{
    public class SyntaxTransformer
    {
        private readonly CSharpSyntaxRewriter _rewriterTo;
        private readonly CSharpSyntaxRewriter _rewriterFrom;

        public SyntaxTransformer(CSharpSyntaxRewriter rewriteTo, CSharpSyntaxRewriter rewriteFrom) 
        {
            _rewriterTo = rewriteTo;
            _rewriterFrom = rewriteFrom;
        }

        public string TransformTo(string code) => Transform(code, _rewriterTo);

        public string TransformFrom(string code) => Transform(code, _rewriterFrom);

        private string Transform(string code, CSharpSyntaxRewriter rewriter)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var root = syntaxTree.GetRoot();
            var newRoot = rewriter.Visit(root);
            return newRoot.ToFullString();
        }
    }
}
