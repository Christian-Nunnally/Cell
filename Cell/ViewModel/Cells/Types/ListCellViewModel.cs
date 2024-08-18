using Cell.Execution;
using Cell.Model;
using Cell.Model.Plugin;
using Cell.Persistence;
using Cell.ViewModel.Application;
using System.Collections.ObjectModel;

namespace Cell.ViewModel.Cells.Types
{
    public class ListCellViewModel : CellViewModel
    {
        public ListCellViewModel(CellModel model, SheetViewModel sheetViewModel) : base(model, sheetViewModel)
        {
            if (CollectionName == string.Empty) return;
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
                if (!string.IsNullOrEmpty(value))
                {
                    CellPopulateManager.SubscribeToCollectionUpdates(this, CollectionName);
                    UpdateList();
                }
                NotifyPropertyChanged(nameof(CollectionName));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Used in binding")]
        public IEnumerable<string> CollectionNames => UserCollectionLoader.CollectionNames;

        public ObservableCollection<object> ListItems { get; set; } = [];

        public string MaxItemsString
        {
            get => MaxNumberOfItems.ToString();
            set
            {
                if (int.TryParse(MaxItemsString, out int result))
                {
                    MaxNumberOfItems = result;
                }
            }
        }

        public int MaxNumberOfItems
        {
            get => (int)Model.GetNumericProperty(nameof(MaxNumberOfItems), 40);
            set
            {
                Model.SetNumericProperty(nameof(MaxNumberOfItems), value);
                NotifyPropertyChanged(nameof(MaxNumberOfItems));
                UpdateList();
            }
        }

        public ObservableCollection<string> PluginTypeNames { get; } = new ObservableCollection<string>(PluginModel.GetPluginDataTypeNames());

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
            var collection = UserCollectionLoader.GetCollection(CollectionName);
            if (collection == null) return;
            if (!string.IsNullOrEmpty(PopulateFunctionName))
            {
                int i = 0;
                foreach (var item in collection.Items)
                {
                    var result = DynamicCellPluginExecutor.RunPopulate(new PluginContext(ApplicationViewModel.Instance, i++), Model);
                    if (result.Result == null) continue;
                    ListItems.Add(result.Result);
                    if (ListItems.Count >= MaxNumberOfItems) break;
                }
            }
            else
            {
                foreach (var item in collection.Items)
                {
                    ListItems.Add(item);
                    if (ListItems.Count >= MaxNumberOfItems) break;
                }
            }
        }
    }
}
