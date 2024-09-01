using Cell.Persistence;

namespace Cell.ViewModel.ToolWindow
{
    public class ImportWindowViewModel : ResizeableToolWindowViewModel
    {

        public string ImportingTemplateName { get; set; }

        public string NewSheetNameForImportedTemplates { get; set; }

        public IEnumerable<string> PossibleTemplates => PersistenceManager.GetTemplateNames();

        public ImportWindowViewModel()
        {
        }
    }
}
