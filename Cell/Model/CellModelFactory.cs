using Cell.Common;
using Cell.Data;
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

        public static CellModel CopyAndTrackNewCell(this CellModel modelToCopy)
        {
            var model = modelToCopy.Copy();
            model.ID = Utilities.GenerateUnqiueId(12);
            CellTracker.Instance.AddCell(model);
            return model;
        }

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
            CellTracker.Instance.AddCell(model);
            return model;
        }
    }
}
