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
                Model.SetBooleanProperty(nameof(IsChecked), value);
                OnPropertyChanged(nameof(IsChecked));
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

        public ICommand CheckboxCheckedCommand
        {
            get
            {
                return _checkboxCheckedCommand ??= new RelayCommand(x => CanExecute, x => CheckboxChecked());
            }
        }

        public static bool CanExecute => true;

        public void CheckboxChecked()
        {
            //IsChecked = !IsChecked;
            //CellTriggerManager.CellEdited(Model, new EditContext(nameof(CheckboxCellViewModel.IsChecked), IsChecked.ToString(), (!IsChecked).ToString()));
        }
    }

    public static class ClassCheckbocCellModelExtensions
    {
        public static bool IsChecked(this CellModel model)
        {
            return model.GetBooleanProperty(nameof(CheckboxCellViewModel.IsChecked));
        }

        public static void Check(this CellModel model)
        {
            model.SetBooleanProperty(nameof(CheckboxCellViewModel.IsChecked), true);
        }

        public static void Uncheck(this CellModel model)
        {
            model.SetBooleanProperty(nameof(CheckboxCellViewModel.IsChecked), false);
        }
    }
}