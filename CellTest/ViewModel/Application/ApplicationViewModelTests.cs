﻿using Cell.Core.Common;
using Cell.Core.Data;
using Cell.Core.Data.Tracker;
using Cell.Core.Execution;
using Cell.Core.Persistence;
using Cell.Core.Persistence.Loader;
using Cell.ViewModel.Application;
using CellTest.TestUtilities;

namespace CellTest.ViewModel.Application
{
    public class ApplicationViewModelTests
    {
        private readonly TestDialogFactory _testDialogFactory;
        private readonly DictionaryFileIO _testFileIO;
        private readonly PersistedDirectory _persistedDirectory;
        private readonly PersistedDirectory _backupDirectory;
        private readonly UserCollectionTracker _userCollectionTracker;
        private readonly UserCollectionLoader _userCollectionLoader;
        private readonly CellPopulateManager _cellPopulateManager;
        private readonly CellLoader _cellLoader;
        private readonly CellTriggerManager _cellTriggerManager;
        private readonly FunctionLoader _pluginFunctionLoader;
        private readonly CellTracker _cellTracker;
        private readonly SheetTracker _sheetTracker;
        private readonly ApplicationSettings _applicationSettings;
        private readonly UndoRedoManager _undoRedoManager;
        private readonly ITextClipboard _textClipboard;
        private readonly CellClipboard _cellClipboard;
        private readonly BackupManager _backupManager;
        private readonly CellSelector _cellSelector;
        private readonly PersistedProject _persistedProject;
        private readonly FunctionTracker _functionTracker;
        private readonly ApplicationViewModel _testing;

        public ApplicationViewModelTests()
        {
            _testDialogFactory = new TestDialogFactory();
            _testFileIO = new DictionaryFileIO();
            _persistedDirectory = new PersistedDirectory("", _testFileIO);
            _backupDirectory = new PersistedDirectory("", _testFileIO);
            _persistedProject = new PersistedProject(_persistedDirectory);
            _functionTracker = new FunctionTracker();
            _pluginFunctionLoader = new FunctionLoader(_persistedProject.FunctionsDirectory, _functionTracker);
            _cellTracker = new CellTracker();
            _cellLoader = new CellLoader(_persistedProject.SheetsDirectory, _cellTracker);
            _userCollectionTracker = new UserCollectionTracker(_functionTracker, _cellTracker);
            _userCollectionLoader = new UserCollectionLoader(_persistedProject.CollectionsDirectory, _userCollectionTracker, _functionTracker, _cellTracker);
            _cellTriggerManager = new CellTriggerManager(_cellTracker, _functionTracker, _userCollectionTracker, _testDialogFactory);
            _cellPopulateManager = new CellPopulateManager(_cellTracker, _functionTracker, _userCollectionTracker);
            _sheetTracker = new SheetTracker(_cellTracker);
            _backupManager = new BackupManager(_persistedDirectory, _backupDirectory);
            _cellSelector = new CellSelector(_cellTracker);
            _applicationSettings = new ApplicationSettings();
            _undoRedoManager = new UndoRedoManager(_cellTracker);
            _textClipboard = new TestTextClipboard();
            _cellClipboard = new CellClipboard(_undoRedoManager, _cellTracker, _textClipboard);
            _testing = new ApplicationViewModel
            {
                FunctionTracker = _functionTracker,
                FunctionLoader = _pluginFunctionLoader,
                CellTracker = _cellTracker,
                CellLoader = _cellLoader,
                UserCollectionTracker = _userCollectionTracker,
                CellPopulateManager = _cellPopulateManager,
                CellTriggerManager = _cellTriggerManager,
                SheetTracker = _sheetTracker,
                BackupManager = _backupManager,
                UndoRedoManager = _undoRedoManager,
                CellClipboard = _cellClipboard,
                CellSelector = _cellSelector,
                DialogFactory = _testDialogFactory,
                PersistedProject = _persistedProject,
                ApplicationSettings = _applicationSettings
            };
        }

        [Fact]
        public void BasicLaunchTest()
        {
        }

        [Fact]
        public void UpgradeRequired_LoadStarted_FailsWithoutMigrator()
        {
            _persistedProject.Version = "0";
            Assert.NotEqual("1", _persistedProject.Version);
            _persistedProject.SaveVersion();
            _persistedProject.Version = "1";

            Assert.Throws<CellError>(() => _testing.Load(_userCollectionLoader));
        }

