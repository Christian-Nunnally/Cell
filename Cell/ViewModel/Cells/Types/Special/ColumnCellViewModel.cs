using Cell.Data;
using Cell.Model;

namespace Cell.ViewModel
{
    public class ColumnCellViewModel : SpecialCellViewModel
    {
        public ColumnCellViewModel(CellModel model, SheetViewModel sheetViewModel) : base(model, sheetViewModel)
        {
            NotifyPropertyChanged(nameof(Text));
            model.PropertyChanged += ModelPropertyChanged;
        }

        private void ModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellModel.Column)) NotifyPropertyChanged(nameof(Text));
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
            if (_sheetViewModel.CellViewModels.OfType<ColumnCellViewModel>().Count() == 1) return;
            var cellsToDelete = Cells.Instance.GetCellModelsForSheet(Model.SheetName).Where(x => x.Column == Column).ToList();
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

        private void AddColumnAt(int index)
        {
            InsertColumnAtIndex(index);
            _sheetViewModel.UpdateLayout();
        }

        private void InsertColumnAtIndex(int index)
        {
            IncrementColumnOfAllAtOrToTheRightOf(index);

            var columnModel = CellModelFactory.Create(0, index, CellType.Column, Model.SheetName);
            var column = CellViewModelFactory.Create(columnModel, _sheetViewModel);
            _sheetViewModel.CellViewModels.Add(column);

            var rowIndexs = _sheetViewModel.CellViewModels.OfType<RowCellViewModel>().Select(x => x.Row).ToList();
            foreach (var rowIndex in rowIndexs)
            {
                var cellModel = CellModelFactory.Create(rowIndex, index, CellType.Label, Model.SheetName);
                var cell = CellViewModelFactory.Create(cellModel, _sheetViewModel);
                _sheetViewModel.CellViewModels.Add(cell);
                
                var cellAboveMergedId = Cells.Instance.GetCell(Model.SheetName, rowIndex, index - 1)?.MergedWith ?? string.Empty;
                var cellBelowMergedId = Cells.Instance.GetCell(Model.SheetName, rowIndex, index + 1)?.MergedWith ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(cellAboveMergedId) && cellAboveMergedId == cellBelowMergedId)
                {
                    cellModel.MergedWith = cellAboveMergedId;
                }
            }
        }

        private void IncrementColumnOfAllAtOrToTheRightOf(int column, int amount = 1)
        {
            var cells = GetAllCellsAtOrToTheRightOf(column);
            foreach (var cell in cells) cell.Column += amount;
        }

        private List<CellModel> GetAllCellsAtOrToTheRightOf(int column) => Cells.Instance.GetCellModelsForSheet(Model.SheetName).Where(x => x.Column >= column).ToList();

        public override string BackgroundColorHex { get => "#2d2d30"; set => base.BackgroundColorHex = value; }
    }
}