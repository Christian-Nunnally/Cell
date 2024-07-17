using Cell.Model;
using Cell.Persistence;
using System.Collections.ObjectModel;

namespace Cell.ViewModel
{
    public class ListCellViewModel : CellViewModel
    {

        public ObservableCollection<string> ListItems { get; set; } = [];

        public ListCellViewModel(CellModel model, SheetViewModel sheetViewModel) : base(model, sheetViewModel)
        {
            UpdateListItems(model.Text);
        }

        public override string Text
        {
            get => base.Text;
            set
            {
                UpdateListItems(value);
                base.Text = value;
            }
        }

        private void UpdateListItems(string value)
        {
            ListItems.Clear();
            var splitValues = value.Split(",");
            foreach (var item in splitValues.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                ListItems.Add(item);
            }
        }

        public string CollectionName
        {
            get => Model.GetStringProperty(nameof(CollectionName));
            set
            {
                if (UserCollectionLoader.CreateEmptyCollection(value))
                {
                    Model.SetStringProperty(nameof(CollectionName), value);
                    OnPropertyChanged(nameof(CollectionName));
                }
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
    }
}