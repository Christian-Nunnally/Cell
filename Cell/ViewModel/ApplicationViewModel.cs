using Cell.Data;
using Cell.Model;
using Cell.View;
using System.Collections.ObjectModel;

namespace Cell.ViewModel
{
    public class ApplicationViewModel : PropertyChangedBase
    {
        private readonly CellClipboard _cellClipboard = new();
        private const int BottomPanelHeight = 0;
        private const int TopPanelHeight = 35;
        private const int LeftPanelHeight = 215;
        private const int RightPanelHeight = 0;

        public static ApplicationViewModel Instance 
        { 
            get => instance ?? throw new NullReferenceException("Application instance not set"); 
            private set => instance = value ?? throw new NullReferenceException("Static instances not allowed to be null"); 
        }

        private ApplicationViewModel(ApplicationView mainWindow)
        {
            MainWindow = mainWindow;
        }

        public readonly ApplicationView MainWindow;
        private static ApplicationViewModel? instance;
        private SheetViewModel sheetViewModel = SheetViewModelFactory.GetOrCreate(ApplicationSettings.Instance.LastLoadedSheet);
        public bool AreEditingPanelsOpen;

        public double ApplicationWindowWidth
        {
            get { return _applicationWindowWidth; }
            set
            {
                _applicationWindowWidth = value;
                NotifyPropertyChanged(nameof(ApplicationWindowWidth));
            }
        }
        private double _applicationWindowWidth = 1200;

        public double ApplicationWindowHeight
        {
            get { return _applicationWindowHeight; }
            set
            {
                _applicationWindowHeight = value;
                NotifyPropertyChanged(nameof(ApplicationWindowHeight));
            }
        }
        private double _applicationWindowHeight = 1300;

        public SheetViewModel SheetViewModel
        {
            get { return sheetViewModel; }
            set
            {
                sheetViewModel = value;
                NotifyPropertyChanged(nameof(SheetViewModel));
            }
        }

        public int EditingSpaceTop
        {
            get => editingSpaceTop;
            set
            {
                editingSpaceTop = value;
                NotifyPropertyChanged(nameof(EditingSpaceTop));
            }
        }
        private int editingSpaceTop;

        public int EditingSpaceBottom
        {
            get => editingSpaceBottom;
            set
            {
                editingSpaceBottom = value;
                NotifyPropertyChanged(nameof(EditingSpaceBottom));
            }
        }
        private int editingSpaceBottom;

        public int EditingSpaceLeft
        {
            get => editingSpaceLeft;
            set
            {
                editingSpaceLeft = value;
                NotifyPropertyChanged(nameof(EditingSpaceLeft));
            }
        }
        private int editingSpaceLeft;

        public int EditingSpaceRight
        {
            get => editingSpaceRight;
            set
            {
                editingSpaceRight = value;
                NotifyPropertyChanged(nameof(EditingSpaceRight));
            }
        }
        private int editingSpaceRight;

        public bool IsAddingSheet
        {
            get => _isAddingSheet;
            set
            {
                _isAddingSheet = value;
                NotifyPropertyChanged(nameof(IsAddingSheet));
            }
        }
        private bool _isAddingSheet;

        public string NewSheetName
        {
            get => _newSheetName;
            set
            {
                _newSheetName = value;
                NotifyPropertyChanged(nameof(NewSheetName));
            }
        }
        private string _newSheetName;

        public ObservableCollection<string> SheetNames => Cells.Instance.SheetNames;

        public void ChangeSelectedCellsType(CellType newType)
        {
            UndoRedoManager.RecordCellStatesOntoUndoStack(SheetViewModel.SelectedCellViewModels.Select(x => x.Model));
            var selectedCells = SheetViewModel.SelectedCellViewModels.ToList();
            foreach (var selectedCell in selectedCells)
            {
                selectedCell.CellType = newType;
            }
            selectedCells.ForEach(x => SheetViewModel.SelectCell(x.Model));
            SheetViewModel.UpdateLayout();
        }

        public static ApplicationViewModel GetOrCreateInstance(ApplicationView mainWindow)
        {
            instance ??= new ApplicationViewModel(mainWindow);
            return instance;
        }

        internal void GoToSheet(string sheetName)
        {
            if (SheetViewModel.SheetName == sheetName) return;
            SheetViewModel = SheetViewModelFactory.GetOrCreate(sheetName);
            if (!sheetViewModel.CellViewModels.Any()) sheetViewModel.LoadCellViewModels();
            ApplicationSettings.Instance.LastLoadedSheet = sheetName;
        }

        internal void GoToCell(CellModel cellModel)
        {
            GoToSheet(cellModel.SheetName);
            var cell = SheetViewModel.CellViewModels.FirstOrDefault(x => x.Model.ID == cellModel.ID);
            if (cell is not null) MainWindow.SheetView?.PanAndZoomCanvas?.PanCanvasTo(cell.X, cell.Y);
        }

        internal void CopySelectedCells(bool copyTextOnly)
        {
            _cellClipboard.CopySelectedCells(SheetViewModel, copyTextOnly);
        }

        internal void PasteCopiedCells()
        {
            _cellClipboard.PasteCopiedCells(SheetViewModel);
        }

        public bool ToggleEditingPanels()
        {
            if (AreEditingPanelsOpen)
            {
                CloseEditingPanels();
                return false;
            }
            OpenEditingPanels();
            return true;
        }

        public void OpenEditingPanels()
        {
            AreEditingPanelsOpen = true;
            EditingSpaceTop = TopPanelHeight;
            EditingSpaceBottom = BottomPanelHeight;
            EditingSpaceLeft = LeftPanelHeight;
            EditingSpaceRight = RightPanelHeight;
        }

        public void CloseEditingPanels()
        {
            AreEditingPanelsOpen = false;
            EditingSpaceTop = 0;
            EditingSpaceBottom = 0;
            EditingSpaceLeft = 0;
            EditingSpaceRight = 0;
        }

        internal void RenameSheet(string oldSheetName, string newSheetName)
        {
            if (oldSheetName == newSheetName) return;
            Cells.Instance.RenameSheet(oldSheetName, newSheetName);
        }
    }
}
