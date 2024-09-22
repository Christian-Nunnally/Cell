using Cell.Execution.References;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cell.Execution.SyntaxWalkers.CellReferences
{
    public partial class CellReferenceSyntaxWalker : CSharpSyntaxWalker
    {
        public readonly List<LocationReference> LocationReferences = [];
        public override void Visit(SyntaxNode? node)
        {
            base.Visit(node);
            if (LocationReference.TryCreateReferenceFromCode(node, out var cellReference))
            {
                LocationReferences.Add(cellReference);
            }
        }
    }
}
