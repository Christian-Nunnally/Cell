namespace Cell.Execution.SyntaxWalkers.UserCollections
{
    internal class CollectionReferenceSyntaxTransformer(Func<string, string> getDataTypeFromCollectionNameFunction, Predicate<string> isCollectionPredicate) : SyntaxTransformer(new CodeToCollectionReferenceSyntaxRewriter(), new CollectionReferenceToCodeSyntaxRewriter(getDataTypeFromCollectionNameFunction, isCollectionPredicate))
    {
    }
}
