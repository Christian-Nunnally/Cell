using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Cell.View.ToolWindow
{
    public partial class DialogWindow : UserControl, IResizableToolWindow, IDialogWindow
    {
        private readonly string _title;
        public DialogWindow(string title, string message, List<CommandViewModel> actions)
        {
            DataContext = this;
            InitializeComponent();
            _messageLabel.Text = message;
            _title = title;
            foreach (var action in actions)
            {
                DialogOptions.Add(action);
            }
        }

        public ObservableCollection<CommandViewModel> DialogOptions { get; set; } = [];

        public double MinimumHeight => 200;

        public double MinimumWidth => 280;

        public Action? RequestClose { get; set; }

        public List<CommandViewModel> ToolBarCommands => [];

        public string ToolWindowTitle => _title;

        public ToolWindowViewModel ToolViewModel => new DialogWindowViewModel();

        public void HandleBeingClosed()
        {
        }

        public void HandleBeingShown()
        {
        }

        public bool HandleCloseRequested()
        {
            return true;
        }

        public void ShowDialog()
        {
            ApplicationViewModel.Instance.ShowToolWindow(this);
        }

        private void ButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            RequestClose?.Invoke();
        }
    }
}
