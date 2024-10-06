using Cell.View.Skin;
using FontAwesome.Sharp;
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
        private readonly string _displayText;
        /// <summary>
        /// Creates a new instance of <see cref="CodeCompletionData"/>.
        /// </summary>
        /// <param name="replacementString">The text inserted into the document when this completion data is 'entered'.</param>
        /// <param name="text">The text displayed in the completion list</param>
        /// <param name="description">The description displayed when this completion is highlighted.</param>
        /// <param name="icon">The icon displayed when this completion is highlighted.</param>
        public CodeCompletionData(string replacementString, string text, string description, IconChar icon)
        {
            Image = icon.ToImageSource(ColorConstants.ForegroundColorConstantBrush, 20);
            _displayText = text;
            Text = replacementString;
            Description = description;
        }

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
        public ImageSource? Image { get; }

        /// <summary>
        /// Controls how this completion data should be sorted in the completion window relative to other completion datas.
        /// </summary>
        public double Priority => 0;

        /// <summary>
        /// The text that will be inserted into the document when this completion data is selected.
        /// </summary>
        public string Text { get; private set; }

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
