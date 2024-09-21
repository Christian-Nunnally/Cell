namespace Cell.ViewModel.Application
{
    public class LoadingProgressResult
    {
        private readonly Func<LoadingProgressResult>? _continuation;
        public LoadingProgressResult(bool success, string message)
        {
            IsComplete = true;
            Success = success;
            Message = message;
        }

        public LoadingProgressResult(string message, Func<LoadingProgressResult> continuation)
        {
            IsComplete = false;
            Message = message;
            _continuation = continuation;
        }

        public bool IsComplete { get; }

        public string Message { get; }

        public bool Success { get; }

        public LoadingProgressResult Continue()
        {
            if (_continuation == null) throw new InvalidOperationException("No continuation available");
            return _continuation();
        }
    }
}
