using Cell.Core.Common;
using Cell.ViewModel.Application;
using System.Windows.Controls;

namespace Cell.ViewModel.ToolWindow
{
    /// <summary>
    /// A view model for a tool window that can be shown in the main window.
    /// </summary>
    public class ToolWindowViewModel : PropertyChangedBase
    {
        private double x = -1;
        private double y = -1;

        /// <summary>
        /// Whether or not this tool window is currently docked in the main window or floating in its own border.
        /// </summary>
        public virtual bool IsDocked { get; set; } = false;

        /// <summary>
        /// What side of the main window this tool window should be docked to.
        /// </summary>
        public virtual Dock Dock { get; set; } = Dock.Top;

        /// <summary>
        /// Gets the default height of this tool window when it is shown.
        /// </summary>
        public virtual double DefaultHeight { get; } = 200;

        /// <summary>
        /// Gets the default width of this tool window when it is shown.
        /// </summary>
        public virtual double DefaultWidth { get; } = 200;

        /// <summary>
        /// Gets the X position of this tool window. Ignored if docked.
        /// </summary>
        public virtual double X
        {
            get => x; 
            set
            {
                if (value == y) return;
                x = value;
                NotifyPropertyChanged(nameof(X));
            }
        }

        /// <summary>
        /// Gets the Y position of this tool window. Ignored if docked.
        /// </summary>
        public virtual double Y
        {
            get => y; 
            set
            {
                if (value == y) return;
                y = value;
                NotifyPropertyChanged(nameof(Y));
            }
        }

        /// <summary>
        /// Gets the minimum height this tool window is allowed to be reized to.
        /// </summary>
        public virtual double MinimumHeight => DefaultHeight;

        /// <summary>
        /// Gets the default width of this tool window when it is shown.
        /// </summary>
        public virtual double MinimumWidth => DefaultWidth;

        /// <summary>
        /// Function that is set by whatever is 'showing' this tool window. This tool window can call it to close itself. 
        /// If this is called the host should call HandleCloseRequested, and if that returns true, the host should call 
        /// HandleBeingClosed and do what it needs to to close the window.
        /// </summary>
        public Action? RequestClose { get; set; }

        /// <summary>
        /// Provides a list of commands to display in the title bar of the tool window.
        /// </summary>
        public virtual List<CommandViewModel> ToolBarCommands => [];

        /// <summary>
        /// Gets the string displayed in top bar of this tool window.
        /// </summary>
        public virtual string ToolWindowTitle { get; set; } = "<<not set>>";

        /// <summary>
        /// Occurs when the tool window is really being closed.
        /// </summary>
        public virtual void HandleBeingClosed()
        {
        }

        /// <summary>
        /// Occurs when the tool window is being shown.
        /// </summary>
        public virtual void HandleBeingShown()
        {
        }

        /// <summary>
        /// Called when the tool window requested to be closed, either from the window itself or the host showing it, and gives the tool window a change to disallow the close.
        /// </summary>
        /// <returns>True if the tool window is allowing itself to be closed. If false, the caller should respect it and not call HandleBeingClosed or close the window.</returns>
        public virtual bool HandleCloseRequested() => true;
    }
}
