using Cell.Data;

namespace Cell.Model
{
    internal static class CellModelFactory
    {
        private const int DefaultCellWidth = 125;
        private const int DefaultCellHeight = 25;

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
            Cells.AddCell(model);
            return model;
        }

        public static CellModel Copy(this CellModel modelToCopy)
        {
            var model = CellModel.DeserializeModel(CellModel.SerializeModel(modelToCopy));
            model.ID = Utilities.GenerateUnqiueId(12);
            Cells.AddCell(model);
            return model;
        }
    }
}
