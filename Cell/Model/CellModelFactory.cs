using Cell.Common;
using Cell.Data;
using Cell.ViewModel.Application;
using System.Text.Json;

namespace Cell.Model
{
    public static class CellModelFactory
    {
        public const int DefaultCellHeight = 25;
        public const int DefaultCellWidth = 125;
        public static CellModel Copy(this CellModel modelToCopy)
        {
            var serialized = JsonSerializer.Serialize(modelToCopy);
            return JsonSerializer.Deserialize<CellModel>(serialized) ?? throw new CellError("Unable to copy model");
        }

        public static CellModel Create(CellType type, CellLocationModel location)
        {
            var newCell = new CellModel
            {
                Width = DefaultCellWidth,
                Height = DefaultCellHeight,
                CellType = type,
                Location = location,
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

        public static CellModel Create(int row, int column, CellType type, string sheet, CellTracker trackerToTrackCellWith)
        {
            var location = new CellLocationModel
            {
                Row = row,
                Column = column,
                SheetName = sheet,
            };
            var newCell = Create(type, location);
            trackerToTrackCellWith.AddCell(newCell);
            return newCell;
        }
    }
}
