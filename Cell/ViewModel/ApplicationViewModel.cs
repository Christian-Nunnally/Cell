using Cell.Model;
using Cell.View;

namespace Cell.ViewModel
{
    public class ApplicationViewModel : PropertyChangedBase
    {
        private const int BottomPanelHeight = 0;
        private const int TopPanelHeight = 35;
        private const int LeftPanelHeight = 215;
        private const int RightPanelHeight = 0;

        public static ApplicationViewModel Instance 
        { 
            get => instance ?? throw new NullReferenceException("Application instance not set"); 
            private set => instance = value ?? throw new NullReferenceException("Static instances not allowed to be null"); 
        }

        private ApplicationViewModel(MainWindow mainWindow)
        {
            MainWindow = mainWindow;
        }

        public readonly MainWindow MainWindow;
        private static ApplicationViewModel? instance;
        private SheetViewModel sheetViewModel = SheetViewModelFactory.GetOrCreate("Default");
        private int editingSpaceTop;
        private int editingSpaceBottom;
        private int editingSpaceLeft;
        private int editingSpaceRight;
        public bool AreEditingPanelsOpen;

        public SheetViewModel SheetViewModel
        {
            get { return sheetViewModel; }
            set
            {
                sheetViewModel = value;
                OnPropertyChanged(nameof(SheetViewModel));
            }
        }

        public int EditingSpaceTop
        {
            get => editingSpaceTop;
            set
            {
                editingSpaceTop = value;
                OnPropertyChanged(nameof(EditingSpaceTop));
            }
        }

        public int EditingSpaceBottom
        {
            get => editingSpaceBottom;
            set
            {
                editingSpaceBottom = value;
                OnPropertyChanged(nameof(EditingSpaceBottom));
            }
        }

        public int EditingSpaceLeft
        {
            get => editingSpaceLeft;
            set
            {
                editingSpaceLeft = value;
                OnPropertyChanged(nameof(EditingSpaceLeft));
            }
        }

        public int EditingSpaceRight
        {
            get => editingSpaceRight;
            set
            {
                editingSpaceRight = value;
                OnPropertyChanged(nameof(EditingSpaceRight));
            }
        }

        public void ChangeSelectedCellsType(CellType newType)
        {
            foreach (var selectedCell in SheetViewModel.SelectedCellViewModels.ToList())
            {
                SheetViewModel.ChangeCellType(selectedCell, newType);
            }
            SheetViewModel.UpdateLayout();
        }

        public static ApplicationViewModel GetOrCreateInstance(MainWindow mainWindow)
        {
            instance ??= new ApplicationViewModel(mainWindow);
            return instance;
        }

        internal void GoToSheet(string sheetName)
        {
            if (SheetViewModel.SheetName == sheetName) return;
            SheetViewModel = SheetViewModelFactory.GetOrCreate(sheetName);
            SheetViewModel.LoadCellViewModels();
        }

        internal void GoToCell(CellModel cellModel)
        {
            GoToSheet(cellModel.SheetName);
            var cell = SheetViewModel.CellViewModels.FirstOrDefault(x => x.Model.ID == cellModel.ID);
            if (cell is not null) MainWindow.SheetView?.PanAndZoomCanvas?.PanCanvasTo(cell.X, cell.Y);
        }

        internal void CopySelectedCells()
        {
            CellClipboard.CopySelectedCells(SheetViewModel);
        }

        internal void PasteCopiedCells()
        {
            CellClipboard.PasteCopiedCells(SheetViewModel);
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
    }
}
