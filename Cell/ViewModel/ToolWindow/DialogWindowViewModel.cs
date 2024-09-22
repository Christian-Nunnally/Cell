namespace Cell.ViewModel.ToolWindow
{
    public class DialogWindowViewModel : ToolWindowViewModel
    {
        private string _title;
        public DialogWindowViewModel(string title)
        {
            _title = title;
        }

        public override string ToolWindowTitle => _title;
    }
}
