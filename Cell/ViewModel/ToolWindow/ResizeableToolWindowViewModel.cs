using Cell.Common;

namespace Cell.ViewModel.ToolWindow
{
    public class ResizeableToolWindowViewModel : PropertyChangedBase
    {
        private double userSetHeight;
        private double userSetWidth;
        public double UserSetHeight
        {
            get => userSetHeight; set
            {
                if (userSetHeight == value) return;
                userSetHeight = value;
                NotifyPropertyChanged(nameof(UserSetHeight));
            }
        }

        public double UserSetWidth
        {
            get => userSetWidth; set
            {
                if (userSetWidth == value) return;
                userSetWidth = value;
                NotifyPropertyChanged(nameof(UserSetWidth));
            }
        }
    }
}
