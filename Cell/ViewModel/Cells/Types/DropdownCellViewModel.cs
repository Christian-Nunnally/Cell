using Cell.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Cell.ViewModel
{
    public class DropdownCellViewModel : CellViewModel
    {
        public DropdownCellViewModel(CellModel model, SheetViewModel sheetViewModel) : base(model, sheetViewModel)
        {
            UpdateDropdownItems(CommaSeperatedItems);
            model.PropertyChanged += ModelPropertyChanged;
        }

        public string CommaSeperatedItems
        {
            get => Model.GetStringProperty(nameof(CommaSeperatedItems));
            set
            {
                Model.SetStringProperty(nameof(CommaSeperatedItems), value);
            }
        }

        public ObservableCollection<string> DropdownOptions { get; set; } = [];

        private void ModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CommaSeperatedItems))
            {
                UpdateDropdownItems(CommaSeperatedItems);
            }
        }

        private void UpdateDropdownItems(string value)
        {
            DropdownOptions.Clear();
            var splitValues = value.Split(",");
            foreach (var item in splitValues.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                DropdownOptions.Add(item);
            }
        }
    }
}
