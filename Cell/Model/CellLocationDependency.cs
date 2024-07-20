
namespace Cell.Model
{
    public class CellLocationDependency(string sheet, int row, int column)
    {
        public string SheetName { get; set; } = sheet;

        public int Row { get; set; } = row;

        public int Column { get; set; } = column;
    }
}
