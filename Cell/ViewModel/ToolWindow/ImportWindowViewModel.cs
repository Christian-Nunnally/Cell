﻿using Cell.ViewModel.Application;

namespace Cell.ViewModel.ToolWindow
{
    public class ImportWindowViewModel : ToolWindowViewModel
    {
        public ImportWindowViewModel()
        {
        }

        public override double DefaultHeight => 200;

        public override double DefaultWidth => 200;

        public string ImportingTemplateName { get; set; } = string.Empty;

        public string NewSheetNameForImportedTemplates { get; set; } = string.Empty;

        public IEnumerable<string> PossibleTemplates => ApplicationViewModel.Instance.PersistedProject.GetTemplateNames();

        public bool SkipExistingCollectionsDuringImport { get; set; } = false;

        public override string ToolWindowTitle => "Import";
    }
}
