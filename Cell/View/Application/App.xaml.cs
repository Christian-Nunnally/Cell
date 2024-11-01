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
using Cell.Core.Persistence.Loader;
using Cell.Core.Data.Tracker;

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
            applicationViewModel.FunctionTracker = functionTracker;
            var pluginFunctionLoader = new FunctionLoader(persistedProject.FunctionsDirectory, functionTracker);
            applicationViewModel.FunctionLoader = pluginFunctionLoader;
            var cellTracker = new CellTracker();
            applicationViewModel.CellTracker = cellTracker;
            var userCollectionTracker = new UserCollectionTracker(functionTracker, cellTracker);
            applicationViewModel.UserCollectionTracker = userCollectionTracker;
            var cellTriggerManager = new CellTriggerManager(cellTracker, functionTracker, userCollectionTracker, dialogFactory);
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
            await Task.Run(() => applicationViewModel.LoadAsync(new UserCollectionLoader(persistedProject.CollectionsDirectory, userCollectionTracker, functionTracker, cellTracker)));
            OpenCellContentEditWindowInDockedMode(applicationViewModel, functionTracker, cellSelector);
            OnlyAllowSelectionWhenEditWindowIsOpen(applicationViewModel, cellSelector);
            OpenInitialSheet(applicationViewModel, sheetTracker);
        }

        private static void OpenCellContentEditWindowInDockedMode(ApplicationViewModel applicationViewModel, FunctionTracker functionTracker, CellSelector cellSelector)
        {
            var cellContentEditWindowViewModel = new CellContentEditWindowViewModel(cellSelector.SelectedCells, functionTracker);
            applicationViewModel.DockToolWindow(cellContentEditWindowViewModel, Dock.Top);
        }

        private static void OpenInitialSheet(ApplicationViewModel applicationViewModel, SheetTracker sheetTracker)
        {
            var firstSheet = sheetTracker.OrderedSheets.FirstOrDefault();
            applicationViewModel.GoToSheet(firstSheet?.Name ?? string.Empty);
        }

        private void OnlyAllowSelectionWhenEditWindowIsOpen(ApplicationViewModel applicationViewModel, CellSelector cellSelector)
        {
            applicationViewModel.OpenToolWindowViewModels.CollectionChanged += (o, e) =>
            {
                cellSelector.IsSelectingEnabled = applicationViewModel.OpenToolWindowViewModels.Any();
                if (applicationViewModel.SheetViewModel is not null) applicationViewModel.SheetViewModel.IsCellHighlightOnMouseOverEnabled = applicationViewModel.OpenToolWindowViewModels.Any();
            };
        }
    }
}
