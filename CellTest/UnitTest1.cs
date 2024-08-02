
using Cell.Model;
using Cell.Plugin.SyntaxRewriters;
using Microsoft.CodeAnalysis.CSharp;

namespace CellTest
{
    public class UnitTest1
    {
        [Fact]
        public void RelativeColumnAndRow()
        {
            var cell = new CellModel
            {
                Row = 1,
                Column = 1
            };
            var given = "c.GetCell(cell, cell.Row + 0, cell.Column + 0)";
            var expected = "A1";
            AssertRoundTrip(given, expected, new CodeToCellReferenceSyntaxRewriter(cell), new CellReferenceToCodeSyntaxRewriter(cell));
        }

        [Fact]
        public void RelativeColumnAndRowBottomRight()
        {
            var cell = new CellModel
            {
                Row = 1,
                Column = 1
            };
            var given = "c.GetCell(cell, cell.Row + 1, cell.Column + 1)";
            var expected = "B2";
            AssertRoundTrip(given, expected, new CodeToCellReferenceSyntaxRewriter(cell), new CellReferenceToCodeSyntaxRewriter(cell));
        }

        [Fact]
        public void RelativeColumnAndRowTopLeft()
        {
            var cell = new CellModel
            {
                Row = 2,
                Column = 2
            };
            var given = "c.GetCell(cell, cell.Row + -1, cell.Column + -1)";
            var expected = "A1";
            AssertRoundTrip(given, expected, new CodeToCellReferenceSyntaxRewriter(cell), new CellReferenceToCodeSyntaxRewriter(cell));
        }

        [Fact]
        public void RelativeColumnTopLeft()
        {
            var cell = new CellModel
            {
                Row = 2,
                Column = 2
            };
            var given = "c.GetCell(cell, 1, cell.Column + -1)";
            var expected = "R_A1";
            AssertRoundTrip(given, expected, new CodeToCellReferenceSyntaxRewriter(cell), new CellReferenceToCodeSyntaxRewriter(cell));
        }

        [Fact]
        public void Range()
        {
            var cell = new CellModel
            {
                Row = 2,
                Column = 2
            };
            var given = "c.GetCell(cell, cell.Row + -1, cell.Column + -1, cell.Row + -1, cell.Column + -1)";
            var expected = "A1_Range_A1";
            AssertRoundTrip(given, expected, new CodeToCellReferenceSyntaxRewriter(cell), new CellReferenceToCodeSyntaxRewriter(cell));
        }

        private static void AssertRoundTrip(string given, string expected, CSharpSyntaxRewriter converter, CSharpSyntaxRewriter convertBack)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(given);
            var root = syntaxTree.GetRoot();
            var newRoot = converter.Visit(root);
            var newCode = newRoot!.ToFullString();
            Assert.Equal(expected, newCode);
            newRoot = convertBack.Visit(newRoot);
            newCode = newRoot!.ToFullString();
            Assert.Equal(given, newCode);
        }
    }
}