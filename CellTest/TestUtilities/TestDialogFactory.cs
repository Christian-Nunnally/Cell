using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;

namespace CellTest.TestUtilities
{
    internal class TestDialogFactory : DialogFactoryBase
    {
        private readonly Stack<TestDialogWindowViewModel> _instances = new();

        public override DialogWindowViewModel Create(string title, string message, List<CommandViewModel> actions)
        {
            if (_instances.Count == 0)
            {
                return new TestDialogWindowViewModel();
            }
            var instance = _instances.Pop();
            instance.Actions = actions;
            if (instance.ExpectedTitle != string.Empty) Assert.Equal(instance.ExpectedTitle, title);
            if (instance.ExpectedMessage != string.Empty) Assert.Equal(instance.ExpectedMessage, message);
            return instance;
        }

        public override void ShowDialog(DialogWindowViewModel dialog)
        {
            if (dialog is not TestDialogWindowViewModel testViewModel)
            {
                Assert.Fail("Test dialog factory only works with test dialogs");
                return;
            }
            testViewModel.ShowDialogInstance();
        }

        public TestDialogWindowViewModel Expect(int selectedAction = -1)
        {
            var instance = new TestDialogWindowViewModel(selectedAction);
            _instances.Push(instance);
            return instance;
        }

        public void AssertAllDialogsShown()
        {
            Assert.Empty(_instances);
        }

        internal TestDialogWindowViewModel Expect(string expectedMessage, int selectedAction = -1)
        {
            var dialog = Expect(selectedAction);
            dialog.ExpectedMessage = expectedMessage;
            return dialog;
        }
    }
}
