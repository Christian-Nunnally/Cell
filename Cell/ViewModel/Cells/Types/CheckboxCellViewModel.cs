using Cell.Model;
using System.Windows.Input;

namespace Cell.ViewModel
{
    public class CheckboxCellViewModel(CellModel model, SheetViewModel sheetViewModel) : CellViewModel(model, sheetViewModel)
    {
        public bool IsChecked 
        {
            get => bool.TryParse(Model.Value, out var result) && result;
            set => Model.Value = value.ToString(); 
        }

        private ICommand? _checkboxCheckedCommand;
        public ICommand CheckboxCheckedCommand
        {
            get
            {
                return _checkboxCheckedCommand ??= new RelayCommand(x => CanExecute, x => CheckboxChecked());
            }
        }

        public static bool CanExecute => true;

        public static void CheckboxChecked()
        {
        }
    }
}