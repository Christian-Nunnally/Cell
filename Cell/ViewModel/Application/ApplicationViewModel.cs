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
        private SheetViewModel? sheetViewModel;
        private ApplicationViewModel(ApplicationView view)
        {
            PersistenceManager = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LGF", "Cell"), new FileIO());
            PluginFunctionLoader = new(PersistenceManager);
            CellLoader = new(PersistenceManager);
            CellTracker = new CellTracker(CellLoader);
            UserCollectionLoader = new(PersistenceManager, PluginFunctionLoader, CellTracker);
            CellPopulateManager = new(CellTracker, PluginFunctionLoader, UserCollectionLoader);
            CellTriggerManager = new(CellTracker, PluginFunctionLoader, UserCollectionLoader);
            SheetTracker = new(PersistenceManager, CellLoader, CellTracker, PluginFunctionLoader, UserCollectionLoader);
            TitleBarSheetNavigationViewModel = new(SheetTracker);
            ApplicationSettings = ApplicationSettings.CreateInstance(PersistenceManager);
            UndoRedoManager = new(CellTracker);
            _cellClipboard = new(UndoRedoManager, CellTracker, new TextClipboard());
            BackupManager = new(PersistenceManager, CellTracker, SheetTracker, UserCollectionLoader, PluginFunctionLoader);
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

        public CellLoader CellLoader { get; private set; } // Good

        public CellPopulateManager CellPopulateManager { get; private set; } // Good

        public CellTracker CellTracker { get; private set; } // Good

        public CellTriggerManager CellTriggerManager { get; private set; } // Good

        public PersistenceManager PersistenceManager { get; private set; } // Good

        public PluginFunctionLoader PluginFunctionLoader { get; private set; }

        public BackupManager BackupManager { get; private set; } // Good

        public SheetTracker SheetTracker { get; private set; } // Good

        public SheetViewModel? SheetViewModel
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

        private readonly Dictionary<SheetModel, SheetViewModel> _sheetModelToViewModelMap = [];

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
            if (SheetViewModel == null) return;
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
            var cells = CellLoader.LoadCells();
            foreach (var cell in cells)
            {
                CellTracker.AddCell(cell, false);
            }
            BackupManager.CreateBackup();
            PropertyChanged += ApplicationView.ApplicationViewModelPropertyChanged;
        }

        internal void CopySelectedCells(bool copyTextOnly)
        {
            if (SheetViewModel == null) return;
            _cellClipboard.CopyCells(SheetViewModel.SelectedCellViewModels.Select(x => x.Model), copyTextOnly);
        }

        internal void GoToCell(CellModel cellModel)
        {
            GoToSheet(cellModel.SheetName);
            var cell = SheetViewModel?.CellViewModels.FirstOrDefault(x => x.Model.ID == cellModel.ID);
            if (cell is not null) ApplicationView.ActiveSheetView?.PanAndZoomCanvas?.PanCanvasTo(cell.X, cell.Y);
        }

        internal void GoToSheet(string sheetName)
        {
            if (!SheetModel.IsValidSheetName(sheetName)) return;
            if (SheetViewModel?.SheetName == sheetName) return;
            var sheet = SheetTracker.Sheets.FirstOrDefault(x => x.Name == sheetName);
            if (sheet == null) return;
            if (_sheetModelToViewModelMap.TryGetValue(sheet, out SheetViewModel? existingSheetViewModel))
            {
                SheetViewModel = existingSheetViewModel;
            }
            else
            {
                SheetViewModel = SheetViewModelFactory.Create(sheet, CellPopulateManager, CellTracker, SheetTracker, UserCollectionLoader);
                _sheetModelToViewModelMap.Add(sheet, SheetViewModel);
            }
            ApplicationView.ShowSheetView(SheetViewModel);
            ApplicationSettings.LastLoadedSheet = sheetName;
        }

        internal void PasteCopiedCells()
        {
            if (SheetViewModel == null) return;
            UndoRedoManager.StartRecordingUndoState();
            if (SheetViewModel.SelectedCellViewModel != null) _cellClipboard.PasteIntoCells(SheetViewModel.SelectedCellViewModel.Model, SheetViewModel.SelectedCellViewModels.Select(x => x.Model));
            SheetViewModel.UpdateLayout();
            UndoRedoManager.FinishRecordingUndoState();
        }
    }
}
