using Cell.Core.Execution.CodeCompletion;
using Cell.Model;
using System.Diagnostics;

namespace CellTest.Core.Execution
{
    public class CellFunctionCodeSuggestionGeneratorTests
    {
        private CellFunctionCodeSuggestionGenerator _testing;
        private readonly CellModel _contextCell;
        public CellFunctionCodeSuggestionGeneratorTests()
        {
            _contextCell = new CellModel();
            _testing = new CellFunctionCodeSuggestionGenerator([], _contextCell);
        }

        [Fact]
        public void BasicLaunchTest()
        {
        }

        [Fact]
        public void NoCode_CompletionDataCreated_ShowsTypesProvidedToFactory()
        {
            IEnumerable<string> usings = [];
            var code = "";
            _testing = new CellFunctionCodeSuggestionGenerator(usings, _contextCell);
            _testing.UpdateCode(code, new Dictionary<string, string>());

            var completionData = _testing.CreateCompletionData(code.Length);

            Assert.Single(completionData, x => x.Text == $"c");
        }

        [Fact]
        public void NoCode_CompletionDataCreated_TypesHaveDocumentation()
        {
            IEnumerable<string> usings = [];
            var code = "";
            _testing = new CellFunctionCodeSuggestionGenerator(usings, _contextCell);
            _testing.UpdateCode(code, new Dictionary<string, string>());

            var completionData = _testing.CreateCompletionData(code.Length);

            Assert.NotEmpty(completionData.First().Description.ToString()!);
        }

        [Fact]
        public void PluginContextVariableWithDotButNoUsingProvided_CompletionDataCreated_DoesNotNeedUsingsForSomeReason()
        {
            IEnumerable<string> usings = [];
            var code = "c.";
            _testing = new CellFunctionCodeSuggestionGenerator(usings, _contextCell);
            _testing.UpdateCode(code, new Dictionary<string, string>());

            var completionData = _testing.CreateCompletionData(code.Length);

            Assert.NotEmpty(completionData);
            Assert.True(completionData.Count > 1);
        }

        [Fact]
        public void PluginContextVariableWithDotEButNoUsingProvided_CompletionDataCreated_DoesNotNeedUsingsForSomeReason()
        {
            IEnumerable<string> usings = [];
            var code = "c.E.";
            _testing = new CellFunctionCodeSuggestionGenerator(usings, _contextCell);
            _testing.UpdateCode(code, new Dictionary<string, string>());

            var completionData = _testing.CreateCompletionData(code.Length);

            Assert.NotEmpty(completionData);
            Assert.True(completionData.Count > 1);
        }

        [Fact]
        public void PluginContextVariableWithDot_CompletionDataCreated_HasCorrectTypes()
        {
            IEnumerable<string> usings = ["Cell.Execution"];
            var code = "c.";
            _testing = new CellFunctionCodeSuggestionGenerator(usings, _contextCell);
            _testing.UpdateCode(code, new Dictionary<string, string>());

            var completionData = _testing.CreateCompletionData(code.Length);

            Assert.Single(completionData, x => x.Text == "E");
        }

        [Fact]
        public void PluginContextVariableWithDot_CompletionDataCreated_DoesNotContainPropertyAccessors()
        {
            IEnumerable<string> usings = [];
            var code = "c.";
            _testing = new CellFunctionCodeSuggestionGenerator(usings, _contextCell);
            _testing.UpdateCode(code, new Dictionary<string, string>());

            var completionData = _testing.CreateCompletionData(code.Length);

            Assert.Empty(completionData.Where(x => x.Text.Contains("get_")));
            Assert.Empty(completionData.Where(x => x.Text.Contains("get_")));
        }

        [Fact]
        public void PluginContextVariableWithDot_CompletionDataCreated_DoesNotContainConstructors()
        {
            IEnumerable<string> usings = [];
            var code = "c.";
            _testing = new CellFunctionCodeSuggestionGenerator(usings, _contextCell);
            _testing.UpdateCode(code, new Dictionary<string, string>());

            var completionData = _testing.CreateCompletionData(code.Length);

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
            _testing = new CellFunctionCodeSuggestionGenerator(usings, _contextCell);
            _testing.UpdateCode(code, new Dictionary<string, string>());

            var completionData = _testing.CreateCompletionData(code.Length);

            Assert.NotEmpty(completionData);
            Assert.Single(completionData, x => x.Text == nameof(CellStyleModel.BackgroundColor));
        }

        [Fact]
        public void CellReferenceToA1_CompletionDataCreated_ContainsCellStyleProperty()
        {
            IEnumerable<string> usings = ["Cell.Model"];
            var code = "A1.";
            _testing = new CellFunctionCodeSuggestionGenerator(usings, _contextCell);
            _testing.UpdateCode(code, new Dictionary<string, string>());

            var completionData = _testing.CreateCompletionData(code.Length);

            Assert.NotEmpty(completionData);
            Assert.Single(completionData, x => x.Text == nameof(CellModel.Style));
        }

        [Fact]
        public void CellReferenceToA1Text_CompletionDataCreated_ContainsStringProperties()
        {
            IEnumerable<string> usings = ["Cell.Model"];
            var code = "A1.Text.";
            _testing = new CellFunctionCodeSuggestionGenerator(usings, _contextCell);
            _testing.UpdateCode(code, new Dictionary<string, string>());

            var completionData = _testing.CreateCompletionData(code.Length);

            Assert.NotEmpty(completionData);
            Assert.Single(completionData, x => x.Text == "Length");
        }

        [Fact]
        public void Performance()
        {
            IEnumerable<string> usings = ["Cell.Model"];
            var code = "A1.Text.";

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _testing = new CellFunctionCodeSuggestionGenerator(usings, _contextCell);
            _testing.UpdateCode(code, new Dictionary<string, string>());
            for (int i = 0; i < 100; i++)
            {
                _testing.CreateCompletionData(code.Length);
            }
            stopwatch.Stop();
            Assert.True(stopwatch.Elapsed.TotalSeconds < 1);
        }
    }
}
