using System.Windows;

namespace Cell.View
{
    public static class Utilities
    {
        public static bool TryGetSendersDataContext<T>(object sender, out T dataContext)
        {
            if (sender is FrameworkElement element)
            {
                if (element.DataContext is T cellViewModel)
                {
                    dataContext = cellViewModel;
                    return true;
                }
            }
#pragma warning disable CS8601 // Possible null reference assignment. The Try pattern requires that the out parameter is assigned to.
            dataContext = default;
#pragma warning restore CS8601 // Possible null reference assignment. The Try pattern requires that the out parameter is assigned to.
            return false;
        }
    }
}
