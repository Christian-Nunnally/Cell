using Cell.Common;
using Cell.Execution;
using Cell.Model;
using Cell.ViewModel.Application;
using System.Windows.Input;

namespace Cell.ViewModel.Cells.Types
{
    /// <summary>
    /// A view model for a cell that displays a button.
    /// </summary>
    public class ButtonCellViewModel : CellViewModel
    {
        private ICommand? _buttonClickedCommand;
        /// <summary>
        /// Creates a new instance of <see cref="ButtonCellViewModel"/>.
        /// </summary>
        /// <param name="model">The underlying model for this cell.</param>
        /// <param name="sheet">The sheet this cell is visible on.</param>
        public ButtonCellViewModel(CellModel model, SheetViewModel sheet) : base(model, sheet)
        {
        }

        /// <summary>
        /// Command binding that is run when the button of this button cell is pressed.
        /// </summary>
        public ICommand ButtonClickedCommand
        {
            get
            {
                return _buttonClickedCommand ??= new RelayCommand(x => ButtonClicked());
            }
        }

        private void ButtonClicked()
        {
            ApplicationViewModel.Instance.CellTriggerManager.CellTriggered(Model, new EditContext("Button"));
        }
    }
}
