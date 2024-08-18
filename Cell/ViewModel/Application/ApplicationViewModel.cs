using Cell.Common;
using Cell.Data;
using Cell.Model;
using Cell.Persistence;
using Cell.View.Application;
using Cell.ViewModel.Cells;
using System.Collections.ObjectModel;

namespace Cell.ViewModel.Application
{
    public class ApplicationViewModel : PropertyChangedBase
    {
        private const int BottomPanelHeight = 0;
        private const int LeftPanelHeight = 215;
        private const int RightPanelHeight = 0;
        private const int TopPanelHeight = 35;
        public readonly ApplicationView MainWindow;
        private readonly CellClipboard _cellClipboard = new();
        public bool AreEditingPanelsOpen;
        private static ApplicationViewModel? instance;
        private double _applicationWindowHeight = 1300;
        private double _applicationWindowWidth = 1200;
        private bool _isAddingSheet;
        private string _newSheetName = string.Empty;
        private int editingSpaceBottom;
        private int editingSpaceLeft;
        private int editingSpaceRight;
        private int editingSpaceTop;
        private SheetViewModel sheetViewModel = SheetViewModelFactory.GetOrCreate(ApplicationSettings.Instance.LastLoadedSheet);
        private ApplicationViewModel(ApplicationView mainWindow)
        {
            MainWindow = mainWindow;
        }

        public static ApplicationViewModel Instance { get => instance ?? throw new NullReferenceException("Application instance not set"); private set => instance = value ?? throw new NullReferenceException("Static instances not allowed to be null"); }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Binding")]
        public ApplicationSettings ApplicationSettings => ApplicationSettings.Instance;

        public double ApplicationWindowHeight
        {
            get { return _applicationWindowHeight; }
            set
            {
                _applicationWindowHeight = value;
                NotifyPropertyChanged(nameof(ApplicationWindowHeight));
            }
        }

        public double ApplicationWindowWidth
        {
            get { return _applicationWindowWidth; }
            set
            {
                _applicationWindowWidth = value;
                NotifyPropertyChanged(nameof(ApplicationWindowWidth));
            }
        }

        public int EditingSpaceBottom
        {
            get => editingSpaceBottom;
            set
            {
                editingSpaceBottom = value;
                NotifyPropertyChanged(nameof(EditingSpaceBottom));
            }
        }

        public int EditingSpaceLeft
        {
            get => editingSpaceLeft;
            set
            {
                editingSpaceLeft = value;
                NotifyPropertyChanged(nameof(EditingSpaceLeft));
            }
        }

        public int EditingSpaceRight
        {
            get => editingSpaceRight;
            set
            {
                editingSpaceRight = value;
                NotifyPropertyChanged(nameof(EditingSpaceRight));
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

        public bool IsAddingSheet
        {
            get => _isAddingSheet;
            set
            {
                _isAddingSheet = value;
                NotifyPropertyChanged(nameof(IsAddingSheet));
            }
        }

        public string NewSheetName
        {
            get => _newSheetName;
            set
            {
                _newSheetName = value;
                NotifyPropertyChanged(nameof(NewSheetName));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Binding")]
        public ObservableCollection<string> SheetNames => CellTracker.Instance.SheetNames;

        public SheetViewModel SheetViewModel
        {
            get { return sheetViewModel; }
            set
            {
                sheetViewModel = value;
                NotifyPropertyChanged(nameof(SheetViewModel));
            }
        }

        public static ApplicationViewModel GetOrCreateInstance(ApplicationView mainWindow)
        {
            instance ??= new ApplicationViewModel(mainWindow);
            return instance;
        }

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

        public void CloseEditingPanels()
        {
            AreEditingPanelsOpen = false;
            EditingSpaceTop = 0;
            EditingSpaceBottom = 0;
            EditingSpaceLeft = 0;
            EditingSpaceRight = 0;
        }

        public void OpenEditingPanels()
        {
            AreEditingPanelsOpen = true;
            EditingSpaceTop = TopPanelHeight;
            EditingSpaceBottom = BottomPanelHeight;
            EditingSpaceLeft = LeftPanelHeight;
            EditingSpaceRight = RightPanelHeight;
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

        internal void CopySelectedCells(bool copyTextOnly)
        {
            _cellClipboard.CopySelectedCells(SheetViewModel, copyTextOnly);
        }

        internal void GoToCell(CellModel cellModel)
        {
            GoToSheet(cellModel.SheetName);
            var cell = SheetViewModel.CellViewModels.FirstOrDefault(x => x.Model.ID == cellModel.ID);
            if (cell is not null) MainWindow.ActiveSheetView?.PanAndZoomCanvas?.PanCanvasTo(cell.X, cell.Y);
        }

        internal void GoToSheet(string sheetName)
        {
            if (SheetViewModel.SheetName == sheetName) return;
            SheetViewModel = SheetViewModelFactory.GetOrCreate(sheetName);
            if (!sheetViewModel.CellViewModels.Any()) sheetViewModel.LoadCellViewModels();
            MainWindow.ShowSheetView(sheetViewModel);
            ApplicationSettings.Instance.LastLoadedSheet = sheetName;
        }

        internal void PasteCopiedCells()
        {
            _cellClipboard.PasteCopiedCells(SheetViewModel);
        }

        internal static void RenameSheet(string oldSheetName, string newSheetName)
        {
            if (oldSheetName == newSheetName) return;
            CellTracker.Instance.RenameSheet(oldSheetName, newSheetName);
        }
    }
}
