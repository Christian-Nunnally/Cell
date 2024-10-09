using Cell.Common;
using Cell.Data;
using Cell.Model;
using System.Collections.ObjectModel;

namespace Cell.ViewModel.Application
{
    /// <summary>
    /// A view model for the title bar sheet navigation, which allows the user to navigate between sheets from the title bar.
    /// </summary>
    public class TitleBarSheetNavigationViewModel : PropertyChangedBase
    {
        private readonly SheetTracker _sheetTracker;
        /// <summary>
        /// Creates a new instance of <see cref="TitleBarSheetNavigationViewModel"/>.
        /// </summary>
        /// <param name="sheetTracker">The sheet tracker to get sheets from.</param>
        public TitleBarSheetNavigationViewModel(SheetTracker sheetTracker)
        {
            _sheetTracker = sheetTracker;
        }

        /// <summary>
        /// Gets the user visible collection of sheets.
        /// </summary>
        public ObservableCollection<SheetModel> OrderedSheets => _sheetTracker.OrderedSheets;
    }
}
