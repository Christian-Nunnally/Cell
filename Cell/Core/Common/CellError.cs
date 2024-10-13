namespace Cell.Core.Common
{
    /// <summary>
    /// An exception that is thrown from the Cell application.
    /// </summary>
    [Serializable]
    public class CellError : Exception
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CellError"/> class.
        /// </summary>
        public CellError()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CellError"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public CellError(string? message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CellError"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public CellError(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
