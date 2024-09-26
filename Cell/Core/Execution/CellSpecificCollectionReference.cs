using Cell.Execution.References;
using Cell.Model;

namespace Cell.Execution
{
    /// <summary>
    /// Represents a reference to a collection from a specific cell.
    /// 
    /// `GetCollectionName()` Gets the name of the collection this reference is currently referring to.
    /// 
    /// Notifies `CollectionNameChanged` when `GetCollectionName()` might return a different value.
    /// </summary>
    public class CellSpecificCollectionReference
    {
        private readonly CellModel _cell;
        private readonly CellSpecificCollectionReferenceInvalidator _cellSpecificCollectionReferenceInvalidator;
        private readonly CellTextChangesAtLocationNotifier _getTextChangesAtLocationNotifier;
        private readonly PluginContext _pluginContext;
        private readonly ICollectionReference _underlyingGenericCollectionReference;
        public CellSpecificCollectionReference(CellModel cell, ICollectionReference collectionReference, CellTextChangesAtLocationNotifier getTextChangesAtLocationNotifier, PluginContext pluginContext)
        {
            _cell = cell;
            _pluginContext = pluginContext;
            _underlyingGenericCollectionReference = collectionReference;
            _getTextChangesAtLocationNotifier = getTextChangesAtLocationNotifier;
            _cellSpecificCollectionReferenceInvalidator = new CellSpecificCollectionReferenceInvalidator(this);
            collectionReference.LocationsThatWillInvalidateCollectionNameForCellHaveChanged += ListenToLocationsThatChangeTheReferencedCollection;
            ListenToLocationsThatChangeTheReferencedCollection();
        }

        public event Action<CellSpecificCollectionReference>? CollectionNameChanged;

        public CellModel Cell => _cell;

        public string GetCollectionName()
        {
            return _underlyingGenericCollectionReference.GetCollectionName(_cell, _pluginContext);
        }

        private void ListenToLocationsThatChangeTheReferencedCollection()
        {
            _getTextChangesAtLocationNotifier.UnsubscribeFromAllLocations(_cellSpecificCollectionReferenceInvalidator);
            var locationsToListenTo = _underlyingGenericCollectionReference.GetLocationsThatWillInvalidateCollectionNameForCell(_cell);
            foreach (var location in locationsToListenTo)
            {
                _getTextChangesAtLocationNotifier.SubscribeToUpdatesAtLocation(_cellSpecificCollectionReferenceInvalidator, location);
            }
        }

        private class CellSpecificCollectionReferenceInvalidator(CellSpecificCollectionReference cellSpecificCollectionReference) : ISubscriber
        {
            private readonly CellSpecificCollectionReference _cellSpecificCollectionReference = cellSpecificCollectionReference;
            public void Action()
            {
                _cellSpecificCollectionReference.CollectionNameChanged?.Invoke(_cellSpecificCollectionReference);
            }
        }
    }
}
