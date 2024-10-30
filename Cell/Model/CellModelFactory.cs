using Cell.Core.Common;
using Cell.Core.Data.Tracker;
using Cell.ViewModel.Application;
using System.Text.Json;

namespace Cell.Model
{
    /// <summary>
    /// A factory for creating new cell models.
    /// </summary>
    public static class CellModelFactory
    {
        /// <summary>
        /// The default height of new cells.
        /// </summary>
        public const int DefaultCellHeight = 25;

        /// <summary>
        /// The default width of new cells.
        /// </summary>
        public const int DefaultCellWidth = 125;
        /// <summary>
        /// Performs a deep copy of the given cell model by serializing and deserializing it.
        /// </summary>
        /// <param name="modelToCopy">The cell to copy.</param>
        /// <returns>The copied cell.</returns>
        /// <exception cref="CellError">If there was an issue during serialization.</exception>
        public static CellModel Copy(this CellModel modelToCopy)
        {
            var serialized = JsonSerializer.Serialize(modelToCopy);
            return JsonSerializer.Deserialize<CellModel>(serialized) ?? throw new CellError("Unable to copy model");
        }

        /// <summary>
        /// Creates a new cell model with the given type and location.
        /// </summary>
        /// <param name="type">The type of the new node.</param>
        /// <param name="location">The location of the new node.</param>
        /// <returns>The new cell.</returns>
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

        /// <summary>
        /// Creates a new cell model with the given type and location and adds it to the given tracker.
        /// </summary>
        /// <param name="row">The row for the new cell.</param>
        /// <param name="column">The column for the new cell.</param>
        /// <param name="type">The type of the new cell.</param>
        /// <param name="sheet">The sheet the new cell should exist in.</param>
        /// <param name="trackerToTrackCellWith">The cell tracker to add the new cell to.</param>
        /// <returns>The new cell.</returns>
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
