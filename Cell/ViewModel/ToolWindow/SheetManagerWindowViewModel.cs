using Cell.Common;
using Cell.Model;
using Cell.ViewModel.Application;
using System.Collections.ObjectModel;

namespace Cell.ViewModel.ToolWindow
{
    public class SheetManagerWindowViewModel : PropertyChangedBase
    {
        public SheetManagerWindowViewModel()
        {
        }

        public string SelectedSheetName { get; set; } = string.Empty;

        public ObservableCollection<SheetModel> Sheets => ApplicationViewModel.Instance.SheetTracker.OrderedSheets;

        public void RefreshSheetsList()
        {
            NotifyPropertyChanged(nameof(Sheets));
        }
    }
}
