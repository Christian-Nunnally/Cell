using Cell.Model;
using Cell.ViewModel.Application;
using System.Collections.ObjectModel;

namespace Cell.ViewModel.ToolWindow
{
    public class SheetManagerWindowViewModel : ToolWindowViewModel
    {
        public SheetManagerWindowViewModel()
        {
        }

        public string SelectedSheetName { get; set; } = string.Empty;

        public ObservableCollection<SheetModel> Sheets => ApplicationViewModel.Instance.SheetTracker.OrderedSheets;

        public override string ToolWindowTitle => "Sheet Manager";

        public void RefreshSheetsList()
        {
            NotifyPropertyChanged(nameof(Sheets));
        }
    }
}
