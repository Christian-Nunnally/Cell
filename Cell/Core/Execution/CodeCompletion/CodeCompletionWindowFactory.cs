using Cell.Core.Persistence;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using Cell.Core.Execution.Functions;

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
        /// <param name="userCollectionLoader">The collection manager used to resolve collection references and types.</param>
        /// <returns>A CompletionWindow, populated with results.</returns>
        public static CompletionWindow? Create(TextArea textArea, UserCollectionLoader userCollectionLoader)
        {
            var outerContextVariables = CodeCompletionFactory.CreateStandardCellFunctionGlobalVariableTypeMap(userCollectionLoader.GenerateDataTypeForCollectionMap());
            var completionData = CodeCompletionFactory.CreateCompletionData(textArea.Document.Text, textArea.Caret.Offset, CellFunction.UsingNamespaces, outerContextVariables);
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
