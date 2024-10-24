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
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

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
            var sheetTracker = new SheetTracker(projectDirectory, cellLoader, cellTracker, pluginFunctionLoader, userCollectionLoader);
            var applicationSettings = ApplicationSettings.CreateInstance(projectDirectory);
            var undoRedoManager = new UndoRedoManager(cellTracker);
            var textClipboard = new TextClipboard();
            var cellClipboard = new CellClipboard(undoRedoManager, cellTracker, textClipboard);
            var backupManager = new BackupManager(projectDirectory, backupDirectory);
            var cellSelector = new CellSelector(cellTracker);

            var viewModel = new ApplicationViewModel(
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
                undoRedoManager,
                cellClipboard,
                backupManager);
            ApplicationViewModel.Instance = viewModel;

            var applicationView = new ApplicationView(viewModel);
            stopWatch.Stop();
            var x = stopWatch.ElapsedMilliseconds;
            applicationView.Show();

            var cellContentEditWindowViewModel = new CellContentEditWindowViewModel(viewModel.CellSelector.SelectedCells);
            viewModel.DockToolWindow(cellContentEditWindowViewModel, Dock.Top);
        }
    }
}
