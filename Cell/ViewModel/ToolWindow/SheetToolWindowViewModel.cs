
using Cell.View.Cells;
using Cell.ViewModel.Cells;

namespace Cell.ViewModel.ToolWindow
{
    public class SheetToolWindowViewModel : ToolWindowViewModel
    {
        private SheetViewModel _sheetViewModel;

        /// <summary>
        /// Gets or sets the current sheet view model that is being displayed in the tool window.
        /// </summary>
        public SheetViewModel? SheetViewModel
        {
            get { return _sheetViewModel; }
            set
            {
                if (_sheetViewModel == value) return;
                _sheetViewModel = value;
                NotifyPropertyChanged(nameof(SheetViewModel));
            }
        }

        public SheetToolWindowViewModel()
        {
            
        }
    }
}
