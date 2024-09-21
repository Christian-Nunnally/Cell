using Cell.ViewModel.Application;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Cell.View.ToolWindow
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
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

        public Action? RequestClose { get; set; }

        public double GetMinimumHeight() => 200;

        public double GetMinimumWidth() => 280;

        public string GetTitle() => _title;

        public List<CommandViewModel> GetToolBarCommands() => [];

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

        public void HandleBeingClosed()
        {
        }

        public void HandleBeingShown()
        {
        }
    }
}
