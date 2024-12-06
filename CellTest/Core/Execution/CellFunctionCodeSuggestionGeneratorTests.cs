using Cell.Core.Execution.CodeCompletion;
using Cell.Core.Execution.Functions;
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
            _testing = new CellFunctionCodeSuggestionGenerator(usings, _contextCell);

            var completionData = _testing.CreateCompletionData(0);

            Assert.Single(completionData, x => x.Text == $"c");
        }

        [Fact]
        public void NoCode_CompletionDataCreated_TypesHaveDocumentation()
        {
            IEnumerable<string> usings = [];
            _testing = new CellFunctionCodeSuggestionGenerator(usings, _contextCell);

            var completionData = _testing.CreateCompletionData(0);

            Assert.NotEmpty(completionData.First().Description.ToString()!);
        }

        [Fact]
        public void NoCode_CompletionDataCreated_VarKeywordSuggested()
        {
            IEnumerable<string> usings = [];
            _testing = new CellFunctionCodeSuggestionGenerator(usings, _contextCell);

            var completionData = _testing.CreateCompletionData(0);

            Assert.True(completionData.Count(x => x.Text == "var") == 1);
        }

        [Fact]
        public void NoCode_CompletionDataCreated_ControlKeywordsSuggested()
        {
            IEnumerable<string> usings = [];
            _testing = new CellFunctionCodeSuggestionGenerator(usings, _contextCell);

            var completionData = _testing.CreateCompletionData(0);

            Assert.True(completionData.Count(x => x.Text == "if") == 1);
            Assert.True(completionData.Count(x => x.Text == "switch") == 1);
            Assert.True(completionData.Count(x => x.Text == "while") == 1);
            Assert.True(completionData.Count(x => x.Text == "for") == 1);
            Assert.True(completionData.Count(x => x.Text == "foreach") == 1);
        }

        [Fact]
        public void NoCodeOnFunctionThatReturnsVoid_CompletionDataCreated_ReturnKeywordNotSuggested()
        {
            IEnumerable<string> usings = [];
            _testing = new CellFunctionCodeSuggestionGenerator(usings, _contextCell);

            var completionData = _testing.CreateCompletionData(0);

            Assert.True(completionData.Count(x => x.Text == "return") == 0);
        }

        [Fact]
        public void NoCodeOnFunctionThatReturnsObject_CompletionDataCreated_ReturnKeywordNotSuggested()
        {
            IEnumerable<string> usings = [];
            _testing = new CellFunctionCodeSuggestionGenerator(usings, _contextCell);
            _testing.UpdateCode(string.Empty, "object", new Dictionary<string, List<string>>());

            var completionData = _testing.CreateCompletionData(0);

            Assert.True(completionData.Count(x => x.Text == "return") == 1);
        }

        [Fact]
        public void PluginContextVariableWithDotButNoUsingProvided_CompletionDataCreated_DoesNotNeedUsingsForSomeReason()
        {
            IEnumerable<string> usings = [];
            var code = "c.";
            _testing = new CellFunctionCodeSuggestionGenerator(usings, _contextCell);
            _testing.UpdateCode(code, "void", new Dictionary<string, List<string>>());

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
            _testing.UpdateCode(code, "void", new Dictionary<string, List<string>>());

            var completionData = _testing.CreateCompletionData(code.Length);

            Assert.NotEmpty(completionData);
            Assert.True(completionData.Count > 1);
        }

        [Fact]
        public void PluginContextVariableWithDotEButNoUsingProvided_CompletionDataCreated_ContainsEditContextField()
        {
            IEnumerable<string> usings = [];
            var code = "c.E.";
            _testing = new CellFunctionCodeSuggestionGenerator(usings, _contextCell);
            _testing.UpdateCode(code, "void", new Dictionary<string, List<string>>());

            var completionData = _testing.CreateCompletionData(code.Length).Select(x => x.Text);

            Assert.Contains(nameof(EditContext.Reason), completionData);
        }

        [Fact]
        public void PluginContextVariableWithDot_CompletionDataCreated_HasCorrectTypes()
        {
            IEnumerable<string> usings = ["Cell.Execution"];
            var code = "c.";
            _testing = new CellFunctionCodeSuggestionGenerator(usings, _contextCell);
            _testing.UpdateCode(code, "void", new Dictionary<string, List<string>>());

            var completionData = _testing.CreateCompletionData(code.Length);

            Assert.Single(completionData, x => x.Text == "E");
        }

        [Fact]
        public void PluginContextVariableWithDot_CompletionDataCreated_DoesNotContainPropertyAccessors()
        {
            IEnumerable<string> usings = [];
            var code = "c.";
            _testing = new CellFunctionCodeSuggestionGenerator(usings, _contextCell);
            _testing.UpdateCode(code, "void", new Dictionary<string, List<string>>());

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
            _testing.UpdateCode(code, "void", new Dictionary<string, List<string>>());

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
            _testing.UpdateCode(code, "void", new Dictionary<string, List<string>>());

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
            _testing.UpdateCode(code, "void", new Dictionary<string, List<string>>());

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
            _testing.UpdateCode(code, "void", new Dictionary<string, List<string>>());

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
            _testing.UpdateCode(code, "void", new Dictionary<string, List<string>>());
            for (int i = 0; i < 100; i++)
            {
                _testing.CreateCompletionData(code.Length);
            }
            stopwatch.Stop();
            Assert.True(stopwatch.Elapsed.TotalSeconds < 1);
        }
    }
}
