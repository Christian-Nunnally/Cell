using Cell.Common;
using Cell.Data;
using Cell.Model;
using System.Collections.ObjectModel;

namespace Cell.ViewModel.Application
{
    public class TitleBarSheetNavigationViewModel : PropertyChangedBase
    {
        private readonly SheetTracker _sheetTracker;
        private bool _isAddingSheet;
        private string _newSheetName = string.Empty;
        public TitleBarSheetNavigationViewModel(SheetTracker sheetTracker)
        {
            _sheetTracker = sheetTracker;
        }

        public bool IsAddingSheet
        {
            get => _isAddingSheet;
            set
            {
                _isAddingSheet = value;
                NotifyPropertyChanged(nameof(IsAddingSheet));
            }
        }

        public string NewSheetName
        {
            get => _newSheetName;
            set
            {
                _newSheetName = value;
                NotifyPropertyChanged(nameof(NewSheetName));
            }
        }

        public ObservableCollection<SheetModel> OrderedSheets => _sheetTracker.OrderedSheets;
    }
}
