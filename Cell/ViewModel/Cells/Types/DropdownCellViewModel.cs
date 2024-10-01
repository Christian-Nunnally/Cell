using Cell.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Cell.ViewModel.Cells.Types
{
    public class DropdownCellViewModel : CollectionCellViewModel
    {
        public DropdownCellViewModel(CellModel model, SheetViewModel sheetViewModel) : base(model, sheetViewModel)
        {
        }

        public override string Text { get => SelectedItem; set => base.Text = value; }

        public string SelectedItem { get; set; }
    }
}
