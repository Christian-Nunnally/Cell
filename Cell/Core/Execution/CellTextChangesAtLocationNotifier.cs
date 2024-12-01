using Cell.Core.Common;
using Cell.Core.Data.Tracker;
using Cell.Model;
using System.ComponentModel;

namespace Cell.Core.Execution
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
        /// <summary>
        /// Creates a new instance of <see cref="CellTextChangesAtLocationNotifier"/>.
        /// </summary>
        /// <param name="cellTracker">The cell tracker to track the cells of.</param>
        public CellTextChangesAtLocationNotifier(CellTracker cellTracker)
        {
            _cellTracker = cellTracker;
            _cellTracker.CellAdded += CellAdded;
            _subscriberNotifier.NewChannelSubscribedTo += StartListeningToCellForTextPropertyChanges;
            _subscriberNotifier.LastChannelUnsubscribedFrom += StopListeningToCellForTextPropertyChanges;
        }

        /// <summary>
        /// Gets or sets whether subscribers should be notified when a cell is added at a location they are subscribed to. Essentially disables this notifier.
        /// </summary>
        public bool NotifyWhenCellIsAdded { get; set; } = true;

        /// <summary>
        /// Gets all of the locations that a subscriber is subscribed to.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        /// <returns>A list of location strings.</returns>
        public IEnumerable<string> GetLocationsSubscriberIsSubscribedTo(CellPopulateSubscriber subscriber)
        {
            return _subscriberNotifier.GetChannelsSubscriberIsSubscribedTo(subscriber);
        }

        /// <summary>
        /// Subscribes the given subscriber to the given location. After subscribing, the subscriber's Action will be invoked when NotifySubscribers is called with the given location.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        /// <param name="locationString">The location string.</param>
        public void SubscribeToUpdatesAtLocation(ISubscriber subscriber, string locationString)
        {
            _subscriberNotifier.SubscribeToChannel(subscriber, locationString);
        }

        /// <summary>
        /// Unsubscribes the given subscriber from all locations it has previously subscribed to.
        /// </summary>
        /// <param name="subscriber">The subscriber to unsubscribe.</param>
        public void UnsubscribeFromAllLocations(ISubscriber subscriber)
        {
            _subscriberNotifier.UnsubscribeFromAllChannels(subscriber);
        }

        private void CellAdded(CellModel addedCell)
        {
            var locationString = addedCell.Location.LocationString;
            if (_locationsThatNeedToBeTrackedIfCellsAreAddedThere.Contains(locationString))
            {
                StartListeningToCellForTextPropertyChanges(locationString);
                _locationsThatNeedToBeTrackedIfCellsAreAddedThere.Remove(locationString);
            }
        }

        private void CellLocationChanged(object? sender, PropertyChangedEventArgs e)
        {
            var cellModel = _cellTracker.GetCell((sender as CellLocationModel)!)!;
            cellModel.PropertyChanged -= TrackedCellPropertyChanged;
            cellModel.Location.PropertyChanged -= CellLocationChanged;
        }

        private void CellRemoved(CellModel removedCell)
        {
            StopListeningToCellForTextPropertyChanges(removedCell.Location.LocationString);
        }

        private void StartListeningToCellForTextPropertyChanges(string locationString)
        {
            var (SheetName, Row, Column) = Utilities.GetLocationFromUnqiueLocationString(locationString);
            var cell = _cellTracker.GetCell(SheetName, Row, Column);
            if (cell is not null)
            {
                cell.PropertyChanged += TrackedCellPropertyChanged;
                cell.Location.PropertyChanged += CellLocationChanged;
                // Ensure anyone already listening to this location is updated because now a cell exists here.
                //if (NotifyWhenCellIsAdded) _subscriberNotifier.NotifySubscribers(locationString);
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
            if (e.PropertyName == nameof(CellModel.Text) || e.PropertyName == nameof(CellModel.Index))
            {
                var locationString = cell.Location.LocationString;
                _subscriberNotifier.NotifySubscribers(locationString);
            }
        }
    }
}
