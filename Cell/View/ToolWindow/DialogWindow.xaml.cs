using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Collections.ObjectModel;

namespace Cell.View.ToolWindow
{
    public partial class DialogWindow : ResizableToolWindow, IDialogWindow
    {
        private readonly string _title;
        public DialogWindow(string title, string message, List<CommandViewModel> actions) : base(new DialogWindowViewModel())
        {
            InitializeComponent();
            _messageLabel.Text = message;
            _title = title;
            foreach (var action in actions)
            {
                DialogOptions.Add(action);
            }
        }

        public ObservableCollection<CommandViewModel> DialogOptions { get; set; } = [];

        public override double MinimumHeight => 200;

        public override double MinimumWidth => 280;

        public override string ToolWindowTitle => _title;

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
