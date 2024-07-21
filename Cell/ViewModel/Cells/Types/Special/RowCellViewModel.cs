using Cell.Data;
using Cell.Model;

namespace Cell.ViewModel
{
    public class RowCellViewModel : SpecialCellViewModel
    {
        public RowCellViewModel(CellModel model, SheetViewModel sheetViewModel) : base(model, sheetViewModel)
        {
            NotifyPropertyChanged(nameof(Text));
            model.PropertyChanged += ModelPropertyChanged;
        }

        private void ModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellModel.Row)) NotifyPropertyChanged(nameof(Text));
        }

        public override double Height
        {
            get => base.Height;
            set
            {
                if (value < 5) return;
                if (value > 500) return;
                if (base.Height == value) return;
                base.Height = value;
                _sheetViewModel.UpdateLayout();
            }
        }

        public override string Text 
        {
            get => Row.ToString();
            set => base.Text = value; 
        }

        public void DeleteRow()
        {
            if (_sheetViewModel.CellViewModels.OfType<RowCellViewModel>().Count() == 1) return;
            var cellsToDelete = Cells.GetCellModelsForSheet(Model.SheetName).Where(x => x.Row == Model.Row).ToList();
            foreach (var cell in cellsToDelete)
            {
                _sheetViewModel.DeleteCell(cell);
            }
            IncrementRowOfAllAtOrBelow(Row, -1);
            _sheetViewModel.UpdateLayout();
        }

        public void AddRowBelow()
        {
            var rowToInsertAt = Row + 1;
            AddRowAt(rowToInsertAt);
        }

        public void AddRowAbove()
        {
            var rowToInsertAt = Row;
            AddRowAt(rowToInsertAt);
        }

        private void AddRowAt(int index)
        {
            InsertRowAtIndex(index);
            _sheetViewModel.UpdateLayout();
        }

        private void InsertRowAtIndex(int index)
        {
            IncrementRowOfAllAtOrBelow(index);

            var rowModel = CellModelFactory.Create(index, 0, CellType.Row, Model.SheetName);
            var row = CellViewModelFactory.Create(rowModel, _sheetViewModel);
            _sheetViewModel.CellViewModels.Add(row);

            var columnIndexs = _sheetViewModel.CellViewModels.OfType<ColumnCellViewModel>().Select(x => x.Column).ToList();
            foreach (var columnIndex in columnIndexs)
            {
                var cellModel = CellModelFactory.Create(index, columnIndex, CellType.Label, Model.SheetName);
                var cell = CellViewModelFactory.Create(cellModel, _sheetViewModel);
                _sheetViewModel.CellViewModels.Add(cell);

                var firstSideMergeId = Cells.GetCell(Model.SheetName, index - 1, columnIndex)?.MergedWith ?? string.Empty;
                var secondSideMergeId = Cells.GetCell(Model.SheetName, index + 1, columnIndex)?.MergedWith ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(firstSideMergeId) && firstSideMergeId == secondSideMergeId)
                {
                    cellModel.MergedWith = firstSideMergeId;
                }
            }
        }

        private void IncrementRowOfAllAtOrBelow(int row, int amount = 1)
        {
            var cells = GetAllCellsAtOrBelow(row);
            foreach (var cell in cells) cell.Row += amount;
        }

        private List<CellModel> GetAllCellsAtOrBelow(int row) => Cells.GetCellModelsForSheet(Model.SheetName).Where(x => x.Row >= row).ToList();

        public override string BackgroundColorHex { get => "#2d2d30"; set => base.BackgroundColorHex = value; }
    }
}