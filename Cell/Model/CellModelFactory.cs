using Cell.Common;
using Cell.Data;
using System.Text.Json;

namespace Cell.Model
{
    internal static class CellModelFactory
    {
        public const int DefaultCellWidth = 125;
        public const int DefaultCellHeight = 25;

        internal static CellModel Create(int row, int column, CellType type, string sheet)
        {
            var model = new CellModel
            {
                Width = DefaultCellWidth,
                Height = DefaultCellHeight,
                CellType = type,
                SheetName = sheet,
                Row = row,
                Column = column,
            };
            Cells.Instance.AddCell(model);
            return model;
        }

        public static CellModel Copy(this CellModel modelToCopy)
        {
            var serialized = JsonSerializer.Serialize(modelToCopy);
            return JsonSerializer.Deserialize<CellModel>(serialized) ?? throw new CellError("Unable to copy model");
        }

        public static CellModel CopyAndTrackNewCell(this CellModel modelToCopy)
        {
            var model = modelToCopy.Copy();
            model.ID = Utilities.GenerateUnqiueId(12);
            Cells.Instance.AddCell(model);
            return model;
        }
    }
}
