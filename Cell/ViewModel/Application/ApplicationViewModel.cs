using Cell.Core.Common;
using Cell.Core.Data;
using Cell.Core.Execution;
using Cell.Model;
using Cell.Core.Persistence;
using Cell.ViewModel.Cells;
using Cell.ViewModel.ToolWindow;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace Cell.ViewModel.Application
{
    /// <summary>
    /// The view model for the cell application.
    /// </summary>
    public class ApplicationViewModel : PropertyChangedBase
    {
        /// <summary>
        /// Error message shown when a migration is needed but no migrator is available.
        /// </summary>
        public const string NoMigratorForVersionError = "Unable to load version";
        public CellLoader? CellLoader;
        public CellTriggerManager? CellTriggerManager;
        private readonly Dictionary<SheetModel, SheetViewModel> _sheetModelToViewModelMap = [];
        private static ApplicationViewModel? _instance;
        private double _applicationWindowHeight = 1300;
        private double _applicationWindowWidth = 1200;
        private Task? _backupTask;
        private bool _isProjectLoaded;
        private bool _isProjectLoading;
        private SheetViewModel? _sheetViewModel;
        private TitleBarSheetNavigationViewModel? titleBarSheetNavigationViewModel;

        /// <summary>
        /// Creates a new instance of <see cref="ApplicationViewModel"/>.
        /// </summary>
        public ApplicationViewModel()
        {
            Instance = this;
        }

        /// <summary>
        /// Gets the current instance of the application view model.
        /// </summary>
        public static ApplicationViewModel Instance { get => _instance ?? throw new NullReferenceException("Application instance not set"); set => _instance = value ?? throw new NullReferenceException("Static instances not allowed to be null"); }

        /// <summary>
        /// Gets the current instance of the application view model, or null if it is not set.
        /// </summary>
        public static ApplicationViewModel? SafeInstance => _instance;

        /// <summary>
        /// Gets the persisted application settings for the application.
        /// </summary>
        public ApplicationSettings? ApplicationSettings { get; set; }

        /// <summary>
        /// Gets or sets the view model for the title bar sheet navigation.
        /// </summary>
        public TitleBarSheetNavigationViewModel? TitleBarSheetNavigationViewModel
        {
            get => titleBarSheetNavigationViewModel; set
            {
                if (titleBarSheetNavigationViewModel == value) return;
                titleBarSheetNavigationViewModel = value;
                NotifyPropertyChanged(nameof(TitleBarSheetNavigationViewModel));
            }
        }

        /// <summary>
        /// A factory for showing dialogs.
        /// </summary>
        public DialogFactoryBase? DialogFactory { get; set; }

        /// <summary>
        /// Enables copy and paste though this clipboard.
        /// </summary>
        public CellClipboard? CellClipboard { get; set; }

        /// <summary>
        /// Gets or sets the height of the application window.
        /// </summary>
        public double ApplicationWindowHeight
        {
            get { return _applicationWindowHeight; }
            set
            {
                _applicationWindowHeight = value;
                NotifyPropertyChanged(nameof(ApplicationWindowHeight));
            }
        }

        /// <summary>
        /// Gets or sets the width of the application window.
        /// </summary>
        public double ApplicationWindowWidth
        {
            get { return _applicationWindowWidth; }
            set
            {
                _applicationWindowWidth = value;
                NotifyPropertyChanged(nameof(ApplicationWindowWidth));
            }
        }

        public event Action<ToolWindowViewModel>? MoveToolWindowToTop;

        public void MoveWindowToTop(ToolWindowViewModel toolWindow)
        {
            MoveToolWindowToTop?.Invoke(toolWindow);
        }

        /// <summary>
        /// Gets the backup manager for the application, which is used to create backups of the project.
        /// </summary>
        public BackupManager? BackupManager { get; set; }

        /// <summary>
        /// Gets the populator for the application, which is used to auto populate cells in the application.
        /// </summary>
        public CellPopulateManager? CellPopulateManager { get; set; }

        /// <summary>
        /// The cell selector for the application, which is used to select cells in the application.
        /// </summary>
        public CellSelector? CellSelector { get; set; }

        /// <summary>
        /// The cell tracker for the application, which is used to store all of the cells in the application.
        /// </summary>
        public CellTracker? CellTracker { get; set; }

        /// <summary>
        /// Gets or sets whether a project is currently loaded and ready to be interacted with.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the persisted project for the application, which is used to save and load the project.
        /// </summary>
        public PersistedProject? PersistedProject { get; set; }

        /// <summary>
        /// Gets the plugin function loader for the application, which loads and stores all plugin functions.
        /// </summary>
        public FunctionLoader? FunctionLoader { get; set; }

        public FunctionTracker? FunctionTracker { get; set; }

        /// <summary>
        /// Gets the sheet tracker for the application, which is used to store all of the sheets in the application.
        /// </summary>
        public SheetTracker? SheetTracker { get; set; }

        /// <summary>
        /// Gets or sets the current sheet view model that is being displayed in the application.
        /// </summary>
        public SheetViewModel? SheetViewModel
        {
            get { return _sheetViewModel; }
            set
            {
                if (_sheetViewModel == value) return;
                _sheetViewModel = value;
                NotifyPropertyChanged(nameof(SheetViewModel));
            }
        }

        /// <summary>
        /// Gets the application wide undo redo manager.
        /// </summary>
        public UndoRedoManager? UndoRedoManager { get; set; }

        /// <summary>
        /// Gets the observable collection of open tool windows in the application.
        /// </summary>
        public ObservableCollection<ToolWindowViewModel> OpenToolWindowViewModels { get; } = [];

        /// <summary>
        /// Gets the user collection loader for the application, which loads and stores all user collections.
        /// </summary>
        public UserCollectionLoader? UserCollectionLoader { get; set; }

        /// <summary>
        /// Gets the application wide undo redo manager.
        /// </summary>
        /// <returns>The global undo/redo manager.</returns>
        public static UndoRedoManager? GetUndoRedoManager()
        {
            if (_instance == null) return null;
            return Instance.UndoRedoManager;
        }

        /// <summary>
        /// Copies the selected cells to the clipboard.
        /// </summary>
        /// <param name="copyTextOnly">Whether to only copy the text of the cells.</param>
        public void CopySelectedCells(bool copyTextOnly)
        {
            if (CellClipboard is null)
            {
                
            }
            if (SheetViewModel == null) return;
            CellClipboard?.CopyCells(SheetViewModel.CellSelector.SelectedCells, copyTextOnly);
        }

        /// <summary>
        /// Opens the sheet that contains the given cell and pans to the given cell.
        /// </summary>
        /// <param name="cellModel">The cell to show and center.</param>
        public void GoToCell(CellModel cellModel)
        {
            GoToSheet(cellModel.Location.SheetName);
            if (SheetViewModel is null) return;
            var cell = SheetViewModel.CellViewModels.FirstOrDefault(x => x.Model.ID == cellModel.ID);
            if (cell is null) return;
            SheetViewModel.PanX = cell.X;
            SheetViewModel.PanY = cell.Y;
        }

        /// <summary>
        /// Opens the sheet with the given name in the application.
        /// </summary>
        /// <param name="sheetName">The name of the sheet to show.</param>
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
                SheetViewModel = new SheetViewModel(sheet, CellPopulateManager, CellTriggerManager, CellTracker, CellSelector, FunctionTracker);
                _sheetModelToViewModelMap.Add(sheet, SheetViewModel);
            }
            ApplicationSettings.LastLoadedSheet = sheetName;
        }

        /// <summary>
        /// Loads the entire project and then returns.
        /// </summary>
        /// <returns>The finish loading progress, which might be mark incomplete if the load did not finish.</returns>
        public LoadingProgressResult Load()
        {
            var progress = LoadWithProgress();
            while (!progress.IsComplete) progress = progress.Continue();
            return progress;
        }

        /// <summary>
        /// Starts loading the project and returns a result that can be used to continue the loading process.
        /// </summary>
        /// <returns>The Loading progress object.</returns>
        public LoadingProgressResult LoadWithProgress()
        {
            if (IsProjectLoaded) return new LoadingProgressResult(true, "Already loaded");
            if (_isProjectLoading) return new LoadingProgressResult(true, "Already loading");
            _isProjectLoading = true;
            return new LoadingProgressResult("Checking for migration", LoadPhase1);
        }

        /// <summary>
        /// Pastes the copied cells into the selected cells.
        /// </summary>
        public void PasteCopiedCells()
        {
            if (SheetViewModel == null) return;
            UndoRedoManager?.StartRecordingUndoState();
            CellClipboard?.PasteIntoCells(SheetViewModel.CellSelector.SelectedCells);
            SheetViewModel.UpdateLayout();
            UndoRedoManager?.FinishRecordingUndoState();
        }

        /// <summary>
        /// Shows the given tool window.
        /// </summary>
        /// <param name="viewModel">The view model for the view to display.</param>
        /// <param name="allowDuplicates">Whether or not to actually open the window if one of the same type is already open.</param>
        public void ShowToolWindow(ToolWindowViewModel viewModel, bool allowDuplicates = false)
        {
            var existingWindowOfSameType = OpenToolWindowViewModels.FirstOrDefault(x => viewModel.GetType() == x.GetType());
            if (existingWindowOfSameType is not null)
            {
                OpenToolWindowViewModels.Remove(existingWindowOfSameType);
                OpenToolWindowViewModels.Add(existingWindowOfSameType);
                if (!allowDuplicates) return;
            }
            viewModel.RequestClose = () => RequestClose(viewModel);
            viewModel.HandleBeingShown();
            OpenToolWindowViewModels.Add(viewModel);
        }

        /// <summary>
        /// Shows the given tool window in the main dock panel.
        /// </summary>
        /// <param name="viewModel">The view model for the view to display.</param>
        /// <param name="dock">The side to put the window on.</param>
        /// <param name="allowDuplicates">Whether or not to actually open the window if one of the same type is already open.</param>
        public void DockToolWindow(ToolWindowViewModel viewModel, Dock dock, bool allowDuplicates = false)
        {
            if (!allowDuplicates && OpenToolWindowViewModels.Any(x => viewModel.GetType() == x.GetType())) return;
            viewModel.Dock = dock;
            viewModel.IsDocked = true;
            viewModel.RequestClose = () => RequestClose(viewModel);
            viewModel.HandleBeingShown();
            OpenToolWindowViewModels.Add(viewModel);
        }

        private void RequestClose(ToolWindowViewModel viewModel)
        {
            var isAllowingClose = viewModel.HandleCloseRequested();
            if (isAllowingClose)
            {
                RemoveToolWindow(viewModel);
            }
        }

        private async Task BackupAsync()
        {
            if (PersistedProject == null) return;
            PersistedProject.IsReadOnly = true;
            BackupManager!.CreateBackup();
            PersistedProject.IsReadOnly = false;
        }

        private bool _hasVersionBeenSaved = false;
        public void EnsureInitialBackupIsStarted()
        {
            if (!_hasVersionBeenSaved) { PersistedProject.SaveVersion(); _hasVersionBeenSaved = true; }
            _backupTask ??= Task.Run(BackupAsync);
        }

        private LoadingProgressResult LoadPhase1()
        {
            if (BackupManager is null) return new LoadingProgressResult(false, "Backup manager not initialized yet, try loading again.");
            if (PersistedProject.NeedsMigration())
            {
                if (!PersistedProject.CanMigrate()) return new LoadingProgressResult(false, NoMigratorForVersionError);
                DialogFactory.ShowYesNo(
                    "Version Mismatch",
                    $"The version of your project is outdated. would you like to migrate it?",
                    MigrateProject,
                    () => { });
                return new LoadingProgressResult(false, "Migration needed, try loading again.");
            }

            EnsureInitialBackupIsStarted();
            return new LoadingProgressResult("Loading Collections", LoadPhase2);
        }

        private LoadingProgressResult LoadPhase2()
        {
            UserCollectionLoader.LoadCollections();
            return new LoadingProgressResult("Loading Plugins", LoadPhase3);
        }

        private LoadingProgressResult LoadPhase3()
        {
            FunctionLoader.LoadCellFunctions();
            return new LoadingProgressResult("Linking Collections to Bases", LoadPhase4);
        }

        private LoadingProgressResult LoadPhase4()
        {
            UserCollectionLoader.LinkUpBaseCollectionsAfterLoad();
            return new LoadingProgressResult("Loading Cells", LoadPhase5);
        }

        private LoadingProgressResult LoadPhase5()
        {
            CellLoader.LoadCells();
            return new LoadingProgressResult("Initializing populate manager", LoadPhase5b);
        }

        private LoadingProgressResult LoadPhase5b()
        {
            CellPopulateManager = new CellPopulateManager(CellTracker, FunctionTracker, UserCollectionLoader);
            return new LoadingProgressResult("Waiting for backup to complete", LoadPhase6);
        }

        private LoadingProgressResult LoadPhase6()
        {
            if (_backupTask is null) return new LoadingProgressResult(false, "Aborted the load because a backup has no yet been started, and I want to backup before loading to protect data.");
            _backupTask.Wait();
            IsProjectLoaded = true;
            _isProjectLoading = false;
            return new LoadingProgressResult(true, "Load Complete");
        }

        private void MigrateProject()
        {
            BackupManager.CreateBackup("PreMigration");
            PersistedProject.Migrate();
            _isProjectLoading = false;
            DialogFactory.Show("Project migrated", "Project has been migrated to the latest version, try clicking 'Load Project' again.");
        }

        internal void RemoveToolWindow(ToolWindowViewModel toolWindowViewModel)
        {
            OpenToolWindowViewModels.Remove(toolWindowViewModel);
            toolWindowViewModel.HandleBeingClosed();
        }
    }
}
