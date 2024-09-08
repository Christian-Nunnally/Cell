using Cell.ViewModel.Application;

namespace Cell.ViewModel.ToolWindow
{
    public class ImportWindowViewModel : ResizeableToolWindowViewModel
    {
        public ImportWindowViewModel()
        {
        }

        public bool SkipExistingCollectionsDuringImport { get; set; } = false;

        public string ImportingTemplateName { get; set; } = string.Empty;

        public string NewSheetNameForImportedTemplates { get; set; } = string.Empty;

        public IEnumerable<string> PossibleTemplates => ApplicationViewModel.Instance.PersistenceManager.GetTemplateNames();
    }
}
