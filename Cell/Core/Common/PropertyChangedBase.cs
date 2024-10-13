using System.ComponentModel;

namespace Cell.Core.Common
{
    /// <summary>
    /// Base class for all classes that need to notify when a property changes.
    /// 
    /// Object can subscribe handlers to the PropertyChanged event to be notified when a property changes.
    /// </summary>
    public class PropertyChangedBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Notifies that a property has changed.
        /// </summary>
        /// <param name="propertyName">The property that changed.</param>
        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Notifies that a property has changed.
        /// </summary>
        /// <param name="propertyName">The property that changed.</param>
        /// <param name="propertyName2">Another property that changed.</param>
        public void NotifyPropertyChanged(string propertyName, string propertyName2)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName2));
        }

        /// <summary>
        /// Notifies that a property has changed.
        /// </summary>
        /// <param name="propertyName">The property that changed.</param>
        /// <param name="propertyName2">Another property that changed.</param>
        /// <param name="propertyName3">Another property that changed.</param>
        public void NotifyPropertyChanged(string propertyName, string propertyName2, string propertyName3)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName2));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName3));
        }
    }
}
