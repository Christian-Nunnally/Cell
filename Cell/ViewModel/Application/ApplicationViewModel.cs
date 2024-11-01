using Cell.Core.Common;
using Cell.Core.Data;
using Cell.Core.Data.Tracker;
using Cell.Core.Execution;
using Cell.Core.Persistence;
using Cell.Core.Persistence.Loader;
using Cell.Model;
using Cell.ViewModel.Cells;
using Cell.ViewModel.ToolWindow;
using System.Collections.ObjectModel;
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
        private readonly Dictionary<SheetModel, SheetViewModel> _sheetModelToViewModelMap = [];
        private static ApplicationViewModel? _instance;
        private string _applicationBackgroundMessage = "No cells loaded";
        private double _applicationWindowHeight = 1300;
        private double _applicationWindowWidth = 1200;
        private bool _hasVersionBeenSaved = false;
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
        /// The handler that is called when this view model wants to move a to the top of the z order.
        /// </summary>
        public event Action<ToolWindowViewModel>? MoveToolWindowToTop;

        /// <summary>
        /// Gets the current instance of the application view model.
        /// </summary>
        public static ApplicationViewModel Instance { get => _instance ?? throw new NullReferenceException("Application instance not set"); set => _instance = value ?? throw new NullReferenceException("Static instances not allowed to be null"); }

        /// <summary>
        /// Gets the current instance of the application view model, or null if it is not set.
        /// </summary>
        public static ApplicationViewModel? SafeInstance => _instance;

        /// <summary>
        /// Gets or sets the message to display in the background of the application.
        /// </summary>
        public string ApplicationBackgroundMessage
        {
            get => _applicationBackgroundMessage; set
            {
                if (_applicationBackgroundMessage == value) return;
                _applicationBackgroundMessage = value;
                NotifyPropertyChanged(nameof(ApplicationBackgroundMessage));
            }
        }

        /// <summary>
        /// Gets the persisted application settings for the application.
        /// </summary>
        public ApplicationSettings? ApplicationSettings { get; set; }

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
        public BackupManager? BackupManager { get; set; }

        /// <summary>
        /// Enables copy and paste though this clipboard.
        /// </summary>
        public CellClipboard? CellClipboard { get; set; }

        /// <summary>
        /// Gets or sets the cell loader for the application, which is used to load cells into the application.
        /// </summary>
        public CellLoader? CellLoader { get; set; }

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
        /// Gets or sets the cell trigger manager for the application, which is used to manage all cell triggers.
        /// </summary>
        public CellTriggerManager? CellTriggerManager { get; set; }

        /// <summary>
        /// A factory for showing dialogs.
        /// </summary>
        public DialogFactoryBase? DialogFactory { get; set; }

        /// <summary>
        /// Gets the plugin function loader for the application, which loads and stores all plugin functions.
        /// </summary>
        public FunctionLoader? FunctionLoader { get; set; }

        /// <summary>
        /// Gets or sets the main functions tracker used to manage all functions loaded into the application.
        /// </summary>
        public FunctionTracker? FunctionTracker { get; set; }

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
        /// Gets the observable collection of open tool windows in the application.
        /// </summary>
        public ObservableCollection<ToolWindowViewModel> OpenToolWindowViewModels { get; } = [];

        /// <summary>
        /// Gets or sets the persisted project for the application, which is used to save and load the project.
        /// </summary>
        public PersistedProject? PersistedProject { get; set; }

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
        /// Gets the application wide undo redo manager.
        /// </summary>
        public UndoRedoManager? UndoRedoManager { get; set; }

        /// <summary>
        /// Gets the user collection tracker for the application, which tracks all user collections.
        /// </summary>
        public UserCollectionTracker? UserCollectionTracker { get; set; }

        /// <summary>
        /// Gets the application wide undo redo manager.
        /// </summary>
        /// <returns>The global undo/redo manager.</returns>
        public static UndoRedoManager? GetUndoRedoManager()
        {
            if (_instance is null) return null;
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
            if (SheetViewModel is null) return;
            CellClipboard?.CopyCells(SheetViewModel.CellSelector.SelectedCells, copyTextOnly);
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
            if (CellPopulateManager is null) throw new CellError("Unable to go to sheet without a populate manager.");
            if (CellTracker is null) throw new CellError("Unable to go to sheet without a cell tracker.");
            if (CellTriggerManager is null) throw new CellError("Unable to go to sheet without a trigger manager.");
            if (CellSelector is null) throw new CellError("Unable to go to sheet without a cell selector.");
            if (FunctionTracker is null) throw new CellError("Unable to go to sheet without a function tracker.");
            if (!SheetModel.IsValidSheetName(sheetName)) return;
            if (SheetViewModel?.SheetName == sheetName) return;
            var sheet = SheetTracker?.Sheets.FirstOrDefault(x => x.Name == sheetName);
            if (sheet is null) return;
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
            if (ApplicationSettings != null) ApplicationSettings.LastLoadedSheet = sheetName;
        }

        /// <summary>
        /// Loads the entire project and then returns.
        /// </summary>
        /// <param name="userCollectionLoader">The loader to load collections from.</param>
        /// <returns>The finish loading progress, which might be mark incomplete if the load did not finish.</returns>
        public LoadingProgressResult Load(UserCollectionLoader userCollectionLoader)
        {
            try
            {
                LoadAsync(userCollectionLoader).Wait();

            }
            catch (AggregateException e)
            {
                throw e.InnerException!;
            }
            return new LoadingProgressResult(true, "Load Complete");
        }

        /// <summary>
        /// Moves the given tool window to the top of the z order.
        /// </summary>
        /// <param name="toolWindow">The tool window.</param>
        public void MoveWindowToTop(ToolWindowViewModel toolWindow)
        {
            MoveToolWindowToTop?.Invoke(toolWindow);
        }

        /// <summary>
        /// Pastes the copied cells into the selected cells.
        /// </summary>
        public void PasteCopiedCells()
        {
            if (SheetViewModel is null) return;
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

        internal async Task LoadAsync(UserCollectionLoader userCollectionLoader)
        {
            if (IsProjectLoaded) throw new CellError("Already loaded");
            if (_isProjectLoading) throw new CellError("Already loading");
            _isProjectLoading = true;
            if (BackupManager is null) throw new CellError("Backup manager not initialized yet, try loading again.");
            ApplicationBackgroundMessage = "Checking for migration";
            if (PersistedProject is null) throw new CellError("Persisted project not initialized yet, try loading again.");
            if (PersistedProject.NeedsMigration()) PromptUserToMigrateAndContinueLoading(userCollectionLoader);
            else await ContinueLoadingAsync(userCollectionLoader);
        }

        internal void RemoveToolWindow(ToolWindowViewModel toolWindowViewModel)
        {
            OpenToolWindowViewModels.Remove(toolWindowViewModel);
            toolWindowViewModel.HandleBeingClosed();
        }

        private async Task BackupAsync()
        {
            if (PersistedProject is null) return;
            PersistedProject.IsReadOnly = true;
            await BackupManager!.CreateBackupAsync();
            PersistedProject.IsReadOnly = false;
        }

        private async Task ContinueLoadingAsync(UserCollectionLoader userCollectionLoader)
        {
            if (PersistedProject is null) throw new CellError("Persisted project not initialized yet, try loading again.");
            if (!_hasVersionBeenSaved) { PersistedProject.SaveVersion(); _hasVersionBeenSaved = true; }
            ApplicationBackgroundMessage = "Starting backup";
            var backupTask = BackupAsync();
            ApplicationBackgroundMessage = "Loading collections";
            await userCollectionLoader.LoadCollectionsAsync();
            ApplicationBackgroundMessage = "Loading functions";
            if (FunctionLoader is null) throw new CellError("Function loader not initialized yet, try loading again.");
            FunctionLoader.LoadCellFunctions();
            ApplicationBackgroundMessage = "Linking collections to thier bases";
            if (UserCollectionTracker is null) throw new CellError("User collection loader not initialized yet, try loading again.");
            UserCollectionTracker.LinkUpBaseCollectionsAfterLoad();
            ApplicationBackgroundMessage = "Loading cells";
            if (CellLoader is null) throw new CellError("Cell loader not initialized yet, try loading again.");
            CellLoader.LoadCells();
            await backupTask;
            if (CellTracker is null) throw new CellError("Cell tracker not initialized yet, try loading again.");
            if (FunctionTracker is null) throw new CellError("Function tracker not initialized yet, try loading again.");
            ApplicationBackgroundMessage = "Creating populate manager";
            CellPopulateManager = new CellPopulateManager(CellTracker, FunctionTracker, UserCollectionTracker);
            IsProjectLoaded = true;
            _isProjectLoading = false;
            ApplicationBackgroundMessage = "Open a sheet to view cells";
        }

        private async Task MigrateProjectAsync()
        {
            if (BackupManager is null) throw new CellError("Backup manager not initialized, unable to migrate.");
            await BackupManager.CreateBackupAsync("PreMigration");
            if (PersistedProject is null) throw new CellError("Persisted project not initialized yet, unable to migrate.");
            PersistedProject.Migrate();
            _isProjectLoading = false;
            DialogFactory?.Show("Project migrated", "Project has been migrated to the latest version. Reload the application now.");
        }

        private void PromptUserToMigrateAndContinueLoading(UserCollectionLoader userCollectionLoader)
        {
            if (PersistedProject is null) throw new CellError("Persisted project not initialized yet, try loading again.");
            if (!PersistedProject.CanMigrate()) throw new CellError(NoMigratorForVersionError);
            if (DialogFactory is null) throw new CellError("Dialog factory not initialized yet, try loading again.");
            DialogFactory.ShowYesNo(
                "Version Mismatch",
                $"The version of your project is outdated. would you like to migrate it?",
                () => { MigrateProjectAsync().Wait(); ContinueLoadingAsync(userCollectionLoader).Wait(); },
                () => { });
        }

        private void RequestClose(ToolWindowViewModel viewModel)
        {
            var isAllowingClose = viewModel.HandleCloseRequested();
            if (isAllowingClose)
            {
                RemoveToolWindow(viewModel);
            }
        }
    }
}
