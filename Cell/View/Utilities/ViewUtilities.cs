using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace Cell.View
{
    /// <summary>
    /// Utility methods for views.
    /// </summary>
    public static class ViewUtilities
    {
        /// <summary>
        /// Attempts to get the data context of the sender as the specified type from a routed events framework element.
        /// </summary>
        /// <typeparam name="T">The type of view model to get.</typeparam>
        /// <param name="sender">The sender of the event to get the data context from.</param>
        /// <param name="dataContext">The resulting datacontext, if it is the right type.</param>
        /// <returns>True if the datacontext exists and is the expected type.</returns>
        public static bool TryGetSendersDataContext<T>(object sender, [MaybeNullWhen(false)] out T dataContext)
        {
            if (sender is FrameworkElement element)
            {
                if (element.DataContext is T dataContextObject)
                {

                    dataContext = dataContextObject;
                    return true;
                }
            }
            dataContext = default;
            return false;
        }
    }
}
