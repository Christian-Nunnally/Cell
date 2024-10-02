using Cell.Model;
using System.Collections;
using System.ComponentModel;

namespace Cell.ViewModel.Cells.Types
{
    /// <summary>
    /// A cell that displays a collection of items instead of a single line of text.
    /// </summary>
    public abstract class CollectionCellViewModel : CellViewModel
    {
        /// <summary>
        /// Creates a new instance of <see cref="CollectionCellViewModel"/>.
        /// </summary>
        /// <param name="model">The underlying model for this cell.</param>
        /// <param name="sheet">The sheet this cell is visible on.</param>
        public CollectionCellViewModel(CellModel model, SheetViewModel sheet) : base(model, sheet)
        {
            model.PropertyChanged += ModelPropertyChanged;
        }

        /// <summary>
        /// Gets the list of string that should be shown in this cells dropdown in the UI.
        /// </summary>
        public List<object> Collection { get; set; } = [];

        /// <summary>
        /// Gets the list of string that should be shown in this cells dropdown in the UI.
        /// </summary>
        public List<string> CollectionDisplayStrings => Collection.Select(x => x?.ToString() ?? "").ToList();

        /// <summary>
        /// Updates the collection of this <see cref="CollectionCellViewModel"/> with the given items.
        /// </summary>
        /// <param name="items">The items to populate the collection with.</param>
        protected virtual void UpdateCollection(object? items)
        {
            Collection.Clear();
            if (items is null) return;
            if (items is IEnumerable enumerableItems)
            {
                foreach (var item in enumerableItems)
                {
                    Collection.Add(item);
                }
            }
            else Collection.Add(items);
            NotifyPropertyChanged(nameof(Collection));
            NotifyPropertyChanged(nameof(CollectionDisplayStrings));
        }

        private void ModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not CellModel cell) return;
            if (e.PropertyName == nameof(CellModel.PopulateResult))
            {
                UpdateCollection(cell.PopulateResult);
            }
        }
    }
}
