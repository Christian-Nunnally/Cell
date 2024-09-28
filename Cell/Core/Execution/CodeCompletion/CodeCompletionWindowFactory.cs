using Cell.Execution;
using Cell.Model;
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
        public static CompletionWindow? Create(TextArea textArea)
        {
            var outerContextVariables = new Dictionary<string, Type> { { "c", typeof(Context) }, { "cell", typeof(CellModel) } };
            var completionData = CodeCompletionFactory.CreateCompletionData(textArea.Document.Text, textArea.Caret.Offset, outerContextVariables);
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
