using Cell.Model;
using Cell.Model.Plugin;
using Cell.Persistence;
using Cell.Plugin;
using System.Collections.ObjectModel;

namespace Cell.ViewModel
{
    public class ListCellViewModel : CellViewModel
    {

        public ObservableCollection<PluginModel> ListItems { get; set; } = [];

        public ListCellViewModel(CellModel model, SheetViewModel sheetViewModel) : base(model, sheetViewModel)
        {
            CellPopulateManager.SubscribeToCollectionUpdates(this, CollectionName);
            UpdateList();
        }

        public string CollectionName
        {
            get => Model.GetStringProperty(nameof(CollectionName));
            set
            {
                CellPopulateManager.UnsubscribeFromCollectionUpdates(this, CollectionName);
                Model.SetStringProperty(nameof(CollectionName), value);
                UserCollectionLoader.CreateEmptyCollection(CollectionName);
                CellPopulateManager.SubscribeToCollectionUpdates(this, CollectionName);
                UpdateList();
                OnPropertyChanged(nameof(CollectionName));
            }
        }

        public string CollectionType
        {
            get => Model.GetStringProperty(nameof(CollectionType));
            set
            {
                Model.SetStringProperty(nameof(CollectionType), value);
                OnPropertyChanged(nameof(CollectionType));
            }
        }

        public string DisplayPath
        {
            get => Model.GetStringProperty(nameof(DisplayPath));
            set
            {
                Model.SetStringProperty(nameof(DisplayPath), value);
                OnPropertyChanged(nameof(DisplayPath));
            }
        }

        internal void UpdateList()
        {
            ListItems.Clear();
            foreach (var item in UserCollectionLoader.GetCollection(CollectionName))
            {
                ListItems.Add(item);
            }
        }
    }
}