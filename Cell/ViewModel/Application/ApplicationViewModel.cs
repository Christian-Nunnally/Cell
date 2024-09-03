using Cell.Common;
using Cell.Data;
using Cell.Execution;
using Cell.Model;
using Cell.Persistence;
using Cell.View.Application;
using Cell.ViewModel.Cells;
using System.IO;

namespace Cell.ViewModel.Application
{
    public class ApplicationViewModel : PropertyChangedBase
    {
        public readonly ApplicationView ApplicationView;
        private readonly CellClipboard _cellClipboard;
        private static ApplicationViewModel? instance;
        private double _applicationWindowHeight = 1300;
        private double _applicationWindowWidth = 1200;
        private SheetViewModel sheetViewModel;
        private ApplicationViewModel(ApplicationView view)
        {
            PersistenceManager = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LGF", "Cell"), new FileIO());
            PluginFunctionLoader = new(PersistenceManager);
            CellTriggerManager = new(PluginFunctionLoader);
            CellPopulateManager = new(PluginFunctionLoader);
            SheetTracker = new();
            TitleBarSheetNavigationViewModel = new(SheetTracker);
            UserCollectionLoader = new(PersistenceManager, CellPopulateManager);
            CellLoader = new(PersistenceManager, SheetTracker, PluginFunctionLoader, UserCollectionLoader);
            CellTracker = new CellTracker(SheetTracker, CellTriggerManager, CellPopulateManager, CellLoader);
            ApplicationSettings = ApplicationSettings.CreateInstance(PersistenceManager);
            sheetViewModel = SheetViewModelFactory.GetOrCreate(ApplicationSettings.LastLoadedSheet);
            UndoRedoManager = new(CellTracker);
            _cellClipboard = new(UndoRedoManager, CellTracker);
            ApplicationView = view;
        }

        public static ApplicationViewModel Instance { get => instance ?? throw new NullReferenceException("Application instance not set"); private set => instance = value ?? throw new NullReferenceException("Static instances not allowed to be null"); }

        public static ApplicationViewModel? SafeInstance => instance;

        public ApplicationSettings ApplicationSettings { get; private set; }

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

        public CellLoader CellLoader { get; private set; }

        public CellPopulateManager CellPopulateManager { get; private set; }

        public CellTracker CellTracker { get; private set; }

        public CellTriggerManager CellTriggerManager { get; private set; }

        public PersistenceManager PersistenceManager { get; private set; }

        public PluginFunctionLoader PluginFunctionLoader { get; private set; }

        public SheetTracker SheetTracker { get; private set; }

        public SheetViewModel SheetViewModel
        {
            get { return sheetViewModel; }
            set
            {
                sheetViewModel = value;
                NotifyPropertyChanged(nameof(SheetViewModel));
            }
        }

        public TitleBarSheetNavigationViewModel TitleBarSheetNavigationViewModel { get; private set; }

        public UndoRedoManager UndoRedoManager { get; private set; }

        public UserCollectionLoader UserCollectionLoader { get; private set; }

        public static ApplicationViewModel GetOrCreateInstance(ApplicationView mainWindow)
        {
            instance ??= new ApplicationViewModel(mainWindow);
            return instance;
        }

        public static UndoRedoManager? GetUndoRedoManager()
        {
            if (instance == null)
            {
                return null;
            }
            return Instance.UndoRedoManager;
        }

        public void ChangeSelectedCellsType(CellType newType)
        {
            UndoRedoManager.StartRecordingUndoState();
            SheetViewModel.SelectedCellViewModels.Select(x => x.Model).ToList().ForEach(UndoRedoManager.RecordStateIfRecording);
            UndoRedoManager.FinishRecordingUndoState();
            var selectedCells = SheetViewModel.SelectedCellViewModels.ToList();
            foreach (var selectedCell in selectedCells)
            {
                selectedCell.CellType = newType;
            }
            selectedCells.ForEach(x => SheetViewModel.SelectCell(x.Model));
            SheetViewModel.UpdateLayout();
        }

        public void Load()
        {
            var versionSchema = PersistenceManager.LoadVersion();
            if (PersistenceManager.Version != versionSchema) throw new CellError($"Error: The project you are trying to load need to be migrated from version {versionSchema} to version {PersistenceManager.Version}.");
            PersistenceManager.SaveVersion();
            UserCollectionLoader.LoadCollections();
            PluginFunctionLoader.LoadPlugins();
            UserCollectionLoader.LinkUpBaseCollectionsAfterLoad();
            CellLoader.LoadAndAddCells();
            PersistenceManager.CreateBackup();
            SheetViewModel.LoadCellViewModels();
            PropertyChanged += ApplicationView.ApplicationViewModelPropertyChanged;
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
            if (!SheetModel.IsValidSheetName(sheetName)) return;
            if (SheetViewModel.SheetName == sheetName) return;
            SheetViewModel = SheetViewModelFactory.GetOrCreate(sheetName);
            if (!sheetViewModel.CellViewModels.Any()) sheetViewModel.LoadCellViewModels();
            ApplicationView.ShowSheetView(sheetViewModel);
            ApplicationSettings.LastLoadedSheet = sheetName;
        }

        internal void PasteCopiedCells()
        {
            UndoRedoManager.StartRecordingUndoState();
            _cellClipboard.PasteCopiedCells(SheetViewModel);
            UndoRedoManager.FinishRecordingUndoState();
        }
    }
}
