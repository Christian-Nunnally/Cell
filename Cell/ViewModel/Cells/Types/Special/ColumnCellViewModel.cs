using Cell.Model;
using Cell.Persistence;

namespace Cell.ViewModel
{
    public class ColumnCellViewModel : SpecialCellViewModel
    {
        public ColumnCellViewModel(CellModel model, SheetViewModel sheetViewModel) : base(model, sheetViewModel)
        {
            OnPropertyChanged(nameof(Text));
            model.PropertyChanged += ModelPropertyChanged;
        }

        private void ModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellModel.Column)) OnPropertyChanged(nameof(Text));
        }

        public override double Width
        {
            get => base.Width;
            set
            {
                if (value < 5) return;
                if (value > 500) return;
                if (base.Width == value) return;
                base.Width = value;
                _sheetViewModel.UpdateLayout();
            }
        }

        public override string Text
        {
            get
            {
                return GetColumnName(Column);
            }
            set => base.Text = value;
        }

        public static string GetColumnName(int columnNumber)
        {
            if (columnNumber < 1) return "=";
            string columnName = "";
            while (columnNumber > 0)
            {
                int modulo = (columnNumber - 1) % 26;
                columnName = Convert.ToChar('A' + modulo) + columnName;
                columnNumber = (columnNumber - modulo) / 26;
            }
            return columnName;
        }
        public void DeleteColumn()
        {
            if (_sheetViewModel.ColumnCellViewModels.Count() == 1) return;
            var cellsToDelete = Cells.GetCellModelsForSheet(Model.SheetName).Where(x => x.Column == Column).ToList();
            foreach (var cell in cellsToDelete)
            {
                _sheetViewModel.DeleteCell(cell);
            }
            IncrementColumnOfAllAtOrToTheRightOf(Column, -1);
            _sheetViewModel.UpdateLayout();
        }

        public void AddColumnToTheRight()
        {
            var columnToInsertAt = Column + 1;
            AddColumnAt(columnToInsertAt);
        }


        public void AddColumnToTheLeft()
        {
            var columnToInsertAt = Column;
            AddColumnAt(columnToInsertAt);
        }

        private void AddColumnAt(int columnToInsertAt)
        {
            IncrementColumnOfAllAtOrToTheRightOf(columnToInsertAt);
            InsertColumnAtIndex(columnToInsertAt);
            _sheetViewModel.UpdateLayout();
        }

        private void InsertColumnAtIndex(int index)
        {
            var columnModel = CellModelFactory.Create(0, index, CellType.Column, Model.SheetName);
            var column = CellViewModelFactory.Create(columnModel, _sheetViewModel);
            _sheetViewModel.CellViewModels.Add(column);

            var rowIndexs = _sheetViewModel.RowCellViewModels.Select(x => x.Row).ToList();
            foreach (var rowIndex in rowIndexs)
            {
                var cellModel = CellModelFactory.Create(rowIndex, index, CellType.Label, Model.SheetName);
                var cell = CellViewModelFactory.Create(cellModel, _sheetViewModel);
                _sheetViewModel.CellViewModels.Add(cell);
            }
        }

        private static void IncrementColumnOfAllAtOrToTheRightOf(int column, int amount = 1)
        {
            var cells = GetAllCellsAtOrToTheRightOf(column);
            foreach (var cell in cells) cell.Column += amount;
        }

        private static IEnumerable<CellModel> GetAllCellsAtOrToTheRightOf(int column) => Cells.AllCells.Where(x => x.Column >= column);

        public override string BackgroundColorHex { get => "#2d2d30"; set => base.BackgroundColorHex = value; }
    }
}