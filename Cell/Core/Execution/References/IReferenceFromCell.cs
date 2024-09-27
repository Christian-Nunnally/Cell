
using Cell.Model;

namespace Cell.Core.Execution.References
{
    public interface IReferenceFromCell
    {
        public string ResolveUserFriendlyNameForCell(CellModel cell);

        public string ResolveUserFriendlyCellAgnosticName();
    }
}
