using Cell.Common;
using Cell.Data;
using Cell.Execution;
using Cell.Model;
using Cell.Persistence;
using Cell.View.Application;
using Cell.View.Cells;
using Cell.ViewModel.Cells;
using Cell.ViewModel.ToolWindow;

namespace Cell.ViewModel.Application
{
    public class ApplicationViewModel : PropertyChangedBase
    {
        public const string NoMigratorForVersionError = "Unable to load version";
        private readonly CellClipboard _cellClipboard;
        private readonly Dictionary<SheetModel, SheetViewModel> _sheetModelToViewModelMap = [];
        private static ApplicationViewModel? _instance;
        private ApplicationView? _applicationView;
        private double _applicationWindowHeight = 1300;
        private double _applicationWindowWidth = 1200;
        private Task _backupTask = Task.CompletedTask;
        private bool _isProjectLoaded;
        private bool _isProjectLoading;
        private SheetViewModel? _sheetViewModel;
        public ApplicationViewModel(
            PersistedDirectory persistedDirectory,
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
            PersistenceManager = persistedDirectory;
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

        public static ApplicationViewModel Instance { get => _instance ?? throw new NullReferenceException("Application instance not set"); set => _instance = value ?? throw new NullReferenceException("Static instances not allowed to be null"); }

        public static ApplicationViewModel? SafeInstance => _instance;

        public SheetView? ActiveSheetView => _applicationView?.ActiveSheetView;

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

        public BackupManager BackupManager { get; private set; }

        public CellLoader CellLoader { get; private set; }

        public CellPopulateManager CellPopulateManager { get; private set; }

        public CellSelector CellSelector { get; private set; }

        public CellTracker CellTracker { get; private set; }

        public CellTriggerManager CellTriggerManager { get; private set; }

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

        public PersistedProject PersistedProject { get; private set; }

        public PersistedDirectory PersistenceManager { get; private set; }

        public PluginFunctionLoader PluginFunctionLoader { get; private set; }

        public SheetTracker SheetTracker { get; private set; }

        public SheetViewModel? SheetViewModel
        {
            get { return _sheetViewModel; }
            set
            {
                _sheetViewModel = value;
                NotifyPropertyChanged(nameof(SheetViewModel));
            }
        }

        public TitleBarSheetNavigationViewModel TitleBarSheetNavigationViewModel { get; private set; }

        public UndoRedoManager UndoRedoManager { get; private set; }

        public UserCollectionLoader UserCollectionLoader { get; private set; }

        public static UndoRedoManager? GetUndoRedoManager()
        {
            if (_instance == null) return null;
            return Instance.UndoRedoManager;
        }

        public void AttachToView(ApplicationView applicationView)
        {
            _applicationView = applicationView;
            _applicationView.DataContext = this;
        }

        public void CopySelectedCells(bool copyTextOnly)
        {
            if (SheetViewModel == null) return;
            _cellClipboard.CopyCells(SheetViewModel.CellSelector.SelectedCells, copyTextOnly);
        }

        public void GoToCell(CellModel cellModel)
        {
            GoToSheet(cellModel.Location.SheetName);
            var cell = SheetViewModel?.CellViewModels.FirstOrDefault(x => x.Model.ID == cellModel.ID);
            if (cell is not null) ActiveSheetView?.PanCanvasTo(cell.X, cell.Y);
        }

        public void GoToSheet(string sheetName)
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
                SheetViewModel = new SheetViewModel(sheet, CellPopulateManager, CellTracker, SheetTracker, CellSelector, UserCollectionLoader, ApplicationSettings, PluginFunctionLoader);
                _sheetModelToViewModelMap.Add(sheet, SheetViewModel);
            }
            _applicationView?.ShowSheetView(SheetViewModel);
            ApplicationSettings.LastLoadedSheet = sheetName;
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
            return new LoadingProgressResult("Checking for migration", LoadPhase1);
        }

        public void PasteCopiedCells()
        {
            if (SheetViewModel == null) return;
            UndoRedoManager.StartRecordingUndoState();
            if (SheetViewModel.SelectedCellViewModel != null) _cellClipboard.PasteIntoCells(SheetViewModel.SelectedCellViewModel.Model, SheetViewModel.CellSelector.SelectedCells);
            SheetViewModel.UpdateLayout();
            UndoRedoManager.FinishRecordingUndoState();
        }

        public void ShowToolWindow(ToolWindowViewModel viewModel, bool allowDuplicates = false)
        {
            _applicationView?.ShowToolWindow(viewModel, allowDuplicates);
        }

        private async Task BackupAsync()
        {
            await Task.Run(() =>
            {
                PersistedProject.IsReadOnly = true;
                BackupManager.CreateBackup();
                PersistedProject.IsReadOnly = false;
            });
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
            _backupTask = BackupAsync();
            return new LoadingProgressResult("Loading Collections", LoadPhase2);
        }

        private LoadingProgressResult LoadPhase2()
        {
            UserCollectionLoader.LoadCollections();
            return new LoadingProgressResult("Loading Plugins", LoadPhase3);
        }

        private LoadingProgressResult LoadPhase3()
        {
            PluginFunctionLoader.LoadCellFunctions();
            return new LoadingProgressResult("Linking Collections to Bases", LoadPhase4);
        }

        private LoadingProgressResult LoadPhase4()
        {
            UserCollectionLoader.LinkUpBaseCollectionsAfterLoad();
            return new LoadingProgressResult("Loading Cells", LoadPhase5);
        }

        private LoadingProgressResult LoadPhase5()
        {
            CellPopulateManager.UpdateCellsWhenANewCellIsAdded = false;
            var cells = CellLoader.LoadCells();
            foreach (var cell in cells)
            {
                CellTracker.AddCell(cell, false);
            }
            CellPopulateManager.UpdateCellsWhenANewCellIsAdded = true;
            return new LoadingProgressResult("Waiting for backup to complete", LoadPhase6);
        }

        private LoadingProgressResult LoadPhase6()
        {
            _backupTask.Wait();
            IsProjectLoaded = true;
            _isProjectLoading = false;
            return new LoadingProgressResult(true, "Load Complete");
        }

        private void MigrateProject()
        {
            BackupManager.CreateBackup("PreMigration");
            PersistedProject.Migrate();
            DialogFactory.ShowDialog("Project migrated", "Project has been migrated to the latest version, try clicking 'Load Project' again.");
        }
    }
}
