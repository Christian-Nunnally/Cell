using Cell.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace Cell.Plugin.SyntaxRewriters
{
    public partial class CellReferenceSyntaxWalker : CSharpSyntaxWalker
    {
        public readonly List<CellLocation> LocationReferences = [];

        public override void Visit(SyntaxNode? node)
        {
            base.Visit(node);
            if (TryGetCellReferenceFromNode(node, out var sheetName, out var rowValue, out var columnValue, out var isRowRelative, out var isColumnRelative))
            {
                LocationReferences.Add(new CellLocation(sheetName, rowValue, columnValue, isRowRelative, isColumnRelative));
            }
        }

        public static bool TryGetCellReferenceFromNode(SyntaxNode? node, out string sheetName, out int rowValue, out int columnValue, out bool isRowRelative, out bool isColumnRelative)
        {
            sheetName = string.Empty;
            rowValue = 0;
            columnValue = 0;
            isColumnRelative = false;
            isRowRelative = false;

            if (!DoesNodeMatchCellReferenceSyntax(node, out var arguments)) return false;

            var sheetSyntax = arguments[0];
            var rowSyntax = arguments[1];
            var columnSyntax = arguments[2];

            sheetName = sheetSyntax.ToString();
            var row = rowSyntax.ToString();
            var column = columnSyntax.ToString();

            if (rowSyntax.Expression is BinaryExpressionSyntax rowBinaryExpression)
            {
                if (rowBinaryExpression.Left is MemberAccessExpressionSyntax memberAccessExpression && memberAccessExpression.ToString() == "cell.Row")
                {
                    row = rowBinaryExpression.Right.ToString();
                    isRowRelative = true;
                }
            }

            if (columnSyntax.Expression is BinaryExpressionSyntax columnBinaryExpression)
            {
                if (columnBinaryExpression.Left is MemberAccessExpressionSyntax columnMemberAccessExpression && columnMemberAccessExpression.ToString() == "cell.Column")
                {
                    column = columnBinaryExpression.Right.ToString();
                    isColumnRelative = true;
                }
            }

            if (!int.TryParse(row, out rowValue)) return false;
            if (!int.TryParse(column, out columnValue)) return false;
            if (sheetName != "cell" && sheetName.StartsWith('"') && sheetName.EndsWith('"')) sheetName = sheetName[1..^1];
            if (sheetName == "cell") sheetName = string.Empty;
            return true;
        }

        private static bool DoesNodeMatchCellReferenceSyntax(SyntaxNode? node, [MaybeNullWhen(false)] out SeparatedSyntaxList<ArgumentSyntax> arguments)
        {
            arguments = SyntaxFactory.SeparatedList<ArgumentSyntax>();
            if (node is not InvocationExpressionSyntax syntax) return false;
            if (syntax is null) return false;
            if (syntax.Expression is not MemberAccessExpressionSyntax memberAccessExpressionSyntax) return false;
            if (memberAccessExpressionSyntax.Name.Identifier.Text != "GetCell") return false;
            if (memberAccessExpressionSyntax.Expression is not IdentifierNameSyntax identifierName) return false;
            if (identifierName.Identifier.Text != "c") return false;
            if (syntax.ArgumentList.Arguments.Count != 3) return false;
            arguments = syntax.ArgumentList.Arguments;
            return true;
        }
    }
}

