using Cell.Core.Data.Tracker;
using Cell.Model;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using CellTest.TestUtilities;
using FontAwesome.Sharp;
using System.Collections.ObjectModel;
using System.Runtime.ExceptionServices;

namespace CellTest.ViewModel.Application
{
    public class WindowDockPanelViewModelTests
    {
        private readonly CellTracker _cellTracker;
        private readonly ObservableCollection<CellModel> _cellsToEdit;
        private readonly FunctionTracker _functionTracker;
        private readonly UserCollectionTracker _userCollectionTracker;
        private readonly WindowDockPanelViewModel _testing;

        public WindowDockPanelViewModelTests()
        {
            _testing = new WindowDockPanelViewModel();
        }

        [Fact]
        public void BasicLaunchTest()
        {
        }

        [Fact]
        public void SetMainContent_PropertyChangeNotified()
        {
            var propertyChangedTester = new PropertyChangedTester(_testing);

            _testing.MainContent = new ToolWindowViewModel();

            propertyChangedTester.AssertPropertyChanged(nameof(_testing.MainContent));
        }

        [Fact]
        public void SetMainContent_RequestCloseNowSet()
        {
            var testToolWindowViewModel = new ToolWindowViewModel();
            Assert.Null(testToolWindowViewModel.RequestClose);

            _testing.MainContent = testToolWindowViewModel;

            Assert.NotNull(testToolWindowViewModel.RequestClose);
        }

        [Fact]
        public void SetMainContent_CloseCommandAddedToViewModelInFirstPosition()
        {
            var testToolWindowViewModel = new ToolWindowViewModel();
            Assert.Empty(testToolWindowViewModel.ToolBarCommands.Where(x => x.Icon == IconChar.Xmark));

            _testing.MainContent = testToolWindowViewModel;

            Assert.Equal(IconChar.Xmark, testToolWindowViewModel.ToolBarCommands[0].Icon);
        }

        [Fact]
        public void MainContentOpen_CloseCommandExecuted_MainWindowClosed()
        {
            var testToolWindowViewModel = new ToolWindowViewModel();
            _testing.MainContent = testToolWindowViewModel;
            var closeCommand = testToolWindowViewModel.ToolBarCommands.First(x => x.Icon == IconChar.Xmark);

            closeCommand.Command.Execute(null);

            Assert.Null(_testing.MainContent);
        }

        [Fact]
        public void SetMainContent_DockCommandAddedToViewModel()
        {
            var testToolWindowViewModel = new ToolWindowViewModel();
            Assert.Empty(testToolWindowViewModel.ToolBarCommands.Where(x => x.Icon == IconChar.LockOpen));

            _testing.MainContent = testToolWindowViewModel;

            Assert.Single(testToolWindowViewModel.ToolBarCommands.Where(x => x.Icon == IconChar.LockOpen));
        }

        [Fact]
        public void ToolWindowOpenInMainContent_DockCommandExecutedFromToolWindow_ToolWindowRemovedFromMainContent()
        {
            var testToolWindowViewModel = new ToolWindowViewModel();
            Assert.Empty(testToolWindowViewModel.ToolBarCommands.Where(x => x.Icon == IconChar.LockOpen));
            _testing.MainContent = testToolWindowViewModel;

            testToolWindowViewModel.ToolBarCommands.First(x => x.Icon == IconChar.LockOpen).Command.Execute(null);

            Assert.NotEqual(testToolWindowViewModel, _testing.MainContent);
        }

        [Fact]
        public void ToolWindowOpenInMainContent_DockCommandExecutedFromToolWindow_ToolWindowBecomesFloatingWindow()
        {
            var testToolWindowViewModel = new ToolWindowViewModel();
            Assert.Empty(testToolWindowViewModel.ToolBarCommands.Where(x => x.Icon == IconChar.LockOpen));
            _testing.MainContent = testToolWindowViewModel;
            Assert.Empty(_testing.VisibleContentAreasThatAreFloating.Where(x => x.MainContent == testToolWindowViewModel));

            testToolWindowViewModel.ToolBarCommands.First(x => x.Icon == IconChar.LockOpen).Command.Execute(null);

            Assert.Single(_testing.VisibleContentAreasThatAreFloating.Where(x => x.MainContent == testToolWindowViewModel));
        }

        [Fact]
        public void ToolWindowInMainContent_ToolWindowRequestsClose_MainContentSetToNull()
        {
            var propertyChangedTester = new PropertyChangedTester(_testing);
            var testToolWindowViewModel = new ToolWindowViewModel();
            _testing.MainContent = testToolWindowViewModel;
            Assert.NotNull(_testing.MainContent);

            testToolWindowViewModel?.RequestClose?.Invoke();

            Assert.Null(_testing.MainContent);
        }
    }
}
