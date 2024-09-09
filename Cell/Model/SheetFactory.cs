using Cell.Data;

namespace Cell.Model
{
    public class SheetFactory
    {
        public static void CreateSheet(string sheetName, int initialRows, int initialColumns, CellTracker cellTracker)
        {
            var corner = CellModelFactory.Create(0, 0, CellType.Corner, sheetName);
            cellTracker.AddCell(corner);

            for (var i = 0; i < initialRows; i++)
            {
                var row = CellModelFactory.Create(i + 1, 0, CellType.Row, sheetName);
                cellTracker.AddCell(row);
            }

            for (var i = 0; i < initialColumns; i++)
            {
                var columnCell = CellModelFactory.Create(0, i + 1, CellType.Column, sheetName);
                cellTracker.AddCell(columnCell);
            }

            for (var i = 0; i < initialRows; i++)
            {
                for (var j = 0; j < initialColumns; j++)
                {
                    var newCell = CellModelFactory.Create(i + 1, j + 1, CellType.Label, sheetName);
                    cellTracker.AddCell(newCell);
                }
            }
        }
    }
}
