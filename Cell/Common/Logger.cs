
namespace Cell.Common
{
    internal static class Logger
    {
        private static int LogNumber = 0;
        private const int MaxRetainedLogs = 2000;
        private static readonly Queue<string> _logsQueue = new Queue<string>();

        public static IEnumerable<string> Logs { get; } = _logsQueue.ToArray();

        public static event Action<string> LogAdded;

        public static void Log(string message)
        {
            var log = $"{DateTime.Now:HH:mm:ss} {LogNumber++:0000}: {message}";
            Console.WriteLine(log);
            LogAdded?.Invoke(log);
            _logsQueue.Enqueue(log);
            if (_logsQueue.Count > MaxRetainedLogs)
            {
                _logsQueue.Dequeue();
            }
        }
    }
}
