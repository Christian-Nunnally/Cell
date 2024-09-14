using Cell.Common;
using Cell.Data;
using Cell.ViewModel.Application;
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
            var newCell = new CellModel
            {
                Width = DefaultCellWidth,
                Height = DefaultCellHeight,
                CellType = type,
                SheetName = sheet,
                Row = row,
                Column = column,
            };
            if (type.IsSpecial())
            {
                ApplicationViewModel.SafeInstance?.ApplicationSettings.DefaultSpecialCellStyleCellModel.Style.CopyTo(newCell.Style);
            }
            else
            {
                ApplicationViewModel.SafeInstance?.ApplicationSettings.DefaultCellStyleCellModel.Style.CopyTo(newCell.Style);
            }
            return newCell;
        }

        internal static CellModel Create(int row, int column, CellType type, string sheet, CellTracker trackerToTrackCellWith)
        {
            var newCell = Create(row, column, type, sheet);
            trackerToTrackCellWith.AddCell(newCell);
            return newCell;
        }
    }
}
