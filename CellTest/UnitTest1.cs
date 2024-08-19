
using Cell.Model;
using Microsoft.CodeAnalysis.CSharp;
using Cell.Execution.SyntaxWalkers;

namespace CellTest
{
    public class UnitTest1
    {
        [Fact]
        public void RelativeColumnAndRow()
        {
            //var cell = new CellModel
            //{
            //    Row = 1,
            //    Column = 1
            //};
            //var given = "c.GetUserList<testType>(\"list\")";
            //var expected = "c.GetUserList<testType>(\"newList\")";
            //var t = new CollectionReferenceRenameRewriter("list", "newList").Visit(SyntaxFactory.ParseExpression(given));
            //Assert.Equal(expected, t.ToFullString());
            ///var SyntaxTransformer = new SyntaxTransformer(new CollectionReferenceToCodeSyntaxRewriter(x => "testType", x => true), new CodeToCollectionReferenceSyntaxRewriter());
            //AssertRoundTrip(given, expected, SyntaxTransformer);
        }

        //[Fact]
        //public void RelativeColumnAndRow()
        //{
        //    var cell = new CellModel
        //    {
        //        Row = 1,
        //        Column = 1
        //    };
        //    var given = "c.GetCell(cell, cell.Row + 0, cell.Column + 0)";
        //    var expected = "A1";
        //    AssertRoundTrip(given, expected, new CodeToCellReferenceSyntaxRewriter(cell), new CellReferenceToCodeSyntaxRewriter(cell));
        //}

        //[Fact]
        //public void RelativeColumnAndRowBottomRight()
        //{
        //    var cell = new CellModel
        //    {
        //        Row = 1,
        //        Column = 1
        //    };
        //    var given = "c.GetCell(cell, cell.Row + 1, cell.Column + 1)";
        //    var expected = "B2";
        //    AssertRoundTrip(given, expected, new CodeToCellReferenceSyntaxRewriter(cell), new CellReferenceToCodeSyntaxRewriter(cell));
        //}

        //[Fact]
        //public void RelativeColumnAndRowTopLeft()
        //{
        //    var cell = new CellModel
        //    {
        //        Row = 2,
        //        Column = 2
        //    };
        //    var given = "c.GetCell(cell, cell.Row + -1, cell.Column + -1)";
        //    var expected = "A1";
        //    AssertRoundTrip(given, expected, new CodeToCellReferenceSyntaxRewriter(cell), new CellReferenceToCodeSyntaxRewriter(cell));
        //}

        //[Fact]
        //public void RelativeColumnTopLeft()
        //{
        //    var cell = new CellModel
        //    {
        //        Row = 2,
        //        Column = 2
        //    };
        //    var given = "c.GetCell(cell, 1, cell.Column + -1)";
        //    var expected = "R_A1";
        //    AssertRoundTrip(given, expected, new CodeToCellReferenceSyntaxRewriter(cell), new CellReferenceToCodeSyntaxRewriter(cell));
        //}

        //[Fact]
        //public void Range()
        //{
        //    var cell = new CellModel
        //    {
        //        Row = 2,
        //        Column = 2
        //    };
        //    var given = "c.GetCell(cell, cell.Row + -1, cell.Column + -1, cell.Row + -1, cell.Column + -1)";
        //    var expected = "A1_Range_A1";
        //    AssertRoundTrip(given, expected, new CodeToCellReferenceSyntaxRewriter(cell), new CellReferenceToCodeSyntaxRewriter(cell));
        //}

        private static void AssertRoundTrip(string given, string expected, SyntaxTransformer transformer)
        {
            var transformed = transformer.TransformTo(given);
            Assert.Equal(expected, transformed);
            transformed = transformer.TransformFrom(transformed);
            Assert.Equal(given, transformed);
        }
    }
}