using Cell.Data;

namespace Cell.ViewModel.ToolWindow
{
    public class ExportWindowViewModel : ResizeableToolWindowViewModel
    {
        public string SheetNameToExport { get; set; } = SheetTracker.Instance.OrderedSheets.Select(x => x.Name).FirstOrDefault("");

        public IEnumerable<string> SheetNames => SheetTracker.Instance.OrderedSheets.Select(x => x.Name);

        public ExportWindowViewModel()
        {
        }
    }
}
