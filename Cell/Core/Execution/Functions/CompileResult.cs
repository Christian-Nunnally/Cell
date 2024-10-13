namespace Cell.Core.Execution
{
    /// <summary>
    /// A result of a compile or execution of code.
    /// </summary>
    public struct CompileResult
    {
        /// <summary>
        /// The string describing the result of the execution.
        /// </summary>
        public string? ExecutionResult { get; set; }

        /// <summary>
        /// The object returned by the code.
        /// </summary>
        public object? ReturnedObject { get; set; }

        /// <summary>
        /// Whether the code was successfully executed or the compilation was successful.
        /// </summary>
        public bool WasSuccess { get; set; }
    }
}
