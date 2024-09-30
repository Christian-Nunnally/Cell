using Cell.Common;
using Cell.ViewModel.Application;

namespace Cell.ViewModel.ToolWindow
{
    public class ToolWindowViewModel : PropertyChangedBase
    {
        public virtual double DefaultHeight { get; } = 200;

        public virtual double DefaultWidth { get; } = 200;

        public virtual double MinimumHeight => DefaultHeight;

        public virtual double MinimumWidth => DefaultWidth;

        public Action? RequestClose { get; set; }

        public virtual List<CommandViewModel> ToolBarCommands => [];

        /// <summary>
        /// Gets the string displayed in top bar of this tool window.
        /// </summary>
        public virtual string ToolWindowTitle { get; set; } = "<<not set>>";

        public virtual void HandleBeingClosed()
        {
        }

        public virtual void HandleBeingShown()
        {
        }

        public virtual bool HandleCloseRequested() => true;
    }
}
