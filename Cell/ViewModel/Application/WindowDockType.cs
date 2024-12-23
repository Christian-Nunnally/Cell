
using System.Windows.Controls;

namespace Cell.ViewModel.Application
{
    /// <summary>
    /// An enumeration of the different ways a panel can be docked inside another panel.
    /// </summary>
    public enum WindowDockType
    {
        /// <summary>
        /// The content is docked to the right side of the host.
        /// </summary>
        DockedRight,
        
        /// <summary>
        /// The content is docked to the left side of the host.
        /// </summary>
        DockedLeft,

        /// <summary>
        /// The content is docked to the top or bottom of the host.
        /// </summary>
        DockedTop,

        /// <summary>
        /// The content is docked to the bottom of the host.
        /// </summary>
        DockedBottom,

        /// <summary>
        /// The content is floating in the host.
        /// </summary>
        Floating,
    }

    public static class WindowDockTypeExtensions
    {
        public static Dock ToDock(this WindowDockType dockType)
        {
            return dockType switch
            {
                WindowDockType.DockedRight => Dock.Right,
                WindowDockType.DockedLeft => Dock.Left,
                WindowDockType.DockedTop => Dock.Top,
                WindowDockType.DockedBottom => Dock.Bottom,
                _ => Dock.Left,
            };
        }

        public static WindowDockType ToWindowDockType(Dock dock)
        {
            return dock switch
            {
                Dock.Right => WindowDockType.DockedRight,
                Dock.Left => WindowDockType.DockedLeft,
                Dock.Top => WindowDockType.DockedTop,
                Dock.Bottom => WindowDockType.DockedBottom,
                _ => WindowDockType.DockedLeft,
            };
        }
    }
}
