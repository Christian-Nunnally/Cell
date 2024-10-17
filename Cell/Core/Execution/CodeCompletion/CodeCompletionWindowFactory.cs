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
        /// <param name="completionData">The completion data to put in the window.</param>
        /// <returns>A CompletionWindow, populated with results.</returns>
        public static CompletionWindow? Create(TextArea textArea, IEnumerable<ICompletionData> completionData)
        {
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
