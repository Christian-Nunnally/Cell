using Cell.Model;

namespace Cell.ViewModel.Cells.Types
{
    public class ListCellViewModel : CollectionCellViewModel
    {
        public ListCellViewModel(CellModel model, SheetViewModel sheetViewModel) : base(model, sheetViewModel)
        {
        }

        public object? SelectedItem
        {
            get => Model.GetStringProperty(nameof(SelectedItem));
            set
            {
                Model.SetStringProperty(nameof(SelectedItem), value?.ToString() ?? "");
                NotifyPropertyChanged(nameof(SelectedItem));
            }
        }
    }
}