        [Fact]
        public void MigratorExists_LoadStarted_PromptsUserToMigrate()
        {
            _persistedProject.Version = "0";
            _persistedProject.SaveVersion();
            _persistedProject.Version = "1";
            var migrator = new TestMigrator();
            _persistedProject.RegisterMigrator("0", "1", migrator);
            var dialog = _testDialogFactory.Expect();

            _testing.Load(_userCollectionLoader);

            Assert.True(dialog.WasShown);
        }

        [Fact]
        public void MigrationConfirmationDialogOpen_UserConfirms_MigratorInvoked()
        {
            _persistedProject.Version = "0";
            _persistedProject.SaveVersion();
            _persistedProject.Version = "1";
            var migrator = new TestMigrator();
            _persistedProject.RegisterMigrator("0", "1", migrator);
            _testDialogFactory.Expect(0);
            Assert.False(migrator.Migrated);

            _testing.Load(_userCollectionLoader);

            Assert.True(migrator.Migrated);
        }

        [Fact]
        public void ShowToolWindow_ToolWindowAddedToListOfOpenWindows()
        {
            var testToolWindow = new TestToolWindowViewModel();
            Assert.Empty(_testing.OpenToolWindowViewModels);

            _testing.ShowToolWindow(testToolWindow);

            Assert.True(_testing.OpenToolWindowViewModels.Single() == testToolWindow);
        }

        [Fact]
        public void ShowToolWindow_RequestCloseFunctionSet()
        {
            var testToolWindow = new TestToolWindowViewModel();
            Assert.Null(testToolWindow.RequestClose);

            _testing.ShowToolWindow(testToolWindow);

            Assert.NotNull(testToolWindow.RequestClose);
        }

        [Fact]
        public void ShowToolWindow_ShowHandlerCalled()
        {
            var testToolWindow = new TestToolWindowViewModel();
            Assert.Empty(_testing.OpenToolWindowViewModels);

            _testing.ShowToolWindow(testToolWindow);

            Assert.True(_testing.OpenToolWindowViewModels.Single() == testToolWindow);
        }

        [Fact]
        public void ToolWindowNotAllowingClose_RequestCloseFunctionCalled_ToolWindowRemainsOpen()
        {
            var testToolWindow = new TestToolWindowViewModel();
            _testing.ShowToolWindow(testToolWindow);
            Assert.Equal(testToolWindow, _testing.OpenToolWindowViewModels.Single());
            testToolWindow.IsAllowingClose = false;

            testToolWindow.RequestClose!.Invoke();

            Assert.Equal(testToolWindow, _testing.OpenToolWindowViewModels.Single());
        }

        [Fact]
        public void ToolWindowNotAllowingClose_RequestCloseFunctionCalled_ToolWindowClosedHandleNotCalled()
        {
            var testToolWindow = new TestToolWindowViewModel();
            _testing.ShowToolWindow(testToolWindow);
            Assert.Equal(testToolWindow, _testing.OpenToolWindowViewModels.Single());
            testToolWindow.IsAllowingClose = false;

            testToolWindow.RequestClose!.Invoke();

            Assert.False(testToolWindow.WasHandleBeingClosedCalled);
        }

        [Fact]
        public void ToolWindowAllowingClose_RequestCloseFunctionCalled_ToolWindowClosed()
        {
            var testToolWindow = new TestToolWindowViewModel();
            _testing.ShowToolWindow(testToolWindow);
            Assert.Equal(testToolWindow, _testing.OpenToolWindowViewModels.Single());
            testToolWindow.IsAllowingClose = true;

            testToolWindow.RequestClose!.Invoke();

            Assert.Empty(_testing.OpenToolWindowViewModels);
        }

        [Fact]
        public void ToolWindowOpened_WasHandleBeingShownCalled()
        {
            var testToolWindow = new TestToolWindowViewModel();
            Assert.False(testToolWindow.WasHandleBeingShownCalled);
            
            _testing.ShowToolWindow(testToolWindow);

            Assert.True(testToolWindow.WasHandleBeingShownCalled);
        }
    }
}
