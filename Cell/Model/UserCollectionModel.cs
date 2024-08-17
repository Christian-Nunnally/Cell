using Cell.Common;
using Cell.Persistence;

namespace Cell.Model
{
    public class UserCollectionModel : PropertyChangedBase
    {
        public string Name
        {
            get => name; set
            {
                if (name == value) return;
                name = value;
                NotifyPropertyChanged(nameof(Name));
            }
        }

        public string BasedOnCollectionName { get; set; } = string.Empty;

        public string SortAndFilterFunctionName
        {
            get { return _sortAndFilterFunctionName; }
            set
            {
                if (value == null) return;
                if (_sortAndFilterFunctionName == value) return;
                _sortAndFilterFunctionName = value;
                if (!string.IsNullOrEmpty(_sortAndFilterFunctionName) && PluginFunctionLoader.TryGetFunction("object", _sortAndFilterFunctionName, out var function))
                {
                    var _ = function.CompiledMethod;
                }
                NotifyPropertyChanged(nameof(SortAndFilterFunctionName));
            }
        }
        private string _sortAndFilterFunctionName = string.Empty;
        private string name = string.Empty;

        public string ItemTypeName { get; set; } = string.Empty;

        public string ItemTypeOrBasedOnCollectionName => BasedOnCollectionName == string.Empty ? ItemTypeName : BasedOnCollectionName;

        public UserCollectionModel()
        {
        }

        public UserCollectionModel(string name, string type, string basedOnCollection)
        {
            Name = name;
            ItemTypeName = type;
            BasedOnCollectionName = basedOnCollection;
        }
    }
}
