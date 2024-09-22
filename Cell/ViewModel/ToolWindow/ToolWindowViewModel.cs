using Cell.Common;

namespace Cell.ViewModel.ToolWindow
{
    public class ToolWindowViewModel : PropertyChangedBase
    {
        public virtual string ToolWindowTitle { get; set; } = "<<not set>>";

        public virtual void HandleBeingClosed()
        {
        }

        public virtual void HandleBeingShown()
        {
        }
    }
}
