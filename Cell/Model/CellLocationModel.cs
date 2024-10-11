using Cell.Common;
using Cell.ViewModel.Cells.Types;
using System.Text.Json.Serialization;

namespace Cell.Model
{
    /// <summary>
    /// Represents the location of a cell in a sheet, with a row and column number plus the name of the sheet.
    /// </summary>
    [Serializable]
    public class CellLocationModel : PropertyChangedBase
    {
        private int _row = 0;
        private int _column = 0;
        private string _sheetName = string.Empty;

        /// <summary>
        /// The row this cell is in. 1 = 1, 2 = 2, etc.
        /// </summary>
        public int Row
        {
            get => _row;
            set
            {
                if (_row == value) return;
                _row = value;
                NotifyPropertyChanged(nameof(Row));
            }
        }

        /// <summary>
        /// The column this cell is in. 1 = A, 2 = B, etc.
        /// </summary>
        public int Column
        {
            get => _column;
            set
            {
                if (_column == value) return;
                _column = value;
                NotifyPropertyChanged(nameof(Column));
            }
        }

        /// <summary>
        /// The name of the sheet this cell is in.
        /// </summary>
        public string SheetName
        {
            get => _sheetName;
            set
            {
                if (_sheetName == value) return;
                _sheetName = value;
                NotifyPropertyChanged(nameof(Column));
            }
        }

        /// <summary>
        /// Gets a unique string representation of the location of this cell, like Sheet1_1_4.
        /// </summary>
        public string LocationString => $"{SheetName}_{Row}_{Column}";

        /// <summary>
        /// Gets a user friendly string representation of the location of this cell, like A4 instead of 1-4.
        /// </summary>
        public string UserFriendlyLocationString => $"{ColumnCellViewModel.GetColumnName(Column)}{Row}";

        /// <summary>
        /// The <see cref="CellModel"/> that this location is associated with.
        /// </summary>
        [JsonIgnore]
        public CellModel? CellModel { get; internal set; }

        /// <summary>
        /// Creates a new instance of <see cref="CellLocationModel"/>
        /// </summary>
        public CellLocationModel()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="CellLocationModel"/>
        /// </summary>
        /// <param name="sheetName">The sheet location.</param>
        /// <param name="row">The row location.</param>
        /// <param name="column">The column location.</param>
        public CellLocationModel(string sheetName, int row, int column)
        {
            SheetName = sheetName;
            Row = row;
            Column = column;
        }

        /// <summary>
        /// Copies the values of this location into another location.
        /// </summary>
        /// <param name="otherLocation">The location model to copy in to.</param>
        public void CopyTo(CellLocationModel otherLocation)
        {
            otherLocation.Row = Row;
            otherLocation.Column = Column;
            otherLocation.SheetName = SheetName;
        }
    }

    /// <summary>
    /// Extensions for <see cref="CellLocationModel"/>.
    /// </summary>
    public static class CellLocationModelExtensions
    {
        /// <summary>
        /// Checks if two locations are the same.
        /// </summary>
        /// <param name="location">The first location.</param>
        /// <param name="otherLocation">The second location.</param>
        /// <returns>True if the locations represented by each location model object are equal (the same location).</returns>
        public static bool IsSameLocation(this CellLocationModel location, CellLocationModel otherLocation)
        {
            return location.SheetName == otherLocation.SheetName && location.Row == otherLocation.Row && location.Column == otherLocation.Column;
        }

        /// <summary>
        /// Creates a new <see cref="CellLocationModel"/> with the row offset by the given amount.
        /// </summary>
        /// <param name="location">The original location</param>
        /// <param name="rowOffset">The row offset</param>
        /// <returns>A new <see cref="CellLocationModel"/> with the row offset by the given amount.</returns>
        public static CellLocationModel WithRowOffset(this CellLocationModel location, int rowOffset)
        {
            return new CellLocationModel(location.SheetName, location.Row + rowOffset, location.Column);
        }

        /// <summary>
        /// Creates a new <see cref="CellLocationModel"/> with the column offset by the given amount.
        /// </summary>
        /// <param name="location">The original location</param>
        /// <param name="columnOffset">The column offset</param>
        /// <returns>A new <see cref="CellLocationModel"/> with the column offset by the given amount.</returns>
        public static CellLocationModel WithColumnOffset(this CellLocationModel location, int columnOffset)
        {
            return new CellLocationModel(location.SheetName, location.Row, location.Column + columnOffset);
        }
    }
}
