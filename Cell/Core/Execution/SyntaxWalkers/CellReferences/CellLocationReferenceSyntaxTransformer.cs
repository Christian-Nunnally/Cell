using Cell.Model;
using Cell.Plugin.SyntaxWalkers;

namespace Cell.Execution.SyntaxWalkers.CellReferences
{
    public class CellLocationReferenceSyntaxTransformer(CellLocationModel location) : SyntaxTransformer(new CodeToCellReferenceSyntaxRewriter(location), new CellReferenceToCodeSyntaxRewriter(location))
    {
    }
}
