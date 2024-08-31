using Cell.Common;
using Cell.Data;
using Cell.Model;
using Cell.Persistence;
using Cell.View.Application;
using Cell.ViewModel.Cells;

namespace Cell.ViewModel.Application
{
    public class ApplicationViewModel : PropertyChangedBase
    {
        public readonly ApplicationView ApplicationView;
        private readonly CellClipboard _cellClipboard = new();
        private static ApplicationViewModel? instance;
        private double _applicationWindowHeight = 1300;
        private double _applicationWindowWidth = 1200;
        private SheetViewModel sheetViewModel = SheetViewModelFactory.GetOrCreate(ApplicationSettings.Instance.LastLoadedSheet);
        private ApplicationViewModel(ApplicationView view)
        {
            ApplicationView = view;
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

        internal void CopySelectedCells(bool copyTextOnly)
        {
            _cellClipboard.CopySelectedCells(SheetViewModel, copyTextOnly);
        }

        internal void GoToCell(CellModel cellModel)
        {
            GoToSheet(cellModel.SheetName);
            var cell = SheetViewModel.CellViewModels.FirstOrDefault(x => x.Model.ID == cellModel.ID);
            if (cell is not null) ApplicationView.ActiveSheetView?.PanAndZoomCanvas?.PanCanvasTo(cell.X, cell.Y);
        }

        internal void GoToSheet(string sheetName)
        {
            if (SheetViewModel.SheetName == sheetName) return;
            SheetViewModel = SheetViewModelFactory.GetOrCreate(sheetName);
            if (!sheetViewModel.CellViewModels.Any()) sheetViewModel.LoadCellViewModels();
            ApplicationView.ShowSheetView(sheetViewModel);
            ApplicationSettings.Instance.LastLoadedSheet = sheetName;
        }

        internal void PasteCopiedCells()
        {
            UndoRedoManager.StartRecordingUndoState();
            _cellClipboard.PasteCopiedCells(SheetViewModel);
            UndoRedoManager.FinishRecordingUndoState();
        }
    }
}
