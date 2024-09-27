using Cell.Data;

namespace Cell.Model
{
    public class SheetFactory
    {
        public static void CreateSheet(string sheetName, int initialRows, int initialColumns, CellTracker cellTracker)
        {
            CellModelFactory.Create(0, 0, CellType.Corner, sheetName, cellTracker);

            for (var i = 0; i < initialRows; i++)
            {
                CellModelFactory.Create(i + 1, 0, CellType.Row, sheetName, cellTracker);
            }

            for (var i = 0; i < initialColumns; i++)
            {
                CellModelFactory.Create(0, i + 1, CellType.Column, sheetName, cellTracker);
            }

            for (var i = 0; i < initialRows; i++)
            {
                for (var j = 0; j < initialColumns; j++)
                {
                    CellModelFactory.Create(i + 1, j + 1, CellType.Label, sheetName, cellTracker);
                }
            }
        }
    }
}
