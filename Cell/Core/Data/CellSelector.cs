using Cell.Model;
using System.Collections.ObjectModel;

namespace Cell.Data
{
    public class CellSelector
    {
        public ObservableCollection<CellModel> SelectedCells { get; } = [];

        public void SelectCell(CellModel cell)
        {
            SelectedCells.Add(cell);
        }

        public void UnselectAllCells()
        {
            foreach (var cell in SelectedCells.ToList()) UnselectCell(cell);
        }

        public void UnselectCell(CellModel cell)
        {
            SelectedCells.Remove(cell);
        }
    }
}
