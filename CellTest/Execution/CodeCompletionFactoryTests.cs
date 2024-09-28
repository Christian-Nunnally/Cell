using Cell.Core.Execution.CodeCompletion;
using Cell.Execution;
using Cell.Model;

namespace CellTest.Execution
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
        public void PluginContextVariableWithDot_CompletionDataCreated_HasCorrectTypes()
        {
            IEnumerable<string> usings = [];
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

            Assert.Empty(completionData.Where(x => x.Content.ToString().Contains(".ctor")));
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
    }
}
