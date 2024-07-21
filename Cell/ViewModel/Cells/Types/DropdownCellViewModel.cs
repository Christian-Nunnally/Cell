﻿using Cell.Model;
using System.Collections.ObjectModel;

namespace Cell.ViewModel
{
    public class DropdownCellViewModel : CellViewModel
    {
        public ObservableCollection<string> DropdownOptions { get; set; } = [];

        public DropdownCellViewModel(CellModel model, SheetViewModel sheetViewModel) : base(model, sheetViewModel)
        {
            UpdateDropdownItems(model.Text);
        }

        public string CommaSeperatedItems
        {
            get => Model.GetStringProperty(nameof(CommaSeperatedItems));
            set
            {
                Model.SetStringProperty(nameof(CommaSeperatedItems), value);
                UpdateDropdownItems(value);
                NotifyPropertyChanged(nameof(CommaSeperatedItems));
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