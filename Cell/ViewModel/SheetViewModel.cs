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
            foreach (var cell in CellViewModelFactory.CreateCellViewModelsForSheet(this))
            {
                AddCell(cell);
            }
            InitializeSheet(); 
        }

        private CellViewModel? selectedCellViewModel;

        public string SheetName { get; private set; } = sheetName;

        public ObservableCollection<CellViewModel> CellViewModels { get; set; } = [];

        public List<CellViewModel> HighlightedCellViewModels { get; } = [];

        public List<CellViewModel> SelectedCellViewModels { get; } = [];

        private string lastKeyPressed = string.Empty;
        private bool _enableMultiEditSelectedCells = true;

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
            if (!_enableMultiEditSelectedCells) return;
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
            if (CellViewModels.Count == 0) AddDefaultCells();
            UpdateLayout();
        }

        private void AddDefaultCells()
        {
            var corner = CellModelFactory.Create(0, 0, CellType.Corner, SheetName);
            AddCell(corner);
            var row = CellModelFactory.Create(1, 0, CellType.Row, SheetName);
            AddCell(row);
            var column = CellModelFactory.Create(0, 1, CellType.Column, SheetName);
            AddCell(column);
            var cell = CellModelFactory.Create(1, 1, CellType.Label, SheetName);
            AddCell(cell);
        }

        internal void UpdateLayout()
        {
            _enableMultiEditSelectedCells = false;
            var layout = new CellLayout(CellViewModels);
            layout.UpdateLayout();
            _enableMultiEditSelectedCells = true;
        }

        public void AddCell(CellModel newModel)
        {
            var newViewModel = CellViewModelFactory.Create(newModel, this);
            AddCell(newViewModel);
        }

        public void AddCell(CellViewModel cell)
        {
            CellViewModels.Add(cell);
        }

        public void DeleteCell(CellViewModel cell)
        {
            CellViewModels.Remove(cell);
            SelectedCellViewModels.Remove(cell);
            Cells.RemoveCell(cell.Model);
        }

        public void DeleteCell(CellModel cell)
        {
            var viewModel = CellViewModels.First(x => x.Model == cell);
            DeleteCell(viewModel);
        }

        public void UnselectAllCells()
        {
            foreach (var cell in SelectedCellViewModels.ToList())
            {
                UnselectCell(cell);
            }
        }

        public void SelectCell(CellModel cell) => SelectCell(CellViewModels.First(x => x.Model == cell));

        public void SelectCell(CellViewModel cell)
        {
            if (cell.IsSelected) return;
            cell.IsSelected = true;
            SelectedCellViewModels.Add(cell);
            SelectedCellViewModel = cell;
            UnhighlightAllCells();
            if (SelectedCellViewModels.Count == 1)
            {
                if (PluginFunctionLoader.TryGetFunction(PluginFunctionLoader.PopulateFunctionsDirectoryName, SelectedCellViewModel.Model.PopulateFunctionName, out var function))
                {
                    for (int i = 0; i < function.SheetDependencies.Count; i++)
                    {
                        var sheet = function.SheetDependencies[i];
                        var row = function.RowDependencies[i];
                        var column = function.ColumnDependencies[i];
                        var cellToHighlight = CellViewModels.FirstOrDefault(x => x.Row == row && x.Column == column);
                        if (cellToHighlight == null) continue;
                        cellToHighlight.HighlightCell("#007acc");
                        HighlightedCellViewModels.Add(cellToHighlight);
                    }
                }
            }
        }

        private void UnhighlightAllCells()
        {
            foreach (var cellToUnHighlight in HighlightedCellViewModels)
            {
                cellToUnHighlight.UnhighlightCell();
            }
            HighlightedCellViewModels.Clear();
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
            SelectedCellViewModels.Remove(cell);
            if (!cell.IsSelected) return;
            cell.IsSelected = false;
            SelectedCellViewModels.Remove(cell);
            if (SelectedCellViewModel == cell) SelectedCellViewModel = null;
        }

        public static readonly SheetViewModel NullSheet = new("null");
    }
}