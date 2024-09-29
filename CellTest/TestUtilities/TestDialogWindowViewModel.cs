using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;

namespace CellTest.TestUtilities
{
    public class TestDialogWindowViewModel : DialogWindowViewModel
    {
        private List<CommandViewModel> _actions = [];
        private static readonly Stack<TestDialogWindowViewModel> _instances = new();
        private readonly int _selectedAction;

        public string ExpectedTitle { get; set; } = string.Empty;

        public string ExpectedMessage { get; set; } = string.Empty;

        public bool WasShown { get; set; } = false;

        public TestDialogWindowViewModel(int selectedAction = -1) : base("", "", [])
        {
            DialogFactory.DialogFactoryFunction = GetInstance;
            DialogFactory.ShowDialogFunction = ShowDialog;
            _selectedAction = selectedAction;
            _instances.Push(this);
        }

        public static TestDialogWindowViewModel GetInstance(string title, string message, List<CommandViewModel> actions)
        {
            if (_instances.Count == 0)
            {
                return new TestDialogWindowViewModel();
            }
            var instance = _instances.Pop();
            instance._actions = actions;

            if (instance.ExpectedTitle != string.Empty) Assert.Equal(instance.ExpectedTitle, title);
            if (instance.ExpectedMessage != string.Empty) Assert.Equal(instance.ExpectedMessage, message);
            return instance;
        }

        public static void ShowDialog(DialogWindowViewModel dialogWindowViewModel)
        {
            if (dialogWindowViewModel is not TestDialogWindowViewModel viewModel) throw new InvalidOperationException("Dialog window is not a test dialog window.");
            viewModel.ShowDialogInstance();
        }

        public void ShowDialogInstance()
        {
            WasShown = true;
            if (_selectedAction < 0) return;
            if (_selectedAction >= _actions.Count) throw new IndexOutOfRangeException("Selected action index is out of range.");
            _actions[_selectedAction].Command.Execute(null);
        }
    }
}
