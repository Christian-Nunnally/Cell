using Cell.ViewModel.Application;

namespace CellTest.TestUtilities
{
    public class TestDialogWindow : IDialogWindow
    {
        private List<CommandViewModel> _actions = [];
        private static readonly Stack<TestDialogWindow> _instances = new();
        private readonly int _selectedAction;

        public string ExpectedTitle { get; set; } = string.Empty;

        public string ExpectedMessage { get; set; } = string.Empty;

        public bool WasShown { get; set; } = false;

        public TestDialogWindow(int selectedAction = -1)
        {
            DialogFactory.DialogFactoryFunction = GetInstance;
            _selectedAction = selectedAction;
            _instances.Push(this);
        }

        public static TestDialogWindow GetInstance(string title, string message, List<CommandViewModel> actions)
        {
            if (_instances.Count == 0)
            {
                return new TestDialogWindow();
            }
            var instance = _instances.Pop();
            instance._actions = actions;

            if (instance.ExpectedTitle != string.Empty) Assert.Equal(instance.ExpectedTitle, title);
            if (instance.ExpectedMessage != string.Empty) Assert.Equal(instance.ExpectedMessage, message);

            return instance;
        }

        public void ShowDialog()
        {
            WasShown = true;
            if (_selectedAction < 0) return;
            if (_selectedAction >= _actions.Count) throw new IndexOutOfRangeException("Selected action index is out of range.");
            _actions[_selectedAction].Command.Execute(null);
        }
    }
}
