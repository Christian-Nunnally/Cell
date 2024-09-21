
namespace Cell.Common
{
    public class Logger
    {
        private const int MaxRetainedLogs = 1000;
        private readonly Queue<string> _logsQueue = new();
        private int LogNumber = 0;
        public event Action<string>? LogAdded;

        private static Logger? _instance;

        public static Logger Instance => _instance ??= new Logger();

        public IEnumerable<string> Logs =>_logsQueue.AsEnumerable();

        public void Log(string message)
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
