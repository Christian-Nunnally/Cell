namespace Cell.Core.Common
{
    /// <summary>
    /// Class that allows anyone to log messages to it.
    /// </summary>
    public class Logger
    {
        private const int MaxRetainedLogs = 1000;
        private readonly Queue<string> _logsQueue = new();
        private static Logger? _instance;
        /// <summary>
        /// Occurs when a new log is added.
        /// </summary>
        public event Action<string>? LogAdded;

        /// <summary>
        /// Gets the instance of the logger.
        /// </summary>
        public static Logger Instance => _instance ??= new Logger();

        /// <summary>
        /// Gets the list of logs.
        /// </summary>
        public IEnumerable<string> Logs => _logsQueue.AsEnumerable();

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Log(string message)
        {
            var log = $"[{DateTime.Now:HH:mm:ss}] {message}";
            Console.WriteLine(log);
            LogAdded?.Invoke(log);
            _logsQueue.Enqueue(log);
            if (_logsQueue.Count > MaxRetainedLogs)
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
