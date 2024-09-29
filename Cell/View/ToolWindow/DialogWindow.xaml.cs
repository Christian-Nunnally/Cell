using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows;

namespace Cell.View.ToolWindow
{
    public partial class DialogWindow : ResizableToolWindow, IDialogWindow
    {
        public DialogWindow(DialogWindowViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }

        public void ShowDialog()
        {
            ApplicationViewModel.Instance.ShowToolWindow(this);
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            RequestClose?.Invoke();
        }
    }
}
