using Cell.Common;
using Cell.Data;
using Cell.Model;
using System.ComponentModel;

namespace Cell.Execution
{
    /// <summary>
    /// ISubscriber's can use this class to subscribe to cell text changes at a specific location, without worrying about what cell is at that location or how the value is actually changing.
    /// 
    /// If the same subscriber subscribes to the same location multiple times, the subscriber will only be notified once when the value changes, but the subscriptions are ref counted so they will have to unsubscribe the same number of times they subscribed.
    /// 
    /// This class is optimized and only subscribes to cells that are at locations being subscribed to.
    /// </summary>
    public class CellTextChangesAtLocationNotifier
    {
        private readonly CellTracker _cellTracker;
        private readonly List<string> _locationsThatNeedToBeTrackedIfCellsAreAddedThere = [];
        private readonly SubscriberNotifier _subscriberNotifier = new();
        public CellTextChangesAtLocationNotifier(CellTracker cellTracker)
        {
            _cellTracker = cellTracker;
            _cellTracker.CellAdded += CellAdded;
            _subscriberNotifier.NewChannelSubscribedTo += StartListeningToCellForTextPropertyChanges;
            _subscriberNotifier.LastChannelUnsubscribedFrom += StopListeningToCellForTextPropertyChanges;
        }

        public IEnumerable<string> GetLocationsSubscriberIsSubscribedTo(CellPopulateSubscriber subscriber)
        {
            return _subscriberNotifier.GetChannelsSubscriberIsSubscribedTo(subscriber);
        }

        public void SubscribeToUpdatesAtLocation(ISubscriber subscriber, string locationString)
        {
            _subscriberNotifier.SubscribeToChannel(subscriber, locationString);
        }

        public void UnsubscribeFromAllLocations(ISubscriber subscriber)
        {
            _subscriberNotifier.UnsubscribeFromAllChannels(subscriber);
        }

        private void CellAdded(CellModel addedCell)
        {
            var locationString = addedCell.GetUnqiueLocationString();
            if (_locationsThatNeedToBeTrackedIfCellsAreAddedThere.Contains(locationString))
            {
                StartListeningToCellForTextPropertyChanges(locationString);
                _locationsThatNeedToBeTrackedIfCellsAreAddedThere.Remove(locationString);
            }
        }

        private void CellRemoved(CellModel removedCell)
        {
            StopListeningToCellForTextPropertyChanges(removedCell.GetUnqiueLocationString());
        }

        private void StartListeningToCellForTextPropertyChanges(string locationString)
        {
            var (SheetName, Row, Column) = Utilities.GetLocationFromUnqiueLocationString(locationString);
            var cell = _cellTracker.GetCell(SheetName, Row, Column);
            if (cell is not null)
            {
                cell.PropertyChanged += TrackedCellPropertyChanged;
                // Ensure anyone already listening to this location is updated because now a cell exists here.
                _subscriberNotifier.NotifySubscribers(locationString);
            }
            else if (!_locationsThatNeedToBeTrackedIfCellsAreAddedThere.Contains(locationString)) _locationsThatNeedToBeTrackedIfCellsAreAddedThere.Add(locationString);
        }

        private void StopListeningToCellForTextPropertyChanges(string locationString)
        {
            var (SheetName, Row, Column) = Utilities.GetLocationFromUnqiueLocationString(locationString);
            var cell = _cellTracker.GetCell(SheetName, Row, Column);
            if (cell is not null) cell.PropertyChanged -= TrackedCellPropertyChanged;
        }

        private void TrackedCellPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var cell = (CellModel)sender!;
            if (e.PropertyName == nameof(CellModel.Text))
            {
                var locationString = cell.GetUnqiueLocationString();
                _subscriberNotifier.NotifySubscribers(locationString);
            }
            else if (e.PropertyName == nameof(CellModel.Row))
            {
                (sender as CellModel)!.PropertyChanged -= TrackedCellPropertyChanged;
            }
            else if (e.PropertyName == nameof(CellModel.Column))
            {
                (sender as CellModel)!.PropertyChanged -= TrackedCellPropertyChanged;
            }
            else if (e.PropertyName == nameof(CellModel.SheetName))
            {
                (sender as CellModel)!.PropertyChanged -= TrackedCellPropertyChanged;
            }
        }
    }
}
