using Cell.Model;
using Cell.Plugin.SyntaxWalkers;

namespace Cell.Execution.SyntaxWalkers.CellReferences
{
    public class CellReferenceSyntaxTransformer(CellModel cell) : SyntaxTransformer(new CodeToCellReferenceSyntaxRewriter(cell), new CellReferenceToCodeSyntaxRewriter(cell))
    {
    }
}
