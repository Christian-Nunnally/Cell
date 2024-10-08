﻿using Cell.Common;
using System.Text;

namespace Cell.ViewModel.ToolWindow
{
    public class LogWindowViewModel : ToolWindowViewModel
    {
        private readonly StringBuilder _logBufferBuilder = new();
        public string LogBuffer => _logBufferBuilder.ToString();

        public override string ToolWindowTitle => "Logs";

        public void ClearBuffer()
        {
            _logBufferBuilder.Clear();
            NotifyPropertyChanged(nameof(LogBuffer));
        }

        public override void HandleBeingClosed()
        {
            Logger.Instance.LogAdded -= AddLog;
            ClearBuffer();
        }

        public override double MinimumHeight => 100;

        public override double MinimumWidth => 100;

        public override double DefaultHeight => 400;

        public override double DefaultWidth => 400;

        public override void HandleBeingShown()
        {
            Logger.Instance.LogAdded += AddLog;
            foreach (var log in Logger.Instance.Logs.Take(100))
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
    }
}
