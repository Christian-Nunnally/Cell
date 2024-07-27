using Cell.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Cell.ViewModel
{
    public class DropdownCellViewModel : CellViewModel
    {
        public ObservableCollection<string> DropdownOptions { get; set; } = [];

        public DropdownCellViewModel(CellModel model, SheetViewModel sheetViewModel) : base(model, sheetViewModel)
        {
            UpdateDropdownItems(CommaSeperatedItems);
            model.PropertyChanged += ModelPropertyChanged;
        }

        private void ModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CommaSeperatedItems))
            {
                UpdateDropdownItems(CommaSeperatedItems);
            }
        }

        public string CommaSeperatedItems
        {
            get => Model.GetStringProperty(nameof(CommaSeperatedItems));
            set
            {
                Model.SetStringProperty(nameof(CommaSeperatedItems), value);
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