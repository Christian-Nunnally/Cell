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
        protected override async void OnStartup(StartupEventArgs e)
        {
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            var appDataPath = Environment.SpecialFolder.ApplicationData;
            var appDataRoot = Environment.GetFolderPath(appDataPath);
            var appPersistanceRoot = Path.Combine(appDataRoot, "LGF");
            var savePath = Path.Combine(appPersistanceRoot, "Cell");
            var backupPath = Path.Combine(appPersistanceRoot, "CellBackups");
            var fileIo = new FileIO();
            var projectDirectory = new PersistedDirectory(savePath, fileIo);
            var persistedProject = new PersistedProject(projectDirectory);
            persistedProject.RegisterMigrator("1", "2", new Migration());
            var backupDirectory = new PersistedDirectory(backupPath, fileIo);

            var pluginFunctionLoader = new PluginFunctionLoader(persistedProject.FunctionsDirectory);
            var cellLoader = new CellLoader(persistedProject.SheetsDirectory);
            var cellTracker = new CellTracker(cellLoader);
            var userCollectionLoader = new UserCollectionLoader(persistedProject.CollectionsDirectory, pluginFunctionLoader, cellTracker);
            var dialogFactory = new DialogFactory();
            var cellTriggerManager = new CellTriggerManager(cellTracker, pluginFunctionLoader, userCollectionLoader, dialogFactory);
            var cellPopulateManager = new CellPopulateManager(cellTracker, pluginFunctionLoader, userCollectionLoader);
            var sheetTracker = new SheetTracker(cellTracker);
            var applicationSettings = ApplicationSettings.CreateInstance(projectDirectory);
            var undoRedoManager = new UndoRedoManager(cellTracker);
            var textClipboard = new TextClipboard();
            var cellSelector = new CellSelector(cellTracker);

            var applicationViewModel = new ApplicationViewModel(
                dialogFactory,
                persistedProject,
                pluginFunctionLoader,
                cellLoader,
                cellTracker,
                userCollectionLoader,
                cellPopulateManager,
                cellTriggerManager,
                sheetTracker,
                cellSelector,
                applicationSettings,
                undoRedoManager);

            var applicationView = new ApplicationView(applicationViewModel);
            stopWatch.Stop();
            var x = stopWatch.ElapsedMilliseconds;
            Console.WriteLine(x);
            applicationView.Show();

            applicationViewModel.InitializeView();
            applicationViewModel.BackupManager = new BackupManager(projectDirectory, backupDirectory);
            applicationViewModel.CellClipboard = new CellClipboard(undoRedoManager, cellTracker, textClipboard);
            applicationViewModel.InitializeProject(persistedProject);

            var cellContentEditWindowViewModel = new CellContentEditWindowViewModel(applicationViewModel.CellSelector.SelectedCells);
            applicationViewModel.DockToolWindow(cellContentEditWindowViewModel, Dock.Top);
        }
    }
}
