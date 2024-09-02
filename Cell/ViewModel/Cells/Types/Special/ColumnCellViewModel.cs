using Cell.Common;
using Cell.Data;
using Cell.Execution.SyntaxWalkers;
using Cell.Model;
using Cell.View.Skin;
using Cell.ViewModel.Application;
using Cell.ViewModel.Execution;
using Microsoft.CodeAnalysis.CSharp;

namespace Cell.ViewModel.Cells.Types.Special
{
    public class ColumnCellViewModel : SpecialCellViewModel
    {
        public ColumnCellViewModel(CellModel model, SheetViewModel sheetViewModel) : base(model, sheetViewModel)
        {
            NotifyPropertyChanged(nameof(Text));
            model.PropertyChanged += ModelPropertyChanged;
        }

        public override string BackgroundColorHex { get => ColorConstants.ToolWindowHeaderColorConstantHex; set => base.BackgroundColorHex = value; }

        public override string Text
        {
            get
            {
                return GetColumnName(Column);
            }
            set => base.Text = value;
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

        public void AddColumnToTheLeft()
        {
            var columnToInsertAt = Column;
            AddColumnAt(columnToInsertAt);
        }

        public void AddColumnToTheRight()
        {
            var columnToInsertAt = Column + 1;
            AddColumnAt(columnToInsertAt);
        }

        public void DeleteColumn()
        {
            if (_sheetViewModel.CellViewModels.OfType<ColumnCellViewModel>().Count() == 1) return;
            var cellsToDelete = ApplicationViewModel.Instance.CellTracker.GetCellModelsForSheet(Model.SheetName).Where(x => x.Column == Column).ToList();
            foreach (var cell in cellsToDelete)
            {
                _sheetViewModel.DeleteCell(cell);
            }
            IncrementColumnOfAllAtOrToTheRightOf(Column, -1);
            _sheetViewModel.UpdateLayout();

            foreach (var function in ApplicationViewModel.Instance.PluginFunctionLoader.ObservableFunctions)
            {
                IncrementColumnReferenceOfAbsoluteReferencesForInsertedColumn(Column, function, -1);
            }
        }

        private void AddColumnAt(int index)
        {
            InsertColumnAtIndex(index);
            _sheetViewModel.UpdateLayout();
        }

        private List<CellModel> GetAllCellsAtOrToTheRightOf(int column) => ApplicationViewModel.Instance.CellTracker.GetCellModelsForSheet(Model.SheetName).Where(x => x.Column >= column).ToList();

        private void IncrementColumnOfAllAtOrToTheRightOf(int column, int amount = 1)
        {
            var cells = GetAllCellsAtOrToTheRightOf(column);
            foreach (var cell in cells) cell.Column += amount;
        }

        private void InsertColumnAtIndex(int index)
        {
            IncrementColumnOfAllAtOrToTheRightOf(index);

            var columnModel = CellModelFactory.Create(0, index, CellType.Column, Model.SheetName);

            var sheet = SheetTracker.Instance.Sheets.FirstOrDefault(x => x.Name == Model.SheetName);

            var column = CellViewModelFactory.Create(columnModel, _sheetViewModel);
            _sheetViewModel.AddCell(column);

            var rowIndexs = _sheetViewModel.CellViewModels.OfType<RowCellViewModel>().Select(x => x.Row).ToList();
            foreach (var rowIndex in rowIndexs)
            {
                var cellModel = CellModelFactory.Create(rowIndex, index, CellType.Label, Model.SheetName);
                sheet?.CornerCell?.CopyPublicProperties(cellModel, [nameof(CellModel.ID), nameof(CellModel.SheetName), nameof(CellModel.Width), nameof(CellModel.Height), nameof(CellModel.Row), nameof(CellModel.Column), nameof(CellModel.MergedWith), nameof(CellModel.Value), nameof(CellModel.Date), nameof(CellModel.CellType)]);
                var cell = CellViewModelFactory.Create(cellModel, _sheetViewModel);

                _sheetViewModel.AddCell(cell);
                ApplicationViewModel.Instance.CellPopulateManager.NotifyCellValueUpdated(cellModel);

                var cellAboveMergedId = ApplicationViewModel.Instance.CellTracker.GetCell(Model.SheetName, rowIndex, index - 1)?.MergedWith ?? string.Empty;
                var cellBelowMergedId = ApplicationViewModel.Instance.CellTracker.GetCell(Model.SheetName, rowIndex, index + 1)?.MergedWith ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(cellAboveMergedId) && cellAboveMergedId == cellBelowMergedId)
                {
                    cellModel.MergedWith = cellAboveMergedId;
                }
            }

            foreach (var function in ApplicationViewModel.Instance.PluginFunctionLoader.ObservableFunctions)
            {
                IncrementColumnReferenceOfAbsoluteReferencesForInsertedColumn(index, function, 1);
            }
        }

        private void IncrementColumnReferenceOfAbsoluteReferencesForInsertedColumn(int newColumnIndex, FunctionViewModel function, int incrementAmount)
        {
            var refactorer = new CellReferenceRefactorRewriter(x =>
            {
                if (x.SheetName != Model.SheetName) return x;
                if (!x.IsColumnRelative) return x;
                if (x.Column >= newColumnIndex) x.Column += 1;
                if (x.IsRange && x.ColumnRangeEnd >= newColumnIndex) x.ColumnRangeEnd += incrementAmount;
                return x;
            });
            function.Model.Code = refactorer.Visit(CSharpSyntaxTree.ParseText(function.Model.Code).GetRoot())?.ToFullString() ?? "";
        }

        private void ModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellModel.Column)) NotifyPropertyChanged(nameof(Text));
        }
    }
}
