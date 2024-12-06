namespace Cell.Core.Execution.SyntaxWalkers.UserCollections
{
    /// <summary>
    /// A transformer that can transform a syntax tree between using user friendly collection references, and code only collection references.
    /// </summary>
    public class CollectionReferenceSyntaxTransformer : SyntaxTransformer
    {
        /// <summary>
        /// Create a new instance of the <see cref="CollectionReferenceSyntaxTransformer"/> class.
        /// </summary>
        public CollectionReferenceSyntaxTransformer(IReadOnlyList<string> collectionNames) : base(new CodeToCollectionReferenceSyntaxRewriter(), new CollectionReferenceToCodeSyntaxRewriter(collectionNames))
        {
        }
    }
}
