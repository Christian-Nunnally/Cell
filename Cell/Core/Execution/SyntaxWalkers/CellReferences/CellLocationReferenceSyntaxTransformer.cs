using Cell.Model;
using Cell.Plugin.SyntaxWalkers;

namespace Cell.Core.Execution.SyntaxWalkers.CellReferences
{
    /// <summary>
    /// A transformer that can transform a syntax tree between using user friendly cell references, and code only cell references.
    /// </summary>
    public class CellLocationReferenceSyntaxTransformer : SyntaxTransformer
    {
        /// <summary>
        /// Create a new instance of the <see cref="CellLocationReferenceSyntaxTransformer"/> class.
        /// </summary>
        /// <param name="location">The location to resolve relative references from.</param>
        public CellLocationReferenceSyntaxTransformer(CellLocationModel location) : base(new CodeToCellReferenceSyntaxRewriter(location), new CellReferenceToCodeSyntaxRewriter(location))
        {
        }
    }
}
