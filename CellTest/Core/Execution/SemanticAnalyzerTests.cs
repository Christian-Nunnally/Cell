using Cell.Core.Execution.CodeCompletion;

namespace CellTest.Core.Execution
{
    public class SemanticAnalyzerTests
    {
        [Fact]
        public void BasicLaunchTest()
        {
            var _ = new SemanticAnalyzer("", [], []);
        }

        [Fact]
        public void NoCode_NoTypesInMap()
        {
            var testing = new SemanticAnalyzer("", [], []);

            Assert.Empty(testing.NameToTypeMap);
        }

        [Fact]
        public void IntDeclarationInFunctionInClass_PopulatesNameToTypeMap()
        {
            var testing = new SemanticAnalyzer("class tempClass { public void temp() { int i = 0; } }", ["System"], []);

            Assert.Single(testing.NameToTypeMap, x => x.Key == "i" && x.Value.Name == "Int32");
        }

        [Fact]
        public void SimpleIntDeclarationUsingVar_PopulatesNameToTypeMap()
        {
            var testing = new SemanticAnalyzer("var i = 0;", [], []);

            Assert.Single(testing.NameToTypeMap, x => x.Key == "i" && x.Value.Name == "Int32");
        }
    }
}
