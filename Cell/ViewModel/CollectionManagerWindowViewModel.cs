using Cell.Common;
using Cell.Model.Plugin;
using Cell.Persistence;
using Cell.Plugin;
using Cell.View.ToolWindow;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;

namespace Cell.ViewModel
{
    public class CollectionManagerWindowViewModel : PropertyChangedBase
    {
        private readonly JsonSerializerOptions _jsonDeserializerOptions = new()
        {
            WriteIndented = true
        };
        private bool isBaseOnCheckBoxChecked;
        private UserCollection? selectedCollection;
        private PluginModel? selectedItem;
        private string selectedItemSerialized = string.Empty;
        private double userSetHeight;
        private double userSetWidth;
        public CollectionManagerWindowViewModel(ObservableCollection<UserCollection> _collections)
        {
            Collections = _collections;
            CollectionBaseOptions = new ObservableCollection<string>(Collections.Select(x => x.Name))
            {
                "---"
            };
            SelectedItemType = PluginTypeNames.FirstOrDefault(string.Empty);
        }

        public ObservableCollection<string> CollectionBaseOptions { get; set; }

        public Visibility CollectionBaseSettingVisibility { get; private set; } = Visibility.Collapsed;

        public ObservableCollection<UserCollection> Collections { get; set; }

        public Visibility CollectionTypeSettingVisibility { get; private set; } = Visibility.Visible;

        public bool IsBaseOnCheckBoxChecked
        {
            get => isBaseOnCheckBoxChecked; set
            {
                isBaseOnCheckBoxChecked = value;
                CollectionBaseSettingVisibility = isBaseOnCheckBoxChecked ? Visibility.Visible : Visibility.Collapsed;
                CollectionTypeSettingVisibility = isBaseOnCheckBoxChecked ? Visibility.Collapsed : Visibility.Visible;
                NotifyPropertyChanged(nameof(CollectionBaseSettingVisibility));
                NotifyPropertyChanged(nameof(CollectionTypeSettingVisibility));
            }
        }

        public ObservableCollection<PluginModel> ItemsInSelectedCollection { get; set; } = [];

        public string NewCollectionBaseName { get; set; } = string.Empty;

        public string NewCollectionName { get; set; } = string.Empty;

        public ObservableCollection<string> PluginTypeNames { get; } = new ObservableCollection<string>(PluginModel.GetPluginDataTypeNames());

        public UserCollection? SelectedCollection
        {
            get => selectedCollection; set
            {
                if (selectedCollection == value) return;
                if (selectedCollection != null)
                {
                    selectedCollection.ItemAdded -= SelectedCollectionChanged;
                    selectedCollection.ItemRemoved -= SelectedCollectionChanged;
                    selectedCollection.ItemPropertyChanged -= SelectedCollectionChanged;
                    selectedCollection.OrderChanged -= SelectedCollectionOrderChanged;
                }

                selectedCollection = value;
                RefreshItemsForSelectedCollection();

                if (selectedCollection != null)
                {
                    selectedCollection.ItemAdded += SelectedCollectionChanged;
                    selectedCollection.ItemRemoved += SelectedCollectionChanged;
                    selectedCollection.ItemPropertyChanged += SelectedCollectionChanged;
                    selectedCollection.OrderChanged += SelectedCollectionOrderChanged;
                }
                NotifyPropertyChanged(nameof(SelectedCollection));
            }
        }

        private void SelectedCollectionOrderChanged(UserCollection collection)
        {
            RefreshItemsForSelectedCollection();
        }

        public PluginModel? SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;
                if (selectedItem != null)
                {
                    selectedItemSerialized = JsonSerializer.Serialize(selectedItem, _jsonDeserializerOptions);
                    NotifyPropertyChanged(nameof(SelectedItemSerialized));
                }
                else
                {
                    selectedItemSerialized = string.Empty;
                }
            }
        }

        public string SelectedItemSerialized
        {
            get => selectedItemSerialized;
            set
            {
                if (selectedItemSerialized == value) return;

                try
                {
                    var item = JsonSerializer.Deserialize<PluginModel>(value);
                    if (item != null && selectedItem != null)
                    {
                        item.CopyProperties(selectedItem, ["ID"]);
                        selectedItemSerialized = value;
                        NotifyPropertyChanged(nameof(SelectedItemSerialized));
                    }
                }
                catch (JsonException)
                {
                    DialogWindow.ShowDialog("Did not save", "Invalid json for item");
                }
            }
        }

        public string SelectedItemType { get; set; }

        public double UserSetHeight
        {
            get => userSetHeight; set
            {
                if (userSetHeight == value) return;
                userSetHeight = value;
                NotifyPropertyChanged(nameof(UserSetHeight));
            }
        }

        public double UserSetWidth
        {
            get => userSetWidth; set
            {
                if (userSetWidth == value) return;
                userSetWidth = value;
                NotifyPropertyChanged(nameof(UserSetWidth));
            }
        }

        internal void AddCurrentCollection()
        {
            var collectionName = NewCollectionName;
            if (string.IsNullOrEmpty(collectionName)) return;
            if (Collections.Any(x => x.Name == collectionName)) return;

            if (IsBaseOnCheckBoxChecked)
            {
                var basedOnCollection = NewCollectionBaseName;
                if (string.IsNullOrEmpty(basedOnCollection)) return;

                var baseCollection = Collections.FirstOrDefault(x => x.Name == basedOnCollection);
                if (baseCollection == null) return;
                
                UserCollectionLoader.CreateCollection(collectionName, baseCollection.Model.ItemTypeName, baseCollection.Name);
            }
            else
            {
                var collectionType = SelectedItemType;
                if (string.IsNullOrEmpty(collectionType)) return;

                UserCollectionLoader.CreateCollection(collectionName, collectionType, string.Empty);
            }
        }

        private void RefreshItemsForSelectedCollection()
        {
            ItemsInSelectedCollection.Clear();
            if (selectedCollection == null) return;
            foreach (var item in selectedCollection.Items)
            {
                ItemsInSelectedCollection.Add(item);
            }
        }

        private void SelectedCollectionChanged(UserCollection collection, PluginModel model)
        {
            RefreshItemsForSelectedCollection();
        }
    }
}
