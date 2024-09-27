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

        public virtual double MinimumHeight => DefaultHeight;

        public virtual double MinimumWidth => DefaultWidth;

        public virtual double DefaultHeight { get; } = 200;

        public virtual double DefaultWidth { get; } = 200;
    }
}
