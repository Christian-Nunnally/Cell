using System.ComponentModel;

namespace CellTest.TestUtilities
{
    public class PropertyChangedTester
    {
        public PropertyChangedTester(INotifyPropertyChanged objectToTrack)
        {
            objectToTrack.PropertyChanged += OnPropertyChanged;
        }

        public List<string> Notifications = [];

        public void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) => Notifications.Add(e?.PropertyName ?? "");

        public void AssertPropertyChanged(string propertyName, int count = 1)
        {
            if (count != Notifications.Count(x => x == propertyName))
            {
                Assert.Fail("Expected " + count + " notifications for " + propertyName + " but got " + Notifications.Count(x => x == propertyName) + " notifications.");
            }
        }
    }
}
