
namespace Cell.Plugin.SyntaxWalkers
{
    internal class CollectionReferenceSyntaxTransformer(Func<string, string> getDataTypeFromCollectionNameFunction, Predicate<string> isCollectionPredicate) : SyntaxTransformer(new CodeToCollectionReferenceSyntaxRewriter(), new CollectionReferenceToCodeSyntaxRewriter(getDataTypeFromCollectionNameFunction, isCollectionPredicate))
    {
    }
}
