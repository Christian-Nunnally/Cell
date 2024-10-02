using Cell.ViewModel.Application;

namespace Cell.ViewModel.ToolWindow
{
    /// <summary>
    /// Tool window view model for importing templates.
    /// </summary>
    public class ImportWindowViewModel : ToolWindowViewModel
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ImportWindowViewModel"/>.
        /// </summary>
        public ImportWindowViewModel()
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
        /// The name of the template to import.
        /// </summary>
        public string ImportingTemplateName { get; set; } = string.Empty;

        /// <summary>
        /// The name to give the new sheet that is created when importing a template.
        /// </summary>
        public string NewSheetNameForImportedTemplates { get; set; } = string.Empty;

        /// <summary>
        /// The list of possible templates that can be imported.
        /// </summary>
        public IEnumerable<string> PossibleTemplates => ApplicationViewModel.Instance.PersistedProject.GetTemplateNames();

        /// <summary>
        /// Whether to skip existing collections during import.
        /// </summary>
        public bool SkipExistingCollectionsDuringImport { get; set; } = false;

        /// <summary>
        /// Gets the string displayed in top bar of this tool window.
        /// </summary>
        public override string ToolWindowTitle => "Import";
    }
}
