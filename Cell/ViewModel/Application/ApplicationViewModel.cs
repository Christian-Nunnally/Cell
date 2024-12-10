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
using System.Diagnostics;
using System.IO;
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
        private TitleBarSheetNavigationViewModel? _titleBarSheetNavigationViewModel;
        private TitleBarNotificationButtonViewModel? _titleBarNotificationButtonViewModel;
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
                if (_applicationWindowHeight == value) return;
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
                if (_applicationWindowWidth == value) return;
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
        public CellClipboard? CellClipboard { private get; set; }
        public CellViewModelFlasher? CellViewModelFlasher { get; set; }

        /// <summary>
        /// Gets or sets the cell loader for the application, which is used to load cells into the application.
        /// </summary>
        public CellLoader? CellLoader { private get; set; }

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
            get => _titleBarSheetNavigationViewModel; set
            {
                if (_titleBarSheetNavigationViewModel == value) return;
                _titleBarSheetNavigationViewModel = value;
                NotifyPropertyChanged(nameof(TitleBarSheetNavigationViewModel));
            }
        }

        /// <summary>
        /// Gets or sets the view model for the title bar notifcation button.
        /// </summary>
        public TitleBarNotificationButtonViewModel? TitleBarNotificationButtonViewModel
        {
            get => _titleBarNotificationButtonViewModel; set
            {
                if (_titleBarNotificationButtonViewModel == value) return;
                _titleBarNotificationButtonViewModel = value;
                NotifyPropertyChanged(nameof(TitleBarNotificationButtonViewModel));
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
        /// The logger for the application.
        /// </summary>
        public Logger Logger { get; set; } = Logger.Null;

        /// <summary>
        /// Logger used to notify the user in the title bar.
        /// </summary>
        public Logger NotificationLogger { get; internal set; }

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
            if (CellSelector is null) return;
            CellClipboard?.CopyCells(CellSelector.SelectedCells, copyTextOnly);
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
                SheetViewModel = new SheetViewModel(sheet, CellPopulateManager, CellTriggerManager, CellTracker, CellSelector, UndoRedoManager, FunctionTracker);
                SheetViewModel.InitializeCellViewModelsAsync();
                _sheetModelToViewModelMap.Add(sheet, SheetViewModel);
            }
        }

        /// <summary>
        /// Opens the sheet with the given name in the application.
        /// </summary>
        /// <param name="sheetName">The name of the sheet to show.</param>
        public async Task GoToSheetAsync(string sheetName)
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
                SheetViewModel = new SheetViewModel(sheet, CellPopulateManager, CellTriggerManager, CellTracker, CellSelector, UndoRedoManager, FunctionTracker);
                _sheetModelToViewModelMap.Add(sheet, SheetViewModel);
                await SheetViewModel.InitializeCellViewModelsAsync();
            }
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
            if (CellSelector is null) return;
            UndoRedoManager?.StartRecordingUndoState();
            CellClipboard?.PasteIntoCells(CellSelector.SelectedCells);
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

        internal async Task<LoadResult> LoadAsync(UserCollectionLoader userCollectionLoader)
        {
            if (IsProjectLoaded) return FailLoad("Already loaded", "");
            if (_isProjectLoading) return FailLoad("Already loading", "");
            _isProjectLoading = true;
            if (BackupManager is null) return FailLoad("Backup manager not initialized yet, try loading again.", "");
            ApplicationBackgroundMessage = "Checking for migration";
            if (PersistedProject is null) return FailLoad("Persisted project not initialized yet, try loading again.", "");
            if (PersistedProject.NeedsMigration()) return await MigrateAndContinueLoadingAsync(userCollectionLoader);
            else return await ContinueLoadingAsync(userCollectionLoader);
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
            NotificationLogger.Log($"Backup created at {BackupManager.BackupDirectory.GetFullPath()}");
        }

        private async Task<LoadResult> ContinueLoadingAsync(UserCollectionLoader userCollectionLoader)
        {
            if (PersistedProject is null) return FailLoad("Persisted project not initialized yet, try loading again.", "");
            if (!_hasVersionBeenSaved) { PersistedProject.SaveVersion(); _hasVersionBeenSaved = true; }
            ApplicationBackgroundMessage = "Starting backup";
            var backupTask = BackupAsync();
            ApplicationBackgroundMessage = "Loading collections";
            await userCollectionLoader.LoadCollectionsAsync();
            ApplicationBackgroundMessage = "Loading functions";
            if (FunctionLoader is null) return FailLoad("Function loader not initialized yet, try loading again.", "");
            await FunctionLoader.LoadCellFunctionsAsync();
            ApplicationBackgroundMessage = "Linking collections to thier bases";
            if (UserCollectionTracker is null) return FailLoad("User collection loader not initialized yet, try loading again.", "");
            UserCollectionTracker.LinkUpBaseCollectionsAfterLoad();
            ApplicationBackgroundMessage = "Loading cells";
            if (CellLoader is null) return FailLoad("Cell loader not initialized yet, try loading again.", "");
            if (CellTracker is null) return FailLoad("Cell tracker not initialized yet, try loading again.", "");
            if (FunctionTracker is null) return FailLoad("Function tracker not initialized yet, try loading again.", "");
            CellPopulateManager = new CellPopulateManager(CellTracker, FunctionTracker, UserCollectionTracker, Logger ?? Logger.Null);
            IsProjectLoaded = true;
            await CellLoader.LoadSheetsAsync();
            await CellLoader.FinishLoadingSheetsAsync();
            ApplicationBackgroundMessage = "Creating populate manager";
            _isProjectLoading = false;
            ApplicationBackgroundMessage = "Waiting for backup to finish";
            await backupTask;
            ApplicationBackgroundMessage = "Open a sheet to view cells";
            return new LoadResult { WasSuccess = true };
        }

        private async Task MigrateProjectAsync()
        {
            if (BackupManager is null) throw new CellError("Backup manager not initialized, unable to migrate.");
            ApplicationBackgroundMessage = "Creating pre-migration backup";
            await BackupManager.CreateBackupAsync("PreMigration");
            if (PersistedProject is null) throw new CellError("Persisted project not initialized yet, unable to migrate.");
            ApplicationBackgroundMessage = $"Starting migration to {PersistedProject.Version}";
            PersistedProject.Migrate();
        }

        private async Task<LoadResult> MigrateAndContinueLoadingAsync(UserCollectionLoader userCollectionLoader)
        {
            if (PersistedProject is null) return FailLoad("Persisted project not initialized yet, try loading again.", "");
            if (!await PersistedProject.CanMigrateAsync())
            {
                if (!HelpUserCheckoutCommit(PersistedProject.Version)) return FailLoad($"Migration needed but unable to complete until ${PersistedProject.Version} is checked out.", PersistedProject.Version);
                else return FailLoad($"Checkout attempted, close the application, rebuild, and relaunch it to complete the migration. You can then go back to the latest commit.", PersistedProject.Version);
            }
            await MigrateProjectAsync(); 
            return await ContinueLoadingAsync(userCollectionLoader);
        }

        private bool HelpUserCheckoutCommit(string commitId)
        {
            string batchFilePath = Path.Combine(Path.GetTempPath(), "tempScript.bat");
            var script =
                """
                @echo off
                cd ..
                cd ..
                cd ..
                cd ..
                echo <PROMPT>
                set /p userinput=(y/n): 
                if "%userinput%"=="y" (
                    <COMMAND>
                    pause
                    exit 0
                ) else if "%userinput%"=="n" (
                    echo Exiting...
                ) else (
                    echo Invalid input. Exiting...
                )
                pause
                exit 1
                """;
            script = script.Replace("<PROMPT>", $"Do you want to automatically check out commit {commitId}");
            script = script.Replace("<COMMAND>", $"git checkout {commitId}");
            File.WriteAllText(batchFilePath, script);
        
            Process cmdProcess = new();
            cmdProcess.StartInfo.FileName = "cmd.exe";
            cmdProcess.StartInfo.Arguments = "/C \"" + batchFilePath + "\"";
            cmdProcess.StartInfo.UseShellExecute = false;
            cmdProcess.StartInfo.CreateNoWindow = false;
            cmdProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;

            cmdProcess.Start();
            cmdProcess.WaitForExit();
            File.Delete(batchFilePath);

            if (cmdProcess.ExitCode == 0)
            {
                ApplicationBackgroundMessage = $"Commit {commitId} checkout attempted, rebuild and relaunch to migrate";
            }
            else
            {
                ApplicationBackgroundMessage = $"Must be on version {commitId} to migrate this project.";
            }
            return cmdProcess.ExitCode == 0;
        }


        private void RequestClose(ToolWindowViewModel viewModel)
        {
            var isAllowingClose = viewModel.HandleCloseRequested();
            if (isAllowingClose)
            {
                RemoveToolWindow(viewModel);
            }
        }

        private LoadResult FailLoad(string details, string migrationCommit)
        {
            return new LoadResult 
            { 
                WasSuccess = false,
                Details = details,
                MigrationCommit = migrationCommit
            };
        }

        internal void GoToNextSheet()
        {
            if (SheetTracker is null) return;
            if (SheetViewModel is null) return;
            var currentSheet = SheetTracker.Sheets.FirstOrDefault(x => x.Name == SheetViewModel.SheetName);
            if (currentSheet is null) return;
            var startingIndex = SheetTracker.OrderedSheets.IndexOf(currentSheet);
            for (int i = startingIndex + 1; i < SheetTracker.OrderedSheets.Count; i++)
            {
                var sheet = SheetTracker.OrderedSheets[i];
                if (sheet is not null && sheet.IsVisibleInTopBar)
                {
                    GoToSheet(sheet.Name);
                    return;
                }
            }
            for (int i = 0; i < SheetTracker.OrderedSheets.Count; i++)
            {
                var sheet = SheetTracker.OrderedSheets[i];
                if (sheet is not null && sheet.IsVisibleInTopBar)
                {
                    GoToSheet(sheet.Name);
                    return;
                }
            }
        }

        internal void GoToPreviousSheet()
        {
            if (SheetTracker is null) return;
            if (SheetViewModel is null) return;
            var currentSheet = SheetTracker.Sheets.FirstOrDefault(x => x.Name == SheetViewModel.SheetName);
            if (currentSheet is null) return;
            var startingIndex = SheetTracker.OrderedSheets.IndexOf(currentSheet);
            for (int i = startingIndex - 1; i >= 0; i--)
            {
                var sheet = SheetTracker.OrderedSheets[i];
                if (sheet is not null && sheet.IsVisibleInTopBar)
                {
                    GoToSheet(sheet.Name);
                    return;
                }
            }
            for (int i = SheetTracker.OrderedSheets.Count - 1; i >= 0; i--)
            {
                var sheet = SheetTracker.OrderedSheets[i];
                if (sheet is not null && sheet.IsVisibleInTopBar)
                {
                    GoToSheet(sheet.Name);
                    return;
                }
            }
        }
    }

    public struct LoadResult
    {
        public bool WasSuccess;
        public string MigrationCommit;
        public string Details;
    }
}
