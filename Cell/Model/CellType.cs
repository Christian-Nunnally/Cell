
namespace Cell.Model
{
    public enum CellType
    {
        None     = 0,
        Corner   = 1 << 0,
        Row      = 1 << 1,
        Column   = 1 << 2,
        Label    = 1 << 3,
        Textbox  = 1 << 4,
        Checkbox = 1 << 5,
        Button   = 1 << 6,
        Progress = 1 << 7,
        Dropdown = 1 << 8,
        List     = 1 << 9,
        Graph    = 1 << 10,
    }

    public static class CellTypeExtensions
    {
        public static bool IsSpecial(this CellType value)
        {
            CellType isSpecialType = CellType.Corner | CellType.Row | CellType.Column;
            return (value & isSpecialType) != 0;
        }
    }
}
