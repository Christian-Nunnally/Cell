﻿using Cell.ViewModel.Application;

namespace Cell.ViewModel.ToolWindow
{
    public class ExportWindowViewModel : ResizeableToolWindowViewModel
    {
        public ExportWindowViewModel()
        {
        }

        public IEnumerable<string> SheetNames => ApplicationViewModel.Instance.SheetTracker.OrderedSheets.Select(x => x.Name);

        public string SheetNameToExport { get; set; } = ApplicationViewModel.Instance.SheetTracker.OrderedSheets.Select(x => x.Name).FirstOrDefault("");
    }
}
