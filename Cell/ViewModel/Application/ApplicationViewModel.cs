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
        private readonly CellLoader _cellLoader;
        private readonly CellTriggerManager _cellTriggerManager;
        private readonly Dictionary<SheetModel, SheetViewModel> _sheetModelToViewModelMap = [];
        private static ApplicationViewModel? _instance;
        private double _applicationWindowHeight = 1300;
        private double _applicationWindowWidth = 1200;
        private Task _backupTask = Task.CompletedTask;
        private bool _isProjectLoaded;
        private bool _isProjectLoading;
        private SheetViewModel? _sheetViewModel;
        /// <summary>
        /// Creates a new instance of <see cref="ApplicationViewModel"/>.
        /// </summary>
        /// <param name="dialogFactory">A factory for creating and showing dialogs.</param>
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
        public ApplicationViewModel(
            DialogFactoryBase dialogFactory,
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
            UndoRedoManager undoRedoManager)
        {
            DialogFactory = dialogFactory;
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
        public ApplicationSettings ApplicationSettings { get; private set; }

        public TitleBarSheetNavigationViewModel TitleBarSheetNavigationViewModel { get; private set; }

        /// <summary>
        /// A factory for showing dialogs.
        /// </summary>
        public DialogFactoryBase DialogFactory { get; set; }

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

        public event Action<ToolWindowViewModel> MoveToolWindowToTop;

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
        /// Gets or sets the persisted project for the application, which is used to save and load the project.
        /// </summary>
        public PersistedProject? PersistedProject { get; set; }

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
        /// Gets the observable collection of open tool windows in the application.
        /// </summary>
        public ObservableCollection<ToolWindowViewModel> OpenToolWindowViewModels { get; } = [];

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
        /// Copies the selected cells to the clipboard.
        /// </summary>
        /// <param name="copyTextOnly">Whether to only copy the text of the cells.</param>
        public void CopySelectedCells(bool copyTextOnly)
        {
            if (CellClipboard is null)
            {
                
            }
            if (SheetViewModel == null) return;
            CellClipboard.CopyCells(SheetViewModel.CellSelector.SelectedCells, copyTextOnly);
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
                SheetViewModel = new SheetViewModel(sheet, CellPopulateManager, _cellTriggerManager, CellTracker, CellSelector, PluginFunctionLoader);
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
            UndoRedoManager.StartRecordingUndoState();
            CellClipboard?.PasteIntoCells(SheetViewModel.CellSelector.SelectedCells);
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
            await Task.Run(() =>
            {
                PersistedProject.IsReadOnly = true;
                BackupManager!.CreateBackup();
                PersistedProject.IsReadOnly = false;
            });
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
            DialogFactory.Show("Project migrated", "Project has been migrated to the latest version, try clicking 'Load Project' again.");
        }

        internal void RemoveToolWindow(ToolWindowViewModel toolWindowViewModel)
        {
            OpenToolWindowViewModels.Remove(toolWindowViewModel);
            toolWindowViewModel.HandleBeingClosed();
        }

        internal void InitializeView()
        {
            TitleBarSheetNavigationViewModel = new TitleBarSheetNavigationViewModel(SheetTracker);
            NotifyPropertyChanged(nameof(TitleBarSheetNavigationViewModel));
        }

        internal void InitializeProject(PersistedProject persistedProject)
        {
        }
    }
}
