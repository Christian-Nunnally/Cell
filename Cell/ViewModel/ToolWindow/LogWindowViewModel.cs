using Cell.Common;
using System.Text;

namespace Cell.ViewModel.ToolWindow
{
    public class LogWindowViewModel : PropertyChangedBase
    {
        private readonly StringBuilder _logBufferBuilder = new();
        public LogWindowViewModel()
        {
            Logger.Instance.LogAdded += AddLog;
            foreach (var log in Logger.Instance.Logs.Take(100))
            {
                AddLog(log);
            }
        }

        public string LogBuffer => _logBufferBuilder.ToString();

        public void ClearBuffer()
        {
            _logBufferBuilder.Clear();
            NotifyPropertyChanged(nameof(LogBuffer));
        }

        private void AddLog(string log)
        {
            _logBufferBuilder.Insert(0, log + "\n");
            if (_logBufferBuilder.Length > 10000)
            {
                _logBufferBuilder.Remove(_logBufferBuilder.Length - 1000, 1000);
            }
            NotifyPropertyChanged(nameof(LogBuffer));
        }
    }
}
