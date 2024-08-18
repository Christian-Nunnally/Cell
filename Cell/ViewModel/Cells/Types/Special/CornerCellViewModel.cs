using Cell.Model;
using Cell.Persistence;

namespace Cell.ViewModel.Cells.Types.Special
{
    public class CornerCellViewModel(CellModel model, SheetViewModel sheetViewModel) : SpecialCellViewModel(model, sheetViewModel)
    {
        public override string BackgroundColorHex { get => "#2d2d30"; set => base.BackgroundColorHex = value; }

        public string ImportingTemplateName { get; set; }

        public string NewSheetNameForImportedTemplates { get; set; }

        public IEnumerable<string> PossibleTemplates => PersistenceManager.GetTemplateNames();
    }
}
