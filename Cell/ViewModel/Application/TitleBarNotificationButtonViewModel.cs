
using Cell.Core.Common;
using Cell.ViewModel.ToolWindow;

namespace Cell.ViewModel.Application
{
    public class TitleBarNotificationButtonViewModel
    {
        private readonly Logger _notificationLogger;

        public TitleBarNotificationButtonViewModel(Logger notificationLogger)
        {
            _notificationLogger = notificationLogger;
        }

        public void ShowNotifcationsWindow()
        {
            var logWindowViewModel = new LogWindowViewModel(_notificationLogger, "Notifications");
            ApplicationViewModel.Instance.ShowToolWindow(logWindowViewModel, true);
        }
    }
}
