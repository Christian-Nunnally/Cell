using System.ComponentModel;

namespace CellTest.TestUtilities
{
    internal class PropertyChangedTester
    {
        public PropertyChangedTester(INotifyPropertyChanged objectToTrack)
        {
            objectToTrack.PropertyChanged += OnPropertyChanged;
        }

        public List<string> Notifications = [];

        public void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) => Notifications.Add(e?.PropertyName ?? "");

        internal void AssertPropertyChanged(string propertyName)
        {
            Notifications.Single(x => x == propertyName);
        }
    }
}
