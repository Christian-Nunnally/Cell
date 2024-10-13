using Cell.Core.Common;
using Cell.Model;
using System.Collections;

namespace Cell.Core.Execution
{
    /// <summary>
    /// Represents a range of cells that can be indexed.
    /// </summary>
    /// <param name="cells"></param>
    public class CellRange(List<CellModel> cells) : IEnumerable<CellModel>
    {
        private readonly List<CellModel> _cells = cells;

        /// <summary>
        /// Gets the text values of all of the cells in this range as a list.
        /// </summary>
        public IEnumerable<string> Texts => _cells.Select(x => x.Text);

        /// <summary>
        /// Gets the text values as numbers of all of the cells in this range as a list.
        /// </summary>
        public IEnumerable<double> Values => _cells.Select(x => x.Value);

        /// <summary>
        /// Gets an enumerator that iterates through the cells in this range.
        /// </summary>
        /// <returns>The enumerator</returns>
        public IEnumerator<CellModel> GetEnumerator() => _cells.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Gets the cell at the specified index.
        /// </summary>
        /// <param name="key">The index of cell to get within the range.</param>
        /// <returns>The cell at the index from the range.</returns>
        /// <exception cref="CellError">Throws if the cell is out of the range.</exception>
        public CellModel this[int key]
        {
            get => key >= 0 && key < _cells.Count
                ? _cells[key]
                : throw new CellError($"{key} is out of range for the cell range starting with cell {_cells.FirstOrDefault()?.Location.UserFriendlyLocationString}");
        }
    }
}
