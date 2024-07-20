namespace Cell.Exceptions
{
    [Serializable]
    internal class PluginFunctionSyntaxTransformException : Exception
    {
        public PluginFunctionSyntaxTransformException()
        {
        }

        public PluginFunctionSyntaxTransformException(string? message) : base(message)
        {
        }

        public PluginFunctionSyntaxTransformException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}