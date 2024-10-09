namespace Cell.ViewModel.Application
{
    /// <summary>
    /// Represents an incremental result of a loading operation.
    /// </summary>
    public class LoadingProgressResult
    {
        private readonly Func<LoadingProgressResult>? _continuation;

        /// <summary>
        /// Creates a new instance of <see cref="LoadingProgressResult"/> that is marked as complete.
        /// </summary>
        /// <param name="success">Whether or not the load was actually successful.</param>
        /// <param name="message">The message to report the load progress to the user.</param>
        public LoadingProgressResult(bool success, string message)
        {
            IsComplete = true;
            Success = success;
            Message = message;
        }

        /// <summary>
        /// Creates a new instance of <see cref="LoadingProgressResult"/> that is not complete and includes a continuation.
        /// </summary>
        /// <param name="message">The message to display to the user.</param>
        /// <param name="continuation">The continuation that returns the next load progress.</param>
        public LoadingProgressResult(string message, Func<LoadingProgressResult> continuation)
        {
            IsComplete = false;
            Message = message;
            _continuation = continuation;
        }

        /// <summary>
        /// Whether or not the load is complete. This does not indicate success or failure, only that the load is done.
        /// </summary>
        public bool IsComplete { get; }

        /// <summary>
        /// The message to display to the user remarking the status of the load.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Whether or not the load was successful.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Continues the loading operation.
        /// </summary>
        /// <returns>The next load progress increment.</returns>
        /// <exception cref="InvalidOperationException">This load progress is complete and can not be continued.</exception>
        public LoadingProgressResult Continue()
        {
            if (_continuation == null) throw new InvalidOperationException("No continuation available");
            return _continuation();
        }
    }
}
