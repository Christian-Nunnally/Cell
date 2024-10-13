using System.Windows;

namespace Cell.Core.Persistence
{
    /// <summary>
    /// A clipboard that can store text using the real system clipboard.
    /// </summary>
    public class TextClipboard : ITextClipboard
    {
        /// <summary>
        /// Clears the contents of the clipboard.
        /// </summary>
        public void Clear() => Clipboard.Clear();

        /// <summary>
        /// Gets whether the clipboard has content stored in it.
        /// </summary>
        /// <returns>True if there is text stored in the clipboard.</returns>
        public bool ContainsText() => Clipboard.ContainsText();

        /// <summary>
        /// Gets the text currently stored in the clipboard.
        /// </summary>
        /// <returns>The text currently stored in the clipboard.</returns>
        public string GetText() => Clipboard.GetText();

        /// <summary>
        /// Stored the specified text in the clipboard.
        /// </summary>
        /// <param name="text">The text to put in the clipboard.</param>
        public void SetText(string text) => Clipboard.SetText(text);
    }
}
