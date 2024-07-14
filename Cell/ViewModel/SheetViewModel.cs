using Cell.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using Cell.Persistence;

namespace Cell.ViewModel
{
    public class SheetViewModel(string sheetName) : PropertyChangedBase
    {
        public void LoadCellViewModels()
        {
            Cells.GetCellViewModelsForSheet(this).ForEach(x => CellViewModels.Add(x));
            InitializeSheet(); 
        }

        private CellViewModel? selectedCellViewModel;

        public string SheetName { get; private set; } = sheetName;

        public ObservableCollection<CellViewModel> CellViewModels { get; set; } = [];

        public IEnumerable<CellViewModel> SelectedCellViewModels => CellViewModels.Where(x => x.IsSelected);

        public IEnumerable<CellViewModel> RowCellViewModels => CellViewModels.OfType<RowCellViewModel>();

        public IEnumerable<CellViewModel> ColumnCellViewModels => CellViewModels.OfType<ColumnCellViewModel>();

        private string lastKeyPressed = string.Empty;
        public string LastKeyPressed
        {
            get => lastKeyPressed;
            set
            {
                lastKeyPressed = value;
                OnPropertyChanged(nameof(LastKeyPressed));
            }
        }

        public CellViewModel? SelectedCellViewModel
        {
            get => selectedCellViewModel;
            set
            {
                if (selectedCellViewModel is not null) selectedCellViewModel.PropertyChanged -= PropertyChangedOnSelectedCell;
                selectedCellViewModel = value;
                if (selectedCellViewModel is not null) selectedCellViewModel.PropertyChanged += PropertyChangedOnSelectedCell;
                OnPropertyChanged(nameof(SelectedCellViewModel));
            }
        }

        private void PropertyChangedOnSelectedCell(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is null) return;
            if (string.IsNullOrEmpty(e.PropertyName)) return;
            foreach (var cell in CellViewModels.Where(x => x.IsSelected).ToList())
            {
                if (cell == sender) continue;
                var cellType = cell.GetType();
                var selectedType = sender.GetType();
                if (cellType == selectedType || cellType.IsAssignableTo(selectedType))
                {
                    PropertyInfo? cellProperty = cellType.GetProperty(e.PropertyName, BindingFlags.Public | BindingFlags.Instance);
                    PropertyInfo? selectedProperty = selectedType.GetProperty(e.PropertyName, BindingFlags.Public | BindingFlags.Instance);
                    if (null != cellProperty && cellProperty.CanWrite && selectedProperty != null && selectedProperty.CanRead)
                    {
                        cellProperty.SetValue(cell, selectedProperty.GetValue(sender), null);
                    }
                }
            }
        }

        public void InitializeSheet()
        {
            if (RowCellViewModels.Any()) return;

            var corner = CellModelFactory.Create(0, 0, CellType.Corner, SheetName);
            AddCell(corner);

            var row = CellModelFactory.Create(1, 0, CellType.Row, SheetName);
            AddCell(row);

            var column = CellModelFactory.Create(0, 1, CellType.Column, SheetName);
            AddCell(column);

            var cell = CellModelFactory.Create(1, 1, CellType.Label, SheetName);
            AddCell(cell);

            UpdateLayout();
        }

        internal void UpdateLayout()
        {
            var cellModels = Cells.GetCellModelsForSheet(SheetName);
            var cornerCell = cellModels.First(x => x.Row == 0 && x.Column == 0);
            var lastCell = cornerCell;
            var rowsInOrder = RowCellViewModelsSorted.ToList();
            foreach (var rowCellViewModel in rowsInOrder)
            {
                rowCellViewModel.X = lastCell.X;
                rowCellViewModel.Y = lastCell.Y + lastCell.Height;
                lastCell = rowCellViewModel.Model;
            }
            lastCell = cornerCell;
            var columnsInOrder = ColumnCellViewModelsSorted.ToList();
            foreach (var columnCellViewModel in columnsInOrder)
            {
                columnCellViewModel.X = lastCell.X + lastCell.Width;
                columnCellViewModel.Y = lastCell.Y;
                lastCell = columnCellViewModel.Model;
            }
            foreach (var cellModel in cellModels)
            {
                if (cellModel.Row == 0 || cellModel.Column == 0) continue;
                var rowCell = Cells.GetCellModelForRow(cellModel.Row);
                var columnCell = Cells.GetCellModelForColumn(cellModel.Column);
                cellModel.X = columnCell?.X ?? cellModel.X;
                cellModel.Width = columnCell?.Width ?? cellModel.Width;
                cellModel.Y = rowCell?.Y ?? cellModel.Y;
                cellModel.Height = rowCell?.Height ?? cellModel.Height;
            }
        }

        public void AddCell(CellModel newModel)
        {
            var newViewModel = CellViewModelFactory.Create(newModel, this);
            CellViewModels.Add(newViewModel);
        }

        public void DeleteCell(CellViewModel cell)
        {
            CellViewModels.Remove(cell);
            Cells.RemoveCell(cell.Model);
        }

        public void DeleteCell(CellModel cell)
        {
            CellViewModels.Remove(CellViewModels.First(x => x.Model == cell));
            Cells.RemoveCell(cell);
        }

        public void UnselectAllCells()
        {
            foreach (var cell in CellViewModels)
            {
                UnselectCell(cell);
            }
        }

        public void SelectCell(CellViewModel cell)
        {
            cell.IsSelected = true;
            SelectedCellViewModel = cell;

            foreach (var cellToUnHighlight in CellViewModels)
            {
                cellToUnHighlight.UnhighlightCell();
            }
            if (SelectedCellViewModels.Count() == 1)
            {
                if (PluginFunctionLoader.TryGetPopulateFunction(SelectedCellViewModel.Model.PopulateFunctionName, out var function))
                {
                    for (int i = 0; i < function.SheetDependencies.Count; i++)
                    {
                        var sheet = function.SheetDependencies[i];
                        var row = function.RowDependencies[i];
                        var column = function.ColumnDependencies[i];
                        CellViewModels.FirstOrDefault(x => x.Row == row && x.Column == column)?.HighlightCell("#007acc");
                    }
                }
            }
        }

        public void ChangeCellType(CellViewModel? cellViewModel, CellType newType)
        {
            if (cellViewModel is null) return;
            CellViewModels.Remove(cellViewModel);
            cellViewModel.Model.CellType = newType;
            var newViewModel = CellViewModelFactory.Create(cellViewModel.Model, this);
            CellViewModels.Add(newViewModel);
        }

        internal void UnselectCell(CellViewModel cell)
        {
            cell.IsSelected = false;
            if (SelectedCellViewModel == cell) SelectedCellViewModel = null;
        }

        private IOrderedEnumerable<CellViewModel> RowCellViewModelsSorted => RowCellViewModels.OrderBy(x => x.Model.Row);

        private IOrderedEnumerable<CellViewModel> ColumnCellViewModelsSorted => ColumnCellViewModels.OrderBy(x => x.Model.Column);


        public static readonly SheetViewModel NullSheet = new("null");
    }
}