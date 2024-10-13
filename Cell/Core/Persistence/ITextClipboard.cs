namespace Cell.Core.Persistence
{
    /// <summary>
    /// Interface for a text based clipboard that can store a single string.
    /// </summary>
    public interface ITextClipboard
    {
        /// <summary>
        /// Clears the contents of the clipboard.
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets whether the clipboard has content stored in it.
        /// </summary>
        /// <returns>True if there is text stored in the clipboard.</returns>
        bool ContainsText();

        /// <summary>
        /// Gets the text currently stored in the clipboard.
        /// </summary>
        /// <returns>The text currently stored in the clipboard.</returns>
        string GetText();

        /// <summary>
        /// Stored the specified text in the clipboard.
        /// </summary>
        /// <param name="text">The text to put in the clipboard.</param>
        void SetText(string text);
    }
}
