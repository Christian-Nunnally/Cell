using Cell.Common;
using Cell.Data;
using Cell.Model.Plugin;
using Cell.Persistence;
using Cell.View.ToolWindow;
using Cell.ViewModel.Application;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Cell.ViewModel.ToolWindow
{
    public class CollectionManagerWindowViewModel : ResizeableToolWindowViewModel
    {
        private readonly JsonSerializerOptions _jsonDeserializerOptions = new()
        {
            WriteIndented = true
        };
        private UserCollection? selectedCollection;
        private PluginModel? selectedItem;
        private string selectedItemSerialized = string.Empty;
        public CollectionManagerWindowViewModel(UserCollectionLoader userCollectionLoader)
        {
            _userCollectionLoader = userCollectionLoader;
        }

        private readonly UserCollectionLoader _userCollectionLoader;

        public ObservableCollection<UserCollection> Collections => _userCollectionLoader.ObservableCollections;

        public ObservableCollection<PluginModel> ItemsInSelectedCollection { get; set; } = [];

        public string NewCollectionBaseName { get; set; } = string.Empty;

        public string NewCollectionName { get; set; } = string.Empty;

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
                        item.CopyPublicProperties(selectedItem, ["ID"]);
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

        private void SelectedCollectionOrderChanged(UserCollection collection)
        {
            RefreshItemsForSelectedCollection();
        }

        internal void OpenCreateCollectionWindow()
        {
            var createCollectionViewModel = new CreateCollectionWindowViewModel(_userCollectionLoader);
            var createCollectionWindow = new CreateCollectionWindow(createCollectionViewModel);
            ApplicationViewModel.Instance.ApplicationView.ShowToolWindow(createCollectionWindow);
        }
    }
}
