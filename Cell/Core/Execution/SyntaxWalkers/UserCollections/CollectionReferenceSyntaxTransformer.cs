namespace Cell.Execution.SyntaxWalkers.UserCollections
{
    public class CollectionReferenceSyntaxTransformer(IReadOnlyDictionary<string, string> collectionNameToDataTypeMap) : SyntaxTransformer(new CodeToCollectionReferenceSyntaxRewriter(), new CollectionReferenceToCodeSyntaxRewriter(collectionNameToDataTypeMap))
    {
    }
}
