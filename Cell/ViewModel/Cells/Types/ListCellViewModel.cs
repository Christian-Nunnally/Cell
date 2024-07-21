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
                UserCollectionLoader.GetOrCreateCollection(CollectionName);
                CellPopulateManager.SubscribeToCollectionUpdates(this, CollectionName);
                UpdateList();
                NotifyPropertyChanged(nameof(CollectionName));
            }
        }

        public string CollectionType
        {
            get => Model.GetStringProperty(nameof(CollectionType));
            set
            {
                Model.SetStringProperty(nameof(CollectionType), value);
                UserCollectionLoader.SaveCollectionType(CollectionName, value);
                NotifyPropertyChanged(nameof(CollectionType));
            }
        }

        internal void UpdateList()
        {
            ListItems.Clear();
            var collection = UserCollectionLoader.GetOrCreateCollection(CollectionName);
            if (collection == null) return;
            foreach (var item in collection.Items)
            {
                ListItems.Add(item);
            }
        }
    }

    public static class ListboxCellModelExtensions
    {
        public static bool IsCollection(this CellModel model, string collectionName)
        {
            return model.GetStringProperty(nameof(ListCellViewModel.CollectionName)) == collectionName;
        }

        public static string GetCollectionType(this CellModel model)
        {
            return model.GetStringProperty(nameof(ListCellViewModel.CollectionType));
        }
    }
}