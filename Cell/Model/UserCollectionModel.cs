using Cell.Common;
using Cell.Model.Plugin;

namespace Cell.Model
{
    /// <summary>
    /// The persistence model for a user collection.
    /// </summary>
    public class UserCollectionModel : PropertyChangedBase
    {
        private string _name = string.Empty;
        private string _sortAndFilterFunctionName = string.Empty;

        /// <summary>
        /// The name of the <see cref="UserCollectionModel"/> this collection is a projection of.
        /// 
        /// This means that this collection will have exactly the same items as the collection with this name,
        /// with the exception that the items will be filtered and sorted with the function specified in <see cref="SortAndFilterFunctionName"/>.
        /// </summary>
        public string BasedOnCollectionName { get; set; } = string.Empty;

        /// <summary>
        /// The name of the <see cref="PluginModel"/> subclass that this collection contains.
        /// 
        /// If <see cref="BasedOnCollectionName"/> is set this is ignored because the type of the items in this collection is the same as the items in the collection with the name <see cref="BasedOnCollectionName"/>.
        /// </summary>
        public string ItemTypeName { get; set; } = string.Empty;

        /// <summary>
        /// Gets either the <see cref="ItemTypeName"/> or the <see cref="BasedOnCollectionName"/> depending on which one is set and being used.
        /// </summary>
        public string ItemTypeOrBasedOnCollectionName => BasedOnCollectionName == string.Empty ? ItemTypeName : BasedOnCollectionName;

        /// <summary>
        /// Gets or sets the name of the collection.
        /// </summary>
        public string Name
        {
            get => _name; 
            set
            {
                if (_name == value) return;
                _name = value;
                NotifyPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// Gets or sets the name of the function that is used to sort and filter the items in this collection.
        /// </summary>
        public string SortAndFilterFunctionName
        {
            get => _sortAndFilterFunctionName;
            set
            {
                if (_sortAndFilterFunctionName == value) return;
                _sortAndFilterFunctionName = value;
                NotifyPropertyChanged(nameof(SortAndFilterFunctionName));
            }
        }
    }
}
