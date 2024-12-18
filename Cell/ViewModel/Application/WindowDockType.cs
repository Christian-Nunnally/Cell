
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
}
