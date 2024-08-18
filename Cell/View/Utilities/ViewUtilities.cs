using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace Cell.View
{
    public static class ViewUtilities
    {
        public static bool TryGetSendersDataContext<T>(object sender, [MaybeNullWhen(false)] out T dataContext)
        {
            if (sender is FrameworkElement element)
            {
                if (element.DataContext is T cellViewModel)
                {

                    dataContext = cellViewModel;
                    return true;
                }
            }
            dataContext = default;
            return false;
        }
    }
}
