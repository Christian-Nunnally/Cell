using Cell.Model;
using Cell.Plugin;
using System.Windows.Input;

namespace Cell.ViewModel
{
    public class CheckboxCellViewModel : CellViewModel
    {
        public bool IsChecked
        {
            get => Model.GetBooleanProperty(nameof(IsChecked));
            set
            {
                var oldValue = IsChecked;
                if (oldValue == value) return;
                Model.SetBooleanProperty(nameof(IsChecked), value);
                OnPropertyChanged(nameof(IsChecked));
                Model.TriggerCellEdited(new EditContext(nameof(IsChecked), IsChecked, oldValue));
            }
        }

        private ICommand? _checkboxCheckedCommand;

        public CheckboxCellViewModel(CellModel model, SheetViewModel sheet) : base(model, sheet)
        {
            model.PropertyChanged += ModelPropertyChanged;
        }

        private void ModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellModel.BooleanProperties))
            {
                OnPropertyChanged(nameof(IsChecked));
            }
        }

        public ICommand CheckboxCheckedCommand => _checkboxCheckedCommand ??= new RelayCommand(x => CanExecute, x => CheckboxChecked());

        public static bool CanExecute => true;

        public static void CheckboxChecked()
        {
        }
    }

    public static class CheckboxCellModelExtensions
    {
        public static bool IsChecked(this CellModel model)
        {
            return model.GetBooleanProperty(nameof(CheckboxCellViewModel.IsChecked));
        }

        public static void Check(this CellModel model)
        {
            model.SetBooleanProperty(nameof(CheckboxCellViewModel.IsChecked), true);
        }

        public static void Check(this CellModel model, bool check)
        {
            model.SetBooleanProperty(nameof(CheckboxCellViewModel.IsChecked), check);
        }

        public static void Uncheck(this CellModel model)
        {
            model.SetBooleanProperty(nameof(CheckboxCellViewModel.IsChecked), false);
        }
    }
}