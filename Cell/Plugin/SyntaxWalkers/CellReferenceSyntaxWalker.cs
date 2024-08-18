using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace Cell.Plugin.SyntaxWalkers
{
    public partial class CellReferenceSyntaxWalker : CSharpSyntaxWalker
    {
        public readonly List<CellReference> LocationReferences = [];
        public static bool TryGetCellReferenceFromNode(SyntaxNode? node, out CellReference cellReference)
        {
            cellReference = new CellReference();
            if (!DoesNodeMatchCellReferenceSyntax(node, out var arguments)) return false;
            var sheetName = arguments[0].ToString();
            if (sheetName != "cell" && sheetName.StartsWith('"') && sheetName.EndsWith('"')) cellReference.SheetName = sheetName[1..^1];

            if (!TryParsePositionFromArgument(arguments[1], out var position, out var isRelative)) return false;
            cellReference.Row = position;
            cellReference.IsRowRelative = isRelative;

            if (!TryParsePositionFromArgument(arguments[2], out position, out isRelative)) return false;
            cellReference.Column = position;
            cellReference.IsColumnRelative = isRelative;

            if (arguments.Count == 5)
            {
                cellReference.IsRange = true;
                if (!TryParsePositionFromArgument(arguments[3], out position, out isRelative)) return false;
                cellReference.RowRangeEnd = position;
                cellReference.IsRowRelativeRangeEnd = isRelative;

                if (!TryParsePositionFromArgument(arguments[4], out position, out isRelative)) return false;
                cellReference.ColumnRangeEnd = position;
                cellReference.IsColumnRelativeRangeEnd = isRelative;
            }
            return true;
        }

        public override void Visit(SyntaxNode? node)
        {
            base.Visit(node);
            if (TryGetCellReferenceFromNode(node, out var cellReference))
            {
                LocationReferences.Add(cellReference);
            }
        }

        private static bool DoesNodeMatchCellReferenceSyntax(SyntaxNode? node, [MaybeNullWhen(false)] out SeparatedSyntaxList<ArgumentSyntax> arguments)
        {
            arguments = SyntaxFactory.SeparatedList<ArgumentSyntax>();
            if (node is not InvocationExpressionSyntax syntax) return false;
            if (syntax.Expression is not MemberAccessExpressionSyntax memberAccessExpressionSyntax) return false;
            if (memberAccessExpressionSyntax.Name.Identifier.Text != "GetCell") return false;
            if (memberAccessExpressionSyntax.Expression is not IdentifierNameSyntax identifierName) return false;
            if (identifierName.Identifier.Text != "c") return false;
            if (!(syntax.ArgumentList.Arguments.Count == 3 || syntax.ArgumentList.Arguments.Count == 5)) return false;
            arguments = syntax.ArgumentList.Arguments;
            return true;
        }

        private static bool TryParsePositionFromArgument(ArgumentSyntax argumentSyntax, out int position, out bool isRelative)
        {
            isRelative = false;
            position = 0;
            var row = argumentSyntax.ToString();
            if (argumentSyntax.Expression is BinaryExpressionSyntax rowBinaryExpression)
            {
                if (rowBinaryExpression.Left is MemberAccessExpressionSyntax memberAccessExpression && (memberAccessExpression.ToString() == "cell.Row" || memberAccessExpression.ToString() == "cell.Column"))
                {
                    row = rowBinaryExpression.Right.ToString();
                    isRelative = true;
                }
            }
            if (!int.TryParse(row, out var rowValue)) return false;
            position = rowValue;
            return true;
        }
    }
}
