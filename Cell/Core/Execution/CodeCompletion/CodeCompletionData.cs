using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System.Windows.Media;

namespace Cell.Core.Execution.CodeCompletion
{
    /// <summary>
    /// Has all of the information needed to auto-complete a code snippet.
    /// </summary>
    public class CodeCompletionData : ICompletionData
    {
        private string _displayText;

        /// <summary>
        /// The UI element that will be displayed in the completion window.
        /// </summary>
        public object Content => _displayText;

        /// <summary>
        /// The description of the completion data displayed in a tooltip when selected in the completion window.
        /// </summary>
        public object Description { get; }

        /// <summary>
        /// An icon that will be displayed in the completion window.
        /// </summary>
        public ImageSource? Image => null;
        /// <summary>
        /// Controls how this completion data should be sorted in the completion window relative to other completion datas.
        /// </summary>
        public double Priority => 0;

        /// <summary>
        /// The text that will be inserted into the document when this completion data is selected.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Creates a new instance of <see cref="CodeCompletionData"/>.
        /// </summary>
        /// <param name="replacementString">The text that will be inserted into the document when this completion data is selected.</param>
        /// <param name="text">The text that will be displayed in the completion list</param>
        /// <param name="description">The description that displayed when this completion is highlighted.</param>
        public CodeCompletionData(string replacementString, string text, string description)
        {
            _displayText = text;
            Text = replacementString;
            Description = description;
        }

        /// <summary>
        /// Inserts the text data into the document.
        /// </summary>
        /// <param name="textArea">The text area object to insert in to.</param>
        /// <param name="completionSegment">The segment to replace in to.</param>
        /// <param name="insertionRequestEventArgs">The event argument associated with this request.</param>
        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }
    }
}
