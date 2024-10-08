﻿using Cell.Common;
using Cell.Execution;
using Cell.Model;
using Cell.ViewModel.Application;
using System.Windows.Input;

namespace Cell.ViewModel.Cells.Types
{
    public static class CheckboxCellModelExtensions
    {
        public static void Check(this CellModel model)
        {
            model.SetBooleanProperty(nameof(CheckboxCellViewModel.IsChecked), true);
        }

        public static void Check(this CellModel model, bool check)
        {
            model.SetBooleanProperty(nameof(CheckboxCellViewModel.IsChecked), check);
        }

        public static bool IsChecked(this CellModel model)
        {
            return model.GetBooleanProperty(nameof(CheckboxCellViewModel.IsChecked));
        }

        public static void Uncheck(this CellModel model)
        {
            model.SetBooleanProperty(nameof(CheckboxCellViewModel.IsChecked), false);
        }
    }

    public class CheckboxCellViewModel : CellViewModel
    {
        private ICommand? _checkboxCheckedCommand;
        public CheckboxCellViewModel(CellModel model, SheetViewModel sheet) : base(model, sheet)
        {
            model.PropertyChanged += ModelPropertyChanged;
        }

        public ICommand CheckboxCheckedCommand => _checkboxCheckedCommand ??= new RelayCommand(x => CheckboxChecked());

        public bool IsChecked
        {
            get => Model.GetBooleanProperty(nameof(IsChecked));
            set
            {
                var oldValue = IsChecked;
                if (oldValue == value) return;
                Model.SetBooleanProperty(nameof(IsChecked), value);
                NotifyPropertyChanged(nameof(IsChecked));
                ApplicationViewModel.Instance.CellTriggerManager.CellTriggered(Model, new EditContext(nameof(IsChecked), oldValue, value));
            }
        }

        public static void CheckboxChecked()
        {
        }

        private void ModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellModel.BooleanProperties))
            {
                NotifyPropertyChanged(nameof(IsChecked));
            }
        }
    }
}
