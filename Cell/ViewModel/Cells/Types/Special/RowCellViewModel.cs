using Cell.Common;
using Cell.Execution.SyntaxWalkers;
using Cell.Model;
using Cell.View.Skin;
using Cell.ViewModel.Application;
using Cell.ViewModel.Execution;
using Microsoft.CodeAnalysis.CSharp;

namespace Cell.ViewModel.Cells.Types.Special
{
    public class RowCellViewModel : SpecialCellViewModel
    {
        public RowCellViewModel(CellModel model, SheetViewModel sheetViewModel) : base(model, sheetViewModel)
        {
            NotifyPropertyChanged(nameof(Text));
            model.PropertyChanged += ModelPropertyChanged;
        }

        public override string BackgroundColorHex { get => ColorConstants.ToolWindowHeaderColorConstantHex; set => base.BackgroundColorHex = value; }

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

        public override string Text { get => Row.ToString(); set => base.Text = value; }

        public void AddRowAbove()
        {
            var rowToInsertAt = Row;
            AddRowAt(rowToInsertAt);
        }

        public void AddRowBelow()
        {
            var rowToInsertAt = Row + 1;
            AddRowAt(rowToInsertAt);
        }

        public void DeleteRow()
        {
            if (_sheetViewModel.CellViewModels.OfType<RowCellViewModel>().Count() == 1) return;
            var cellsToDelete = ApplicationViewModel.Instance.CellTracker.GetCellModelsForSheet(Model.SheetName).Where(x => x.Row == Model.Row).ToList();
            foreach (var cell in cellsToDelete)
            {
                _sheetViewModel.DeleteCell(cell);
            }
            IncrementRowOfAllAtOrBelow(Row, -1);
            _sheetViewModel.UpdateLayout();

            foreach (var function in ApplicationViewModel.Instance.PluginFunctionLoader.ObservableFunctions)
            {
                IncrementRowReferenceOfAbsoluteReferencesForInsertedRow(Row, function, -1);
            }
        }

        private void AddRowAt(int index)
        {
            InsertRowAtIndex(index);
            _sheetViewModel.UpdateLayout();
        }

        private List<CellModel> GetAllCellsAtOrBelow(int row) => ApplicationViewModel.Instance.CellTracker.GetCellModelsForSheet(Model.SheetName).Where(x => x.Row >= row).ToList();

        private void IncrementRowOfAllAtOrBelow(int row, int amount = 1)
        {
            var cells = GetAllCellsAtOrBelow(row);
            foreach (var cell in cells) cell.Row += amount;
        }

        private void InsertRowAtIndex(int newRowIndex)
        {
            IncrementRowOfAllAtOrBelow(newRowIndex);

            var rowModel = CellModelFactory.Create(newRowIndex, 0, CellType.Row, Model.SheetName);

            var sheet = ApplicationViewModel.Instance.SheetTracker.Sheets.FirstOrDefault(x => x.Name == Model.SheetName);

            var row = CellViewModelFactory.Create(rowModel, _sheetViewModel);
            _sheetViewModel.AddCell(row);

            var columnIndexs = _sheetViewModel.CellViewModels.OfType<ColumnCellViewModel>().Select(x => x.Column).ToList();
            foreach (var columnIndex in columnIndexs)
            {
                var cellModel = CellModelFactory.Create(newRowIndex, columnIndex, CellType.Label, Model.SheetName);
                sheet?.CornerCell?.CopyPublicProperties(cellModel, [nameof(CellModel.ID), nameof(CellModel.SheetName), nameof(CellModel.Width), nameof(CellModel.Height), nameof(CellModel.Row), nameof(CellModel.Column), nameof(CellModel.MergedWith), nameof(CellModel.Value), nameof(CellModel.Date), nameof(CellModel.CellType)]);
                var cell = CellViewModelFactory.Create(cellModel, _sheetViewModel);
                _sheetViewModel.AddCell(cell);
                ApplicationViewModel.Instance.CellPopulateManager.NotifyCellValueUpdated(cellModel);

                var firstSideMergeId = ApplicationViewModel.Instance.CellTracker.GetCell(Model.SheetName, newRowIndex - 1, columnIndex)?.MergedWith ?? string.Empty;
                var secondSideMergeId = ApplicationViewModel.Instance.CellTracker.GetCell(Model.SheetName, newRowIndex + 1, columnIndex)?.MergedWith ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(firstSideMergeId) && firstSideMergeId == secondSideMergeId)
                {
                    cellModel.MergedWith = firstSideMergeId;
                }
            }

            foreach (var function in ApplicationViewModel.Instance.PluginFunctionLoader.ObservableFunctions)
            {
                IncrementRowReferenceOfAbsoluteReferencesForInsertedRow(newRowIndex, function, 1);
            }
        }

        private void IncrementRowReferenceOfAbsoluteReferencesForInsertedRow(int newRowIndex, FunctionViewModel function, int incrementAmount)
        {
            var refactorer = new CellReferenceRefactorRewriter(x =>
            {
                if (x.SheetName != Model.SheetName) return x;
                if (!x.IsRowRelative) return x;
                if (x.Row >= newRowIndex) x.Row += 1;
                if (x.IsRange && x.RowRangeEnd >= newRowIndex) x.RowRangeEnd += incrementAmount;
                return x;
            });
            function.Model.Code = refactorer.Visit(CSharpSyntaxTree.ParseText(function.Model.Code).GetRoot())?.ToFullString() ?? "";
        }

        private void ModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellModel.Row)) NotifyPropertyChanged(nameof(Text));
        }
    }
}
