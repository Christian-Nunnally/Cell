using Cell.ViewModel.ToolWindow;

namespace CellTest.TestUtilities
{
    public class TestToolWindowViewModel : ToolWindowViewModel
    {
        public bool IsAllowingClose { get; set; }
        
        public bool WasHandleBeingClosedCalled { get; private set; }

        public bool WasHandleBeingShownCalled { get; private set; }

        public override void HandleBeingClosed()
        {
            WasHandleBeingClosedCalled = true;
        }

        public override void HandleBeingShown()
        {
            WasHandleBeingShownCalled = true;
        }

        public override bool HandleCloseRequested()
        {
            return IsAllowingClose;
        }
    }
}
