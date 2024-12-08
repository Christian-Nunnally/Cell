namespace Cell.Core.Common
{
    /// <summary>
    /// Class that allows anyone to log messages to it.
    /// </summary>
    public class Logger
    {
        private bool _enabled = true;
        private readonly int _maxRetainedLogs = 1000;
        private readonly Queue<string> _logsQueue = new();
        /// <summary>
        /// Occurs when a new log is added.
        /// </summary>
        public event Action<string>? LogAdded;

        /// <summary>
        /// Gets the list of logs.
        /// </summary>
        public IEnumerable<string> Logs => _logsQueue.AsEnumerable();

        /// <summary>
        /// A null logger.
        /// </summary>
        public readonly static Logger Null = new() { _enabled = false };

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Log(string message)
        {
            if (!_enabled) return;
            var log = $"[{DateTime.Now:HH:mm:ss}] {message}";
            Console.WriteLine(log);
            LogAdded?.Invoke(log);
            _logsQueue.Enqueue(log);
            if (_logsQueue.Count > _maxRetainedLogs)
            {
                _logsQueue.Dequeue();
            }
        }

        /// <summary>
        /// Clears all logs.
        /// </summary>
        public void Clear()
        {
            _logsQueue.Clear();
        }
    }
}
