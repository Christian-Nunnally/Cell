namespace Cell.Execution
{
    public struct CompileResult
    {
        public string? ExecutionResult { get; set; }

        public object? ReturnedObject { get; set; }

        public bool WasSuccess { get; set; }
    }
}
