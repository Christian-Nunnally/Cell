
namespace Cell.Model
{
    public class CellLocationDependency(string sheetName, int row, int column)
    {
        public string SheetName { get; set; } = sheetName;

        public int Row { get; set; } = row;

        public int Column { get; set; } = column;
    }
}
