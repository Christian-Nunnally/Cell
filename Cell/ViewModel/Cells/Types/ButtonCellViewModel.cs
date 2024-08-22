using Cell.Common;
using Cell.Execution;
using Cell.Model;
using System.Windows.Input;

namespace Cell.ViewModel.Cells.Types
{
    public class ButtonCellViewModel(CellModel model, SheetViewModel sheetViewModel) : CellViewModel(model, sheetViewModel)
    {
        private ICommand? _buttonClickedCommand;

        public ICommand ButtonClickedCommand
        {
            get
            {
                return _buttonClickedCommand ??= new RelayCommand(x => ButtonClicked());
            }
        }

        public void ButtonClicked()
        {
            CellTriggerManager.CellTriggered(Model, new EditContext("Button"));
        }
    }
}
