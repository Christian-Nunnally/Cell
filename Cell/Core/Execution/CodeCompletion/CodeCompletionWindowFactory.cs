using Cell.Execution;
using Cell.Model;
using Cell.ViewModel.Execution;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;

namespace Cell.Core.Execution.CodeCompletion
{
    /// <summary>
    /// Factory for <see cref="CompletionWindow"/> instances.
    /// </summary>
    public static class CodeCompletionWindowFactory
    {
        /// <summary>
        /// Creates a new completion window for the given text area.
        /// </summary>
        /// <param name="textArea">The text area to complete the text for.</param>
        /// <returns></returns>
        public static CompletionWindow? Create(TextArea textArea, CellFunction function)
        {
            var outerContextVariables = new Dictionary<string, Type> { { "c", typeof(Context) }, { "cell", typeof(CellModel) } };
            // TODO: pull these from the function.
            var usings = new[] { "System", "Cell.Model"};
            var completionData = CodeCompletionFactory.CreateCompletionData(textArea.Document.Text, textArea.Caret.Offset, function, usings, outerContextVariables);
            var completionWindow = new CompletionWindow(textArea);
            var data = completionWindow.CompletionList.CompletionData;
            foreach (var item in completionData)
            {
                data.Add(item);
            }
            return completionWindow;
        }
    }
}
