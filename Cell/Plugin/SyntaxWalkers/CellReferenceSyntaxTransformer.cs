using Cell.Model;

namespace Cell.Plugin.SyntaxWalkers
{
    internal class CellReferenceSyntaxTransformer(CellModel cell) : SyntaxTransformer(new CodeToCellReferenceSyntaxRewriter(cell), new CellReferenceToCodeSyntaxRewriter(cell))
    {
    }
}
