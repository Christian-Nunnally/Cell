using Cell.Core.Data;
using Cell.Core.Execution;
using Cell.Core.Persistence.Migration;
using Cell.Core.Persistence;
using Cell.View.Application;
using Cell.ViewModel.Application;
using System.Windows;
using System.IO;
using Cell.ViewModel.ToolWindow;
using System.Windows.Controls;

namespace Cell
{
    public partial class App : Application
    {
        /// <summary>
        /// The entry point for the entire application.
        /// </summary>
        /// <param name="e">Start up event arguments.</param>
        protected override async void OnStartup(StartupEventArgs e)
        {
            var applicationViewModel = new ApplicationViewModel();
            var applicationView = new ApplicationView(applicationViewModel);
            applicationView.Show();

            var dialogFactory = new DialogFactory();
            applicationViewModel.DialogFactory = dialogFactory;
            var appDataPath = Environment.SpecialFolder.ApplicationData;
            var appDataRoot = Environment.GetFolderPath(appDataPath);
            var appPersistanceRoot = Path.Combine(appDataRoot, "LGF");
            var savePath = Path.Combine(appPersistanceRoot, "Cell");
            var fileIo = new FileIO();
            var projectDirectory = new PersistedDirectory(savePath, fileIo);
            var persistedProject = new PersistedProject(projectDirectory);
            applicationViewModel.PersistedProject = persistedProject;
            var functionTracker = new FunctionTracker();
            var pluginFunctionLoader = new FunctionLoader(persistedProject.FunctionsDirectory, functionTracker);
            applicationViewModel.FunctionLoader = pluginFunctionLoader;
            var cellTracker = new CellTracker();
            applicationViewModel.CellTracker = cellTracker;
            var userCollectionLoader = new UserCollectionLoader(persistedProject.CollectionsDirectory, functionTracker, cellTracker);
            applicationViewModel.UserCollectionLoader = userCollectionLoader;
            var cellTriggerManager = new CellTriggerManager(cellTracker, functionTracker, userCollectionLoader, dialogFactory);
            applicationViewModel.CellTriggerManager = cellTriggerManager;
            var sheetTracker = new SheetTracker(cellTracker);
            applicationViewModel.SheetTracker = sheetTracker;
            var cellLoader = new CellLoader(persistedProject.SheetsDirectory, cellTracker);
            applicationViewModel.CellLoader = cellLoader;
            var titleBarSheetNavigationViewModel = new TitleBarSheetNavigationViewModel(sheetTracker);
            applicationViewModel.TitleBarSheetNavigationViewModel = titleBarSheetNavigationViewModel;
            var applicationSettings = ApplicationSettings.CreateInstance(projectDirectory);
            applicationViewModel.ApplicationSettings = applicationSettings;
            var backupPath = Path.Combine(appPersistanceRoot, "CellBackups");
            var backupDirectory = new PersistedDirectory(backupPath, fileIo);
            applicationViewModel.BackupManager = new BackupManager(projectDirectory, backupDirectory);
            var undoRedoManager = new UndoRedoManager(cellTracker);
            applicationViewModel.UndoRedoManager = undoRedoManager;
            var textClipboard = new TextClipboard();
            applicationViewModel.CellClipboard = new CellClipboard(undoRedoManager, cellTracker, textClipboard);
            var cellSelector = new CellSelector(cellTracker);
            applicationViewModel.CellSelector = cellSelector;

            persistedProject.RegisterMigrator("1", "2", new Migration());

            var cellContentEditWindowViewModel = new CellContentEditWindowViewModel(applicationViewModel.CellSelector.SelectedCells, functionTracker);
            applicationViewModel.DockToolWindow(cellContentEditWindowViewModel, Dock.Top);

            applicationViewModel.EnsureInitialBackupIsStarted();
            if (!persistedProject.NeedsMigration())
            {
                userCollectionLoader.EnsureCollectionLoadHasStarted();
            }
        }
    }
}
