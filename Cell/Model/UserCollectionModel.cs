using Cell.Common;

namespace Cell.Model
{
    public class UserCollectionModel : PropertyChangedBase
    {
        private string _sortAndFilterFunctionName = string.Empty;
        private string name = string.Empty;
        public UserCollectionModel()
        {
        }

        public UserCollectionModel(string name, string type, string basedOnCollection)
        {
            Name = name;
            ItemTypeName = type;
            BasedOnCollectionName = basedOnCollection;
        }

        public string BasedOnCollectionName { get; set; } = string.Empty;

        public string ItemTypeName { get; set; } = string.Empty;

        public string ItemTypeOrBasedOnCollectionName => BasedOnCollectionName == string.Empty ? ItemTypeName : BasedOnCollectionName;

        public string Name
        {
            get => name; set
            {
                if (name == value) return;
                name = value;
                NotifyPropertyChanged(nameof(Name));
            }
        }

        public string SortAndFilterFunctionName
        {
            get
            {
                return _sortAndFilterFunctionName;
            }
            set
            {
                if (_sortAndFilterFunctionName == value) return;
                _sortAndFilterFunctionName = value;
                NotifyPropertyChanged(nameof(SortAndFilterFunctionName));
            }
        }
    }
}
