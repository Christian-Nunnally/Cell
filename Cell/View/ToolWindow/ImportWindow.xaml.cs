using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;

namespace Cell.View.ToolWindow
{
    public partial class ImportWindow : ResizableToolWindow
    {
        private ImportWindowViewModel ImportWindowViewModel => (ImportWindowViewModel)ToolViewModel;
        public ImportWindow(ImportWindowViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }

        private void ImportSheetButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ImportWindowViewModel.ImportingTemplateName))
            {
                DialogFactory.ShowDialog("No template selected", "Please select a template to import.");
                return;
            }
            if (string.IsNullOrWhiteSpace(ImportWindowViewModel.NewSheetNameForImportedTemplates))
            {
                DialogFactory.ShowDialog("No sheet name", "Please enter a name for the new sheet.");
                return;
            }
            var templateName = ImportWindowViewModel.ImportingTemplateName;
            var sheetName = ImportWindowViewModel.NewSheetNameForImportedTemplates;
            ApplicationViewModel.Instance.SheetTracker.ImportSheetTemplate(templateName, sheetName, ImportWindowViewModel.SkipExistingCollectionsDuringImport);
        }
    }
}
