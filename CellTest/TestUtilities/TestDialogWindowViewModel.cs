using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;

namespace CellTest.TestUtilities
{
    public class TestDialogWindowViewModel : DialogWindowViewModel
    {
        public List<CommandViewModel> Actions = [];
        private readonly int _selectedAction;

        public string ExpectedTitle { get; set; } = string.Empty;

        public string ExpectedMessage { get; set; } = string.Empty;

        public bool WasShown { get; set; } = false;

        public TestDialogWindowViewModel(int selectedAction = -1) : base("", "", [])
        {
            _selectedAction = selectedAction;
        }

        public void ShowDialogInstance()
        {
            WasShown = true;
            if (_selectedAction < 0) return;
            if (_selectedAction >= Actions.Count) throw new IndexOutOfRangeException("Selected action index is out of range.");
            Actions[_selectedAction].Command.Execute(null);
        }
    }
}
