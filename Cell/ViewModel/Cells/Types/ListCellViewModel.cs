using Cell.Model;
using Cell.Persistence;
using Cell.Plugin;
using System.Collections.ObjectModel;

namespace Cell.ViewModel
{
    public class ListCellViewModel : CellViewModel
    {
        public ObservableCollection<object> ListItems { get; set; } = [];

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

        public string SelectedItem

        {
            get => Model.GetStringProperty(nameof(SelectedItem));
            set
            {
                Model.SetStringProperty(nameof(SelectedItem), value);
                NotifyPropertyChanged(nameof(SelectedItem));
            }
        }

        internal void UpdateList()
        {
            ListItems.Clear();
            var collection = UserCollectionLoader.GetOrCreateCollection(CollectionName);
            if (collection == null) return;
            if (!string.IsNullOrEmpty(PopulateFunctionName))
            {
                int i = 0;
                foreach (var item in collection.Items)
                {
                    var result = DynamicCellPluginExecutor.RunPopulate(new PluginContext(ApplicationViewModel.Instance, i++), Model);
                    ListItems.Add(result.Result);
                }
            }
            else
            {
                foreach (var item in collection.Items)
                {
                    ListItems.Add(item);
                }
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