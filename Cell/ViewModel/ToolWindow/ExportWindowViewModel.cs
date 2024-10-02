using Cell.ViewModel.Application;

namespace Cell.ViewModel.ToolWindow
{
    /// <summary>
    /// A tool window view model for exporting sheets.
    /// </summary>
    public class ExportWindowViewModel : ToolWindowViewModel
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ExportWindowViewModel"/>.
        /// </summary>
        public ExportWindowViewModel()
        {
        }

        /// <summary>
        /// Gets the default height of this tool window when it is shown.
        /// </summary>
        public override double DefaultHeight => 200;

        /// <summary>
        /// Gets the default width of this tool window when it is shown.
        /// </summary>
        public override double DefaultWidth => 200;

        /// <summary>
        /// Gets a list of all current sheet names.
        /// </summary>
        public IEnumerable<string> SheetNames => ApplicationViewModel.Instance.SheetTracker.OrderedSheets.Select(x => x.Name);

        /// <summary>
        /// Gets the name of the sheet to export.
        /// </summary>
        public string SheetNameToExport { get; set; } = ApplicationViewModel.Instance.SheetTracker.OrderedSheets.Select(x => x.Name).FirstOrDefault("");

        /// <summary>
        /// Gets the string displayed in top bar of this tool window.
        /// </summary>
        public override string ToolWindowTitle => "Export";
    }
}
