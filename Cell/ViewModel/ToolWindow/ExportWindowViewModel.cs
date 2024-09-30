﻿using Cell.ViewModel.Application;

namespace Cell.ViewModel.ToolWindow
{
    public class ExportWindowViewModel : ToolWindowViewModel
    {
        public ExportWindowViewModel()
        {
        }

        public override double DefaultHeight => 200;

        public override double DefaultWidth => 200;

        public IEnumerable<string> SheetNames => ApplicationViewModel.Instance.SheetTracker.OrderedSheets.Select(x => x.Name);

        public string SheetNameToExport { get; set; } = ApplicationViewModel.Instance.SheetTracker.OrderedSheets.Select(x => x.Name).FirstOrDefault("");

        /// <summary>
        /// Gets the string displayed in top bar of this tool window.
        /// </summary>
        public override string ToolWindowTitle => "Export";
    }
}