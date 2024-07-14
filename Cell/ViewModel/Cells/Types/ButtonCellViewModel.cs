using Cell.Model;
using Cell.Plugin;
using System.Windows.Input;

namespace Cell.ViewModel
{
    public class ButtonCellViewModel(CellModel model, SheetViewModel sheetViewModel) : CellViewModel(model, sheetViewModel)
    {
        private ICommand? _buttonClickedCommand;

        public ICommand ButtonClickedCommand
        {
            get
            {
                return _buttonClickedCommand ??= new RelayCommand(x => CanExecute, x => ButtonClicked());
            }
        }
        public static bool CanExecute => true;

        public void ButtonClicked()
        {
            CellEditManager.CellEdited(Model, new EditContext("Button", "Down", "Up"));
        }
    }
}