using Cell.Common;
using Cell.Data;
using Cell.Model;
using System.Collections;

namespace Cell.Execution
{
    public class CellRange(List<CellModel> cells) : IEnumerable<CellModel>
    {
        private readonly List<CellModel> _cells = cells;
        public IEnumerable<string> Texts => _cells.Select(x => x.Text);

        public IEnumerable<double> Values => _cells.Select(x => x.Value);

        public IEnumerator<CellModel> GetEnumerator() => _cells.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public CellModel this[int key]
        {
            get => key >= 0 && key < _cells.Count() 
                ? _cells[key] 
                : throw new CellError($"{key} is out of range for the cell range starting with cell {_cells.FirstOrDefault()?.UserFriendlyCellName}");
        }
    }
}
