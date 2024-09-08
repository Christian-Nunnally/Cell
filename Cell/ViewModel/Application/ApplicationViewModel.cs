using Cell.Common;
using Cell.Data;
using Cell.Execution;
using Cell.Model;
using Cell.Persistence;
using Cell.View.Application;
using Cell.View.Cells;
using Cell.View.ToolWindow;
using Cell.ViewModel.Cells;
using System.IO;
using System.Windows.Controls;

namespace Cell.ViewModel.Application
{
    public class ApplicationViewModel : PropertyChangedBase
    {
        public bool IsProjectLoaded
        {
            get => _isProjectLoaded;
            set
            {
                if (_isProjectLoaded == value) return;
                _isProjectLoaded = value;
                NotifyPropertyChanged(nameof(IsProjectLoaded));
            }
        }

        private ApplicationView? _applicationView;
        private readonly CellClipboard _cellClipboard;
        private static ApplicationViewModel? instance;
        private double _applicationWindowHeight = 1300;
        private double _applicationWindowWidth = 1200;
        private SheetViewModel? sheetViewModel;
        private bool _isProjectLoaded;

        private ApplicationViewModel()
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
        public SheetView? ActiveSheetView => _applicationView?.ActiveSheetView;

        private readonly Dictionary<SheetModel, SheetViewModel> _sheetModelToViewModelMap = [];

        public static ApplicationViewModel GetOrCreateInstance()
        {
            instance ??= new ApplicationViewModel();
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
            var progress = LoadWithProgress();
            while (!progress.IsComplete) progress = progress.Continue();
        }

        public LoadingProgressResult LoadWithProgress()
        {
            if (IsProjectLoaded) return new LoadingProgressResult(true, "Already loaded");
            return new LoadingProgressResult("Checking Save Version", LoadPhase1);
        }

        private LoadingProgressResult LoadPhase1()
        {
            var versionSchema = PersistenceManager.LoadVersion();
            if (PersistenceManager.Version != versionSchema)
            {
                DialogWindow.ShowYesNoConfirmationDialog(
                    "Version Mismatch",
                    $"The version of the data is {versionSchema} but the application is expecting {PersistenceManager.Version}. Do you want to attempt to migrate?",
                    () => MigrateSave(versionSchema, PersistenceManager.Version),
                    () => { Environment.Exit(0); });
            }

            PersistenceManager.SaveVersion();
            return new LoadingProgressResult("Loading Collections", LoadPhase2);
        }

        private LoadingProgressResult LoadPhase2()
        {
            UserCollectionLoader.LoadCollections();
            return new LoadingProgressResult("Loading Plugins", LoadPhase3);
        }

        private LoadingProgressResult LoadPhase3()
        {
            PluginFunctionLoader.LoadPlugins();
            return new LoadingProgressResult("Linking Collections to Bases", LoadPhase4);
        }

        private LoadingProgressResult LoadPhase4()
        {
            UserCollectionLoader.LinkUpBaseCollectionsAfterLoad();
            return new LoadingProgressResult("Loading Cells", LoadPhase5);
        }

        private LoadingProgressResult LoadPhase5()
        {
            var cells = CellLoader.LoadCells();
            foreach (var cell in cells)
            {
                CellTracker.AddCell(cell, false);
            }
            return new LoadingProgressResult("Creating Backup", LoadPhase6);
        }

        private LoadingProgressResult LoadPhase6()
        {
            BackupManager.CreateBackup();
            IsProjectLoaded = true;
            return new LoadingProgressResult(true, "Load Complete");
        }

        public void MigrateSave(string fromVersion, string toVersion)
        {

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
            if (cell is not null) ActiveSheetView?.PanCanvasTo(cell.X, cell.Y);
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
            _applicationView?.ShowSheetView(SheetViewModel);
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

        public void ShowToolWindow(UserControl content, bool allowDuplicates = false)
        {
            _applicationView?.ShowToolWindow(content, allowDuplicates);
        }

        public void AttachToView(ApplicationView applicationView)
        {
            _applicationView = applicationView;
            _applicationView.DataContext = this;
        }
    }
}
