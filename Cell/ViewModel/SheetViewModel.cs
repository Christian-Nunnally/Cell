using Cell.Controls;
using Cell.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Cell.ViewModel
{
    public class SheetViewModel : INotifyPropertyChanged
    {
        private const int BottomPanelHeight = 50;
        private const int TopPanelHeight = 50;
        private const int LeftPanelHeight = 250;
        private const int RightPanelHeight = 250;

        public SheetViewModel(string sheetName)
        {
            SheetName = sheetName;
        }

        public void LoadCellViewModels()
        {
            CellLoader.GetCellViewModelsForSheet(this).ForEach(x => CellViewModels.Add(x));
            PopulateCells(); 
        }

        private const int DefaultCellWidth = 125;
        private const int DefaultCellHeight = 25;

        private int editingSpaceTop;
        private int editingSpaceBottom;
        private int editingSpaceLeft;
        private int editingSpaceRight;
        public bool AreEditingPanelsOpen;
        private CellViewModel selectedCellViewModel;

        public string SheetName { get; private set; }

        public ObservableCollection<CellViewModel> CellViewModels { get; set; } = new ObservableCollection<CellViewModel>();

        public IEnumerable<CellViewModel> RowCellViewModels => CellViewModels.OfType<RowCellViewModel>();

        public IEnumerable<CellViewModel> ColumnCellViewModels => CellViewModels.OfType<ColumnCellViewModel>();

        public int Height { get; set; }

        public int Width { get; set; }

        public int EditingSpaceTop
        {
            get => editingSpaceTop;
            set
            {
                editingSpaceTop = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EditingSpaceTop)));
            }
        }

        public int EditingSpaceBottom
        {
            get => editingSpaceBottom;
            set
            {
                editingSpaceBottom = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EditingSpaceBottom)));
            }
        }

        public int EditingSpaceLeft
        {
            get => editingSpaceLeft;
            set
            {
                editingSpaceLeft = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EditingSpaceLeft)));
            }
        }

        public int EditingSpaceRight
        {
            get => editingSpaceRight;
            set
            {
                editingSpaceRight = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EditingSpaceRight)));
            }
        }

        public CellViewModel SelectedCellViewModel
        {
            get => selectedCellViewModel;
            set
            {
                selectedCellViewModel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedCellViewModel)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void AddRow()
        {
            if (!ColumnCellViewModels.Any())
            {
                var newModel2 = ColumnCellViewModel.CreateColumnCellModel(0, -DefaultCellHeight, DefaultCellWidth, DefaultCellHeight, SheetName, GetColumnName(ColumnCellViewModels.Count()));
                var newViewModel2 = CellViewModel.CreateViewModelForModel(newModel2, this);
                CellViewModels.Add(newViewModel2);
            }

            double totalHeight = RowCellViewModels.Sum(x => x.Height);
            var newModel = RowCellViewModel.CreateRowCellModel(-DefaultCellWidth, totalHeight, DefaultCellWidth, DefaultCellHeight, SheetName, RowCellViewModels.Count().ToString());
            var newViewModel = CellViewModel.CreateViewModelForModel(newModel, this);
            CellViewModels.Add(newViewModel);

            var currentColumns = ColumnCellViewModels.ToList();
            foreach (var columnCellViewModel in currentColumns)
            {
                var newModel2 = CellViewModel.CreateCellModel(columnCellViewModel.X, totalHeight, columnCellViewModel.Width, DefaultCellHeight, SheetName, "Hello world");
                var newViewModel2 = CellViewModel.CreateViewModelForModel(newModel2, this);
                CellViewModels.Add(newViewModel2);
            }
        }

        public void AddColumn()
        {
            if (!RowCellViewModels.Any())
            {
                var newModel2 = RowCellViewModel.CreateRowCellModel(-DefaultCellWidth, 0, DefaultCellWidth, DefaultCellHeight, SheetName, RowCellViewModels.Count().ToString());
                var newViewModel2 = CellViewModel.CreateViewModelForModel(newModel2, this);
                CellViewModels.Add(newViewModel2);
            }

            double totalWidth = ColumnCellViewModels.Sum(x => x.Width);

            var newModel = ColumnCellViewModel.CreateColumnCellModel(totalWidth, -DefaultCellHeight, DefaultCellWidth, DefaultCellHeight, SheetName, GetColumnName(ColumnCellViewModels.Count()));
            var newViewModel = CellViewModel.CreateViewModelForModel(newModel, this);
            CellViewModels.Add(newViewModel);

            var currentRows = RowCellViewModels.ToList();
            foreach (var rowCellViewModel in currentRows)
            {
                var newModel2 = CellViewModel.CreateCellModel(totalWidth, rowCellViewModel.Y, DefaultCellWidth, rowCellViewModel.Height, SheetName, "Hello world");
                var newViewModel2 = CellViewModel.CreateViewModelForModel(newModel2, this);
                CellViewModels.Add(newViewModel2);
            }
        }

        public static string GetColumnName(int columnNumber)
        {
            columnNumber += 1;
            string columnName = "";
            while (columnNumber > 0)
            {
                int modulo = (columnNumber - 1) % 26;
                columnName = Convert.ToChar('A' + modulo) + columnName;
                columnNumber = (columnNumber - modulo) / 26;
            }
            return columnName;
        }

        public void PopulateCells()
        {
            if (RowCellViewModels.Any()) return;
            AddRow();
            for (int i = 0; i < 1; i++)
            {
                AddRow();
                AddColumn();
            }
        }

        public void UnselectAllCells()
        {
            foreach (var cell in CellViewModels)
            {
                cell.IsSelected = false;
            }
        }

        public void SelectCell(CellViewModel cell)
        {
            cell.IsSelected = true;
            SelectedCellViewModel = cell;
        }

        public void OpenEditingPanels(PanAndZoomCanvas canvas)
        {
            AreEditingPanelsOpen = true;
            canvas.PanCanvasBy(-LeftPanelHeight, -TopPanelHeight);
            EditingSpaceTop = TopPanelHeight;
            EditingSpaceBottom = BottomPanelHeight;
            EditingSpaceLeft = LeftPanelHeight;
            EditingSpaceRight = RightPanelHeight;
        }

        public void CloseEditingPanels(PanAndZoomCanvas canvas)
        {
            AreEditingPanelsOpen = false;
            canvas.PanCanvasBy(LeftPanelHeight, TopPanelHeight);
            EditingSpaceTop = 0;
            EditingSpaceBottom = 0;
            EditingSpaceLeft = 0;
            EditingSpaceRight = 0;
        }

        public void ResizeRow(RowCellViewModel rowCell, double oldSize, double newSize)
        {
            foreach (var cellInRow in CellViewModels.Where(x => x.Y == rowCell.Y))
            {
                cellInRow.Height = newSize;
            }
            foreach (var cellBelowRow in CellViewModels.Where(x => x.Y > rowCell.Y))
            {
                cellBelowRow.Y += newSize - oldSize;
            }
        }

        internal void ResizeColumn(ColumnCellViewModel columnCell, double oldSize, double newSize)
        {
            foreach (var cellInColumn in CellViewModels.Where(x => x.X == columnCell.X))
            {
                cellInColumn.Width = newSize;
            }
            foreach (var cellBesideColumn in CellViewModels.Where(x => x.X > columnCell.X))
            {
                cellBesideColumn.X += newSize - oldSize;
            }
        }
    }
}