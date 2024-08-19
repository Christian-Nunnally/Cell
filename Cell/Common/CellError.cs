namespace Cell.Common
{
    [Serializable]
    internal class CellError : Exception
    {
        public CellError()
        {
        }

        public CellError(string? message) : base(message)
        {
        }

        public CellError(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
