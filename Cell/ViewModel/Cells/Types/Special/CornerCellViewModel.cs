using Cell.Model;
using Cell.Persistence;
using Cell.View.Skin;

namespace Cell.ViewModel.Cells.Types.Special
{
    public class CornerCellViewModel(CellModel model, SheetViewModel sheetViewModel) : SpecialCellViewModel(model, sheetViewModel)
    {
        public override string BackgroundColorHex { get => ColorConstants.ToolWindowHeaderColorConstantHex; set => base.BackgroundColorHex = value; }

        public string ImportingTemplateName { get; set; }

        public string NewSheetNameForImportedTemplates { get; set; }

        public IEnumerable<string> PossibleTemplates => PersistenceManager.GetTemplateNames();
    }
}
