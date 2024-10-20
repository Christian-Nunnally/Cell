using Cell.Core.Execution.CodeCompletion;
using Cell.Model;
using Cell.Core.Execution.Functions;

namespace CellTest.Core.Execution
{
    public class CodeCompletionFactoryTests
    {
        [Fact]
        public void BasicLaunchTest()
        {
            var _ = CodeCompletionFactory.CreateCompletionData("", 0, [], []);
        }

        [Fact]
        public void NoCode_CompletionDataCreated_ShowsTypesProvidedToFactory()
        {
            IEnumerable<string> usings = [];
            var code = "";
            var outerContext = new Dictionary<string, Type>
            {
                { "c", typeof(Context) }
            };
            var completionData = CodeCompletionFactory.CreateCompletionData(code, code.Length, usings, outerContext);

            Assert.Single(completionData, x => x.Text == $"c");
        }

        [Fact]
        public void NoCode_CompletionDataCreated_TypesHaveDocumentation()
        {
            IEnumerable<string> usings = [];
            var code = "";
            var outerContext = new Dictionary<string, Type>
            {
                { "c", typeof(Context) }
            };
            var completionData = CodeCompletionFactory.CreateCompletionData(code, code.Length, usings, outerContext);

            Assert.NotEmpty(completionData.First().Description.ToString()!);
        }

        [Fact]
        public void PluginContextVariableWithDotButNoUsingProvided_CompletionDataCreated_DoesNotNeedUsingsForSomeReason()
        {
            IEnumerable<string> usings = [];
            var code = "c.";
            var outerContext = new Dictionary<string, Type>
            {
                { "c", typeof(Context) }
            };
            var completionData = CodeCompletionFactory.CreateCompletionData(code, code.Length, usings, outerContext);

            Assert.NotEmpty(completionData);
            Assert.True(completionData.Count > 1);
        }

        [Fact]
        public void PluginContextVariableWithDotEButNoUsingProvided_CompletionDataCreated_DoesNotNeedUsingsForSomeReason()
        {
            IEnumerable<string> usings = [];
            var code = "c.E.";
            var outerContext = new Dictionary<string, Type>
            {
                { "c", typeof(Context) }
            };
            var completionData = CodeCompletionFactory.CreateCompletionData(code, code.Length, usings, outerContext);

            Assert.NotEmpty(completionData);
            Assert.True(completionData.Count > 1);
        }

        [Fact]
        public void PluginContextVariableWithDot_CompletionDataCreated_HasCorrectTypes()
        {
            IEnumerable<string> usings = ["Cell.Execution"];
            var code = "c.";
            var outerContext = new Dictionary<string, Type>
            {
                { "c", typeof(Context) }
            };
            var completionData = CodeCompletionFactory.CreateCompletionData(code, code.Length, usings, outerContext);

            Assert.Single(completionData, x => x.Text == "E");
        }

        [Fact]
        public void PluginContextVariableWithDot_CompletionDataCreated_DoesNotContainPropertyAccessors()
        {
            IEnumerable<string> usings = [];
            var code = "c.";
            var outerContext = new Dictionary<string, Type>
            {
                { "c", typeof(Context) }
            };
            var completionData = CodeCompletionFactory.CreateCompletionData(code, code.Length, usings, outerContext);

            Assert.Empty(completionData.Where(x => x.Text.Contains("get_")));
            Assert.Empty(completionData.Where(x => x.Text.Contains("get_")));
        }

        [Fact]
        public void PluginContextVariableWithDot_CompletionDataCreated_DoesNotContainConstructors()
        {
            IEnumerable<string> usings = [];
            var code = "c.";
            var outerContext = new Dictionary<string, Type>
            {
                { "c", typeof(Context) }
            };
            var completionData = CodeCompletionFactory.CreateCompletionData(code, code.Length, usings, outerContext);

            Assert.Empty(completionData.Where(x => x.Content.ToString()!.Contains(".ctor")));
        }

        [Fact]
        public void GettingStyleOfCell_CompletionDataCreated_ContainsBackgroundColorProperty()
        {
            IEnumerable<string> usings = ["Cell.Model"];
            var code = "cell.Style.";
            var outerContext = new Dictionary<string, Type>
            {
                { "cell", typeof(CellModel) }
            };
            var completionData = CodeCompletionFactory.CreateCompletionData(code, code.Length, usings, outerContext);

            Assert.NotEmpty(completionData);
            Assert.Single(completionData, x => x.Text == nameof(CellStyleModel.BackgroundColor));
        }

        [Fact]
        public void CellReferenceToA1_CompletionDataCreated_ContainsCellStyleProperty()
        {
            IEnumerable<string> usings = ["Cell.Model"];
            var code = "A1.";

            var completionData = CodeCompletionFactory.CreateCompletionData(code, code.Length, usings, []);

            Assert.NotEmpty(completionData);
            Assert.Single(completionData, x => x.Text == nameof(CellModel.Style));
        }

        [Fact]
        public void CellReferenceToA1Text_CompletionDataCreated_ContainsStringProperties()
        {
            IEnumerable<string> usings = ["Cell.Model"];
            var code = "A1.Text.";

            var completionData = CodeCompletionFactory.CreateCompletionDataForCellFunction(code, code.Length, usings, new Dictionary<string, string>(), new CellModel());

            Assert.NotEmpty(completionData);
            Assert.Single(completionData, x => x.Text == "Length");
        }
    }
}
