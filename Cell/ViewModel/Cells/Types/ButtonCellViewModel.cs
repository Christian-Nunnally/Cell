using Cell.Common;
using Cell.Execution;
using Cell.Model;
using System.Windows.Input;

namespace Cell.ViewModel.Cells.Types
{
    public class ButtonCellViewModel(CellModel model, SheetViewModel sheetViewModel) : CellViewModel(model, sheetViewModel)
    {
        private ICommand? _buttonClickedCommand;
        public static bool CanExecute => true;

        public ICommand ButtonClickedCommand
        {
            get
            {
                return _buttonClickedCommand ??= new RelayCommand(x => CanExecute, x => ButtonClicked());
            }
        }

        public void ButtonClicked()
        {
            CellTriggerManager.CellTriggered(Model, new EditContext("Button"));
        }
    }
}
