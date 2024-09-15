using Cell.Common;
using Cell.Data;
using Cell.Execution;
using Cell.Model;
using Cell.Persistence;
using Cell.View.Application;
using Cell.View.Cells;
using Cell.ViewModel.Cells;
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

        public const string NoMigratorForVersionError = "Unable to load version";
        private ApplicationView? _applicationView;
        private readonly CellClipboard _cellClipboard;
        private static ApplicationViewModel? instance;
        private double _applicationWindowHeight = 1300;
        private double _applicationWindowWidth = 1200;
        private SheetViewModel? sheetViewModel;
        private bool _isProjectLoaded;
        private bool _isProjectLoading;

        public ApplicationViewModel(
            PersistedDirectory persistenceManager, 
            PersistedProject persistedProject,
            PluginFunctionLoader pluginFunctionLoader, 
            CellLoader cellLoader, 
            CellTracker cellTracker, 
            UserCollectionLoader userCollectionLoader, 
            CellPopulateManager cellPopulateManager, 
            CellTriggerManager cellTriggerManager, 
            SheetTracker sheetTracker, 
            CellSelector cellSelector,
            TitleBarSheetNavigationViewModel titleBarSheetNavigationViewModel,
            ApplicationSettings applicationSettings,
            UndoRedoManager undoRedoManager,
            CellClipboard cellClipboard,
            BackupManager backupManager)
        {
            PersistenceManager = persistenceManager;
            PersistedProject = persistedProject;
            PluginFunctionLoader = pluginFunctionLoader;
            CellLoader = cellLoader;
            CellTracker = cellTracker;
            UserCollectionLoader = userCollectionLoader;
            CellPopulateManager = cellPopulateManager;
            CellTriggerManager = cellTriggerManager;
            SheetTracker = sheetTracker;
            CellSelector = cellSelector;
            TitleBarSheetNavigationViewModel = titleBarSheetNavigationViewModel;
            ApplicationSettings = applicationSettings;
            UndoRedoManager = undoRedoManager;
            _cellClipboard = cellClipboard;
            BackupManager = backupManager;
        }

        public static ApplicationViewModel Instance { get => instance ?? throw new NullReferenceException("Application instance not set"); set => instance = value ?? throw new NullReferenceException("Static instances not allowed to be null"); }

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

        public PersistedProject PersistedProject { get; private set; }

        public CellTriggerManager CellTriggerManager { get; private set; }

        public PersistedDirectory PersistenceManager { get; private set; }

        public PluginFunctionLoader PluginFunctionLoader { get; private set; }

        public BackupManager BackupManager { get; private set; }

        public SheetTracker SheetTracker { get; private set; }

        public CellSelector CellSelector{ get; private set; }

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

        public static UndoRedoManager? GetUndoRedoManager()
        {
            if (instance == null) return null;
            return Instance.UndoRedoManager;
        }

        public LoadingProgressResult Load()
        {
            var progress = LoadWithProgress();
            while (!progress.IsComplete) progress = progress.Continue();
            return progress;
        }

        public LoadingProgressResult LoadWithProgress()
        {
            if (IsProjectLoaded) return new LoadingProgressResult(true, "Already loaded");
            if (_isProjectLoading) return new LoadingProgressResult(true, "Already loading");
            _isProjectLoading = true;
            return new LoadingProgressResult("Creating backup", LoadPhase0);
        }

        private LoadingProgressResult LoadPhase0()
        {
            BackupManager.CreateBackup();
            return new LoadingProgressResult("Checking Save Version", LoadPhase1);
        }

        private LoadingProgressResult LoadPhase1()
        {
            if (PersistedProject.NeedsMigration())
            {
                if (!PersistedProject.CanMigrate()) return new LoadingProgressResult(false, NoMigratorForVersionError);
                DialogFactory.ShowYesNoConfirmationDialog(
                    "Version Mismatch",
                    $"The version of your project is outdated. would you like to migrate it?",
                    MigrateProject,
                    () => { });
                return new LoadingProgressResult(false, "Migration needed, try loading again.");
            }

            PersistedProject.SaveVersion();
            return new LoadingProgressResult("Loading Collections", LoadPhase2);
        }

        private void MigrateProject()
        {
            BackupManager.CreateBackup("PreMigration");
            PersistedProject.Migrate();
            DialogFactory.ShowDialog("Project migrated", "Project has been migrated to the latest version, try clicking 'Load Project' again.");
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
            IsProjectLoaded = true;
            _isProjectLoading = false;
            return new LoadingProgressResult(true, "Load Complete");
        }

        internal void CopySelectedCells(bool copyTextOnly)
        {
            if (SheetViewModel == null) return;
            _cellClipboard.CopyCells(SheetViewModel.CellSelector.SelectedCells, copyTextOnly);
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
            SheetViewModel?.CellSelector.UnselectAllCells();
            if (_sheetModelToViewModelMap.TryGetValue(sheet, out SheetViewModel? existingSheetViewModel))
            {
                SheetViewModel = existingSheetViewModel;
            }
            else
            {
                SheetViewModel = SheetViewModelFactory.Create(sheet, CellPopulateManager, CellTracker, SheetTracker, CellSelector, UserCollectionLoader, ApplicationSettings, PluginFunctionLoader);
                _sheetModelToViewModelMap.Add(sheet, SheetViewModel);
            }
            _applicationView?.ShowSheetView(SheetViewModel);
            ApplicationSettings.LastLoadedSheet = sheetName;
        }

        internal void PasteCopiedCells()
        {
            if (SheetViewModel == null) return;
            UndoRedoManager.StartRecordingUndoState();
            if (SheetViewModel.SelectedCellViewModel != null) _cellClipboard.PasteIntoCells(SheetViewModel.SelectedCellViewModel.Model, SheetViewModel.CellSelector.SelectedCells);
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
