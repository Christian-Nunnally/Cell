using Cell.Execution;
using Cell.Model;
using Cell.Model.Plugin;
using Cell.ViewModel.Application;
using System.Collections.ObjectModel;

namespace Cell.ViewModel.Cells.Types
{
    public class ListCellViewModel : CellViewModel, ISubscriber
    {
        private CollectionChangeNotifier _collectionChangedNotifier;

        public ListCellViewModel(CellModel model, SheetViewModel sheetViewModel) : base(model, sheetViewModel)
        {
            _collectionChangedNotifier = new CollectionChangeNotifier(sheetViewModel.UserCollectionLoader);
            if (CollectionName == string.Empty) return;
            _collectionChangedNotifier.SubscribeToCollectionUpdates(this, CollectionName);
            UpdateList();
        }

        public string CollectionName
        {
            get => Model.GetStringProperty(nameof(CollectionName));
            set
            {
                if (!string.IsNullOrEmpty(CollectionName)) _collectionChangedNotifier.SubscribeToCollectionUpdates(this, CollectionName);
                Model.SetStringProperty(nameof(CollectionName), value);
                if (!string.IsNullOrEmpty(CollectionName))
                {
                    _collectionChangedNotifier.SubscribeToCollectionUpdates(this, CollectionName);
                    UpdateList();
                }
                NotifyPropertyChanged(nameof(CollectionName));
            }
        }

        public IEnumerable<string> CollectionNames => ApplicationViewModel.Instance.UserCollectionLoader.CollectionNames;

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

        public void Action()
        {
            UpdateList();
        }

        internal void UpdateList()
        {
            ListItems.Clear();
            var collection = _sheetViewModel.UserCollectionLoader.GetCollection(CollectionName);
            if (collection == null) return;
            if (!string.IsNullOrEmpty(Model.PopulateFunctionName))
            {
                int i = 0;
                foreach (var item in collection.Items)
                {
                    var result = DynamicCellPluginExecutor.RunPopulate(_sheetViewModel.PluginFunctionLoader, new PluginContext(_sheetViewModel.CellTracker, _sheetViewModel.UserCollectionLoader, i++), Model);
                    if (result.ExecutionResult == null) continue;
                    ListItems.Add(result.ExecutionResult);
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
