using Cell.Core.Data;
using Cell.Core.Execution;
using Cell.Core.Persistence;
using Cell.View.Application;
using Cell.ViewModel.Application;
using System.Windows;
using System.IO;
using Cell.ViewModel.ToolWindow;
using System.Windows.Controls;
using Cell.Core.Persistence.Loader;
using Cell.Core.Data.Tracker;
using Cell.Core.Common;

namespace Cell
{
    public partial class App : Application
    {
        private SheetTracker sheetTracker = new (new CellTracker());
        private readonly ApplicationViewModel applicationViewModel = new();

        /// <summary>
        /// The entry point for the entire application.
        /// </summary>
        /// <param name="e">Start up event arguments.</param>
        protected override async void OnStartup(StartupEventArgs e)
        {
            var applicationView = new ApplicationView(applicationViewModel);
            applicationView.Show();

            applicationViewModel.Logger = new Logger();
            var dialogFactory = new DialogFactory();
            applicationViewModel.DialogFactory = dialogFactory;
            var notificationLogger = new Logger();
            applicationViewModel.NotificationLogger = notificationLogger;
            var appDataPath = Environment.SpecialFolder.ApplicationData;
            var appDataRoot = Environment.GetFolderPath(appDataPath);
            var appPersistanceRoot = Path.Combine(appDataRoot, "LGF");
            var savePath = Path.Combine(appPersistanceRoot, "Cell");
            var fileIo = new FileIO();
            var projectDirectory = new PersistedDirectory(savePath, fileIo);
            var persistedProject = new PersistedProject(projectDirectory);

            persistedProject.RegisterMigrator("2", "0.3.0", new FasterLoadMigrator());

            applicationViewModel.WindowDockPanelViewModel = new WindowDockPanelViewModel();
            applicationViewModel.PersistedProject = persistedProject;
            var functionTracker = new FunctionTracker(applicationViewModel.Logger);
            applicationViewModel.FunctionTracker = functionTracker;
            var pluginFunctionLoader = new FunctionLoader(persistedProject.FunctionsDirectory, functionTracker, applicationViewModel.Logger);
            applicationViewModel.FunctionLoader = pluginFunctionLoader;
            var cellTracker = new CellTracker();
            applicationViewModel.CellTracker = cellTracker;
            var userCollectionTracker = new UserCollectionTracker(functionTracker, cellTracker);
            applicationViewModel.UserCollectionTracker = userCollectionTracker;
            var cellTriggerManager = new CellTriggerManager(cellTracker, functionTracker, userCollectionTracker, dialogFactory, applicationViewModel.Logger);
            applicationViewModel.CellTriggerManager = cellTriggerManager;
            sheetTracker = new SheetTracker(cellTracker);
            applicationViewModel.SheetTracker = sheetTracker;
            var cellLoader = new CellLoader(persistedProject.SheetsDirectory, cellTracker);
            applicationViewModel.CellLoader = cellLoader;
            var titleBarSheetNavigationViewModel = new TitleBarSheetNavigationViewModel(sheetTracker);
            applicationViewModel.TitleBarSheetNavigationViewModel = titleBarSheetNavigationViewModel;
            var titleBarNotificationButtonViewModel = new TitleBarNotificationButtonViewModel(notificationLogger);
            applicationViewModel.TitleBarNotificationButtonViewModel = titleBarNotificationButtonViewModel;
            var applicationSettings = ApplicationSettings.CreateInstance(projectDirectory);
            applicationViewModel.ApplicationSettings = applicationSettings;
            var backupPath = Path.Combine(appPersistanceRoot, "CellBackups");
            var backupDirectory = new PersistedDirectory(backupPath, fileIo);
            applicationViewModel.BackupManager = new BackupManager(projectDirectory, backupDirectory);
            var undoRedoManager = new UndoRedoManager(cellTracker, functionTracker);
            applicationViewModel.UndoRedoManager = undoRedoManager;
            var textClipboard = new TextClipboard();
            applicationViewModel.CellClipboard = new CellClipboard(undoRedoManager, cellTracker, textClipboard);
            applicationViewModel.CellViewModelFlasher = new CellViewModelFlasher();
            var cellSelector = new CellSelector(cellTracker);
            applicationViewModel.CellSelector = cellSelector;
            cellLoader.SheetsLoaded += OnAllSheetsLoaded;
            var loadResult = await applicationViewModel.LoadAsync(new UserCollectionLoader(persistedProject.CollectionsDirectory, userCollectionTracker, functionTracker, cellTracker));
            
            if (!loadResult.WasSuccess)
            {
                if (!string.IsNullOrEmpty(loadResult.MigrationCommit))
                {
                    dialogFactory.Show("Migration required to load", loadResult.Details);
                    return;
                }
            }

            OpenCellContentEditWindowInDockedMode(applicationViewModel, functionTracker, cellSelector);
            OnlyAllowSelectionWhenEditWindowIsOpen(applicationViewModel, cellSelector);
        }

        private void OnAllSheetsLoaded()
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                var sheet = sheetTracker.OrderedSheets.FirstOrDefault();
                if (sheet is not null) applicationViewModel.GoToSheetAsync(sheet.Name);
            });
        }

        private static void OpenCellContentEditWindowInDockedMode(ApplicationViewModel applicationViewModel, FunctionTracker functionTracker, CellSelector cellSelector)
        {
            var cellContentEditWindowViewModel = new CellContentEditWindowViewModel(cellSelector.SelectedCells, functionTracker, applicationViewModel.Logger);
            applicationViewModel.DockToolWindow(cellContentEditWindowViewModel, WindowDockType.DockedTop);
        }

        private void OnlyAllowSelectionWhenEditWindowIsOpen(ApplicationViewModel applicationViewModel, CellSelector cellSelector)
        {
            // TODO: Fix this to look at all open window.
            applicationViewModel.WindowDockPanelViewModel.VisibleContentAreasThatAreFloating.CollectionChanged += (o, e) =>
            {
                //cellSelector.IsSelectingEnabled = applicationViewModel.OpenToolWindowViewModels.Any();
                if (applicationViewModel.SheetViewModel is not null) applicationViewModel.SheetViewModel.IsCellHighlightOnMouseOverEnabled = applicationViewModel.WindowDockPanelViewModel.VisibleContentAreasThatAreFloating.Any();
            };
        }
    }
}
