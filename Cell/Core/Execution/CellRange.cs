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
    }
}
