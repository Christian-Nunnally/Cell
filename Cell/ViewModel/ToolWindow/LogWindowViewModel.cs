using Cell.Common;
using System.Collections.ObjectModel;
using System.Text;

namespace Cell.ViewModel.ToolWindow
{
    public class LogWindowViewModel : PropertyChangedBase
    {
        private double userSetHeight;
        private double userSetWidth;
        private StringBuilder _logBufferBuilder = new StringBuilder();
        public string LogBuffer => _logBufferBuilder.ToString();

        public LogWindowViewModel()
        {
            Logger.LogAdded += AddLog;
            foreach (var log in Logger.Logs)
            {
                AddLog(log);
            }
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

        internal void ClearBuffer()
        {
            _logBufferBuilder.Clear();
            NotifyPropertyChanged(nameof(LogBuffer));
        }

        public double UserSetHeight
        {
            get => userSetHeight; set
            {
                if (userSetHeight == value) return;
                userSetHeight = value;
                NotifyPropertyChanged(nameof(UserSetHeight));
            }
        }

        public double UserSetWidth
        {
            get => userSetWidth; set
            {
                if (userSetWidth == value) return;
                userSetWidth = value;
                NotifyPropertyChanged(nameof(UserSetWidth));
            }
        }
    }
}
