
using Cell.Data;
using Cell.Model;
using System.Collections.ObjectModel;

namespace Cell.ViewModel.ToolWindow
{
    public class SheetManagerWindowViewModel : ResizeableToolWindowViewModel
    {
        public string SelectedSheetName { get; set; } = string.Empty;

        public ObservableCollection<SheetModel> Sheets => SheetTracker.Instance.OrderedSheets;

        public SheetManagerWindowViewModel()
        {
        }

        internal void RefreshSheetsList()
        {
            NotifyPropertyChanged(nameof(Sheets));
        }
    }
}