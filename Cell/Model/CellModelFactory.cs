using Cell.Common;
using System.Text.Json;

namespace Cell.Model
{
    internal static class CellModelFactory
    {
        public const int DefaultCellHeight = 25;
        public const int DefaultCellWidth = 125;
        public static CellModel Copy(this CellModel modelToCopy)
        {
            var serialized = JsonSerializer.Serialize(modelToCopy);
            return JsonSerializer.Deserialize<CellModel>(serialized) ?? throw new CellError("Unable to copy model");
        }

        internal static CellModel Create(int row, int column, CellType type, string sheet)
        {
            return new CellModel
            {
                Width = DefaultCellWidth,
                Height = DefaultCellHeight,
                CellType = type,
                SheetName = sheet,
                Row = row,
                Column = column,
            };
        }
    }
}
