using Cell.Core.Common;
using Cell.Core.Data;
using Cell.Core.Execution;
using Cell.Model;
using Cell.Core.Persistence;
using Cell.View.Application;
using Cell.View.Cells;
using Cell.ViewModel.Cells;
using Cell.ViewModel.ToolWindow;
using System.Windows.Controls;

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
        private readonly CellClipboard _cellClipboard;
        private readonly CellLoader _cellLoader;
        private readonly CellTriggerManager _cellTriggerManager;
        private readonly Dictionary<SheetModel, SheetViewModel> _sheetModelToViewModelMap = [];
        private static ApplicationViewModel? _instance;
        private ApplicationView? _applicationView;
        private double _applicationWindowHeight = 1300;
        private double _applicationWindowWidth = 1200;
        private Task _backupTask = Task.CompletedTask;
        private bool _isProjectLoaded;
        private bool _isProjectLoading;
        private SheetViewModel? _sheetViewModel;
        /// <summary>
        /// Creates a new instance of <see cref="ApplicationViewModel"/>.
        /// </summary>
        /// <param name="persistedProject">The project to load in the application.</param>
        /// <param name="pluginFunctionLoader">The function loader for functions.</param>
        /// <param name="cellLoader">The cell loader for loading cells.</param>
        /// <param name="cellTracker">The cell tracker for storing cells.</param>
        /// <param name="userCollectionLoader">The collection loader for user collections.</param>
        /// <param name="cellPopulateManager">The populate manager for populate functions.</param>
        /// <param name="cellTriggerManager">The trigger manager for trigger functions.</param>
        /// <param name="sheetTracker">The sheet tracker for handling sheets.</param>
        /// <param name="cellSelector">The cell selector for handling cell selection.</param>
        /// <param name="applicationSettings">The application settings that get persisted to disk.</param>
        /// <param name="undoRedoManager">The undo redo manager for the application.</param>
        /// <param name="cellClipboard">The clipboard to copy and paste with.</param>
        /// <param name="backupManager">The backup manager to backup the project with.</param>
        public ApplicationViewModel(
            PersistedProject persistedProject,
            PluginFunctionLoader pluginFunctionLoader,
            CellLoader cellLoader,
            CellTracker cellTracker,
            UserCollectionLoader userCollectionLoader,
            CellPopulateManager cellPopulateManager,
            CellTriggerManager cellTriggerManager,
            SheetTracker sheetTracker,
            CellSelector cellSelector,
            ApplicationSettings applicationSettings,
            UndoRedoManager undoRedoManager,
            CellClipboard cellClipboard,
            BackupManager backupManager)
        {
            PersistedProject = persistedProject;
            PluginFunctionLoader = pluginFunctionLoader;
            _cellLoader = cellLoader;
            CellTracker = cellTracker;
            UserCollectionLoader = userCollectionLoader;
            CellPopulateManager = cellPopulateManager;
            _cellTriggerManager = cellTriggerManager;
            SheetTracker = sheetTracker;
            CellSelector = cellSelector;
            ApplicationSettings = applicationSettings;
            UndoRedoManager = undoRedoManager;
            _cellClipboard = cellClipboard;
            BackupManager = backupManager;
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
        /// Gets the active sheet view in the application.
        /// </summary>
        public SheetView? ActiveSheetView => _applicationView?.ActiveSheetView;

        /// <summary>
        /// Gets the persisted application settings for the application.
        /// </summary>
        public ApplicationSettings ApplicationSettings { get; private set; }

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

        /// <summary>
        /// Gets the backup manager for the application, which is used to create backups of the project.
        /// </summary>
        public BackupManager BackupManager { get; private set; }

        /// <summary>
        /// Gets the populator for the application, which is used to auto populate cells in the application.
        /// </summary>
        public CellPopulateManager CellPopulateManager { get; private set; }

        /// <summary>
        /// The cell selector for the application, which is used to select cells in the application.
        /// </summary>
        public CellSelector CellSelector { get; private set; }

        /// <summary>
        /// The cell tracker for the application, which is used to store all of the cells in the application.
        /// </summary>
        public CellTracker CellTracker { get; private set; }

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
        /// Gets the persisted project for the application, which is used to save and load the project.
        /// </summary>
        public PersistedProject PersistedProject { get; private set; }

        /// <summary>
        /// Gets the plugin function loader for the application, which loads and stores all plugin functions.
        /// </summary>
        public PluginFunctionLoader PluginFunctionLoader { get; private set; }

        /// <summary>
        /// Gets the sheet tracker for the application, which is used to store all of the sheets in the application.
        /// </summary>
        public SheetTracker SheetTracker { get; private set; }

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
        public UndoRedoManager UndoRedoManager { get; private set; }

        /// <summary>
        /// Gets the user collection loader for the application, which loads and stores all user collections.
        /// </summary>
        public UserCollectionLoader UserCollectionLoader { get; private set; }

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
        /// Connects the application view to the view model, which should always have a 1 to 1 relationship.
        /// </summary>
        /// <param name="applicationView">The view for this view model.</param>
        public void AttachToView(ApplicationView applicationView)
        {
            _applicationView = applicationView;
            _applicationView.DataContext = this;
        }

        /// <summary>
        /// Copies the selected cells to the clipboard.
        /// </summary>
        /// <param name="copyTextOnly">Whether to only copy the text of the cells.</param>
        public void CopySelectedCells(bool copyTextOnly)
        {
            if (SheetViewModel == null) return;
            _cellClipboard.CopyCells(SheetViewModel.CellSelector.SelectedCells, copyTextOnly);
        }

        /// <summary>
        /// Opens the sheet that contains the given cell and pans to the given cell.
        /// </summary>
        /// <param name="cellModel">The cell to show and center.</param>
        public void GoToCell(CellModel cellModel)
        {
            GoToSheet(cellModel.Location.SheetName);
            var cell = SheetViewModel?.CellViewModels.FirstOrDefault(x => x.Model.ID == cellModel.ID);
            if (cell is not null) ActiveSheetView?.PanCanvasTo(cell.X, cell.Y);
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
                SheetViewModel = new SheetViewModel(sheet, CellPopulateManager, _cellTriggerManager, CellTracker, CellSelector, PluginFunctionLoader);
                _sheetModelToViewModelMap.Add(sheet, SheetViewModel);
            }
            _applicationView?.ShowSheetView(SheetViewModel);
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
            UndoRedoManager.StartRecordingUndoState();
            _cellClipboard.PasteIntoCells(SheetViewModel.CellSelector.SelectedCells);
            SheetViewModel.UpdateLayout();
            UndoRedoManager.FinishRecordingUndoState();
        }

        /// <summary>
        /// Shows the given tool window.
        /// </summary>
        /// <param name="viewModel">The view model for the view to display.</param>
        /// <param name="allowDuplicates">Whether or not to actually open the window if one of the same type is already open.</param>
        public void ShowToolWindow(ToolWindowViewModel viewModel, bool allowDuplicates = false)
        {
            _applicationView?.ShowToolWindow(viewModel, allowDuplicates);
        }

        public void DockToolWindow(ToolWindowViewModel viewModel, Dock dock, bool allowDuplicates = false)
        {
            _applicationView?.DockToolWindow(viewModel, dock, allowDuplicates);
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
            var cells = _cellLoader.LoadCells();
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
            _isProjectLoading = false;
            DialogFactory.ShowDialog("Project migrated", "Project has been migrated to the latest version, try clicking 'Load Project' again.");
        }
    }
}
