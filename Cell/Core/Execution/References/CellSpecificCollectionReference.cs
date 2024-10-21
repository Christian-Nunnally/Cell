using Cell.Core.Execution.Functions;
using Cell.Model;

namespace Cell.Core.Execution.References
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
        private readonly Context _pluginContext;
        private readonly ICollectionReference _underlyingGenericCollectionReference;
        /// <summary>
        /// Initializes a new instance of the <see cref="CellSpecificCollectionReference"/>.
        /// </summary>
        /// <param name="cell">The specific cell.</param>
        /// <param name="collectionReference">The reference to make more specific.</param>
        /// <param name="getTextChangesAtLocationNotifier">A notifier to subscribe to dependencies that update this reference.</param>
        /// <param name="pluginContext">The plugin context used when executing the resolve function.</param>
        public CellSpecificCollectionReference(CellModel cell, ICollectionReference collectionReference, CellTextChangesAtLocationNotifier getTextChangesAtLocationNotifier, Context pluginContext)
        {
            _cell = cell;
            _pluginContext = pluginContext;
            _underlyingGenericCollectionReference = collectionReference;
            _getTextChangesAtLocationNotifier = getTextChangesAtLocationNotifier;
            _cellSpecificCollectionReferenceInvalidator = new CellSpecificCollectionReferenceInvalidator(this);
            collectionReference.LocationsThatWillInvalidateCollectionNameForCellHaveChanged += ListenToLocationsThatChangeTheReferencedCollection;
            ListenToLocationsThatChangeTheReferencedCollection();
        }

        /// <summary>
        /// Occurs when the collection name might have changed.
        /// </summary>
        public event Action<CellSpecificCollectionReference>? CollectionNameChanged;

        /// <summary>
        /// The cell this reference is specified to.
        /// </summary>
        public CellModel Cell => _cell;

        /// <summary>
        /// Gets the name of the collection this reference is currently referring to.
        /// </summary>
        /// <returns>The resolved collection name.</returns>
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
