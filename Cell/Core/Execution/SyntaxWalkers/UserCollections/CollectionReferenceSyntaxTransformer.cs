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
        /// <param name="collectionNameToDataTypeMap">The mapping of collection names and thier items data type.</param>
        public CollectionReferenceSyntaxTransformer(IReadOnlyDictionary<string, string> collectionNameToDataTypeMap) : base(new CodeToCollectionReferenceSyntaxRewriter(), new CollectionReferenceToCodeSyntaxRewriter(collectionNameToDataTypeMap))
        {
        }
    }
}
