using Cell.Core.Execution.CodeCompletion;
using Cell.Execution;

namespace CellTest.Execution
{
    public class CodeCompletionFactoryTests
    {
        [Fact]
        public void BasicLaunchTest()
        {
            var _ = CodeCompletionFactory.CreateCompletionData("", 0, []);
        }

        [Fact]
        public void NoCode_CompletionDataCreated_ShowsTypesProvidedToFactory()
        {
            var code = "";
            var outerContext = new Dictionary<string, Type>
            {
                { "c", typeof(Context) }
            };
            var completionData = CodeCompletionFactory.CreateCompletionData(code, code.Length, outerContext);

            Assert.Single(completionData, x => x.Text == $"{nameof(Context)} c");
        }

        [Fact]
        public void PluginContextVariableWithDot_CompletionDataCreated_HasCorrectTypes()
        {
            var code = "c.";
            var outerContext = new Dictionary<string, Type>
            {
                { "c", typeof(Context) }
            };
            var completionData = CodeCompletionFactory.CreateCompletionData(code, code.Length, outerContext);

            Assert.Single(completionData, x => x.Text == "E");
        }
    }
}
