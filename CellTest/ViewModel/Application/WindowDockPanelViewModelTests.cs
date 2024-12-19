using Cell.View.ToolWindow;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using CellTest.TestUtilities;
using FontAwesome.Sharp;
using System.Windows;

namespace CellTest.ViewModel.Application
{
    public class WindowDockPanelViewModelTests
    {
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
        public void SetMainContent_HostingPanelSetToOwningPanel()
        {
            var testToolWindowViewModel = new ToolWindowViewModel();
            Assert.Null(testToolWindowViewModel.RequestClose);

            _testing.MainContent = testToolWindowViewModel;

            Assert.Equal(_testing, testToolWindowViewModel.HostingPanel);
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
        public void ToolWindowOpenInMainContent_UndockCommandExecutedFromToolWindow_ToolWindowRemovedFromMainContent()
        {
            var testToolWindowViewModel = new ToolWindowViewModel();
            Assert.Empty(testToolWindowViewModel.ToolBarCommands.Where(x => x.Icon == IconChar.LockOpen));
            _testing.MainContent = testToolWindowViewModel;

            testToolWindowViewModel.ToolBarCommands.First(x => x.Icon == IconChar.LockOpen).Command.Execute(null);

            Assert.NotEqual(testToolWindowViewModel, _testing.MainContent);
        }

        [Fact]
        public void ToolWindowOpenInMainContent_UndockCommandExecutedFromToolWindow_ToolWindowBecomesFloatingWindow()
        {
            var testToolWindowViewModel = new ToolWindowViewModel();
            Assert.Empty(testToolWindowViewModel.ToolBarCommands.Where(x => x.Icon == IconChar.LockOpen));
            _testing.MainContent = testToolWindowViewModel;
            Assert.Empty(_testing.VisibleContentAreasThatAreFloating.Where(x => x.MainContent == testToolWindowViewModel));

            testToolWindowViewModel.ToolBarCommands.First(x => x.Icon == IconChar.LockOpen).Command.Execute(null);

            Assert.Single(_testing.VisibleContentAreasThatAreFloating.Where(x => x.MainContent == testToolWindowViewModel));
        }

        [Fact]
        public void ToolWindowOpenInMainContent_UndockCommandExecutedFromToolWindow_FloatingToolViewHasDockCommandInPosition1()
        {
            var testToolWindowViewModel = new ToolWindowViewModel();
            Assert.Empty(testToolWindowViewModel.ToolBarCommands.Where(x => x.Icon == IconChar.Lock));
            _testing.MainContent = testToolWindowViewModel;

            testToolWindowViewModel.ToolBarCommands.First(x => x.Icon == IconChar.LockOpen).Command.Execute(null);

            Assert.Equal(IconChar.Lock, testToolWindowViewModel.ToolBarCommands[1].Icon);
        }

        [Fact]
        public void ToolWindowOpenInMainContent_UndockCommandExecutedFromToolWindow_FloatingToolViewDesiredSizeSetToDefaultToolWindowSize()
        {
            var testToolWindowViewModel = new ToolWindowViewModel();
            _testing.MainContent = testToolWindowViewModel;

            testToolWindowViewModel.ToolBarCommands.First(x => x.Icon == IconChar.LockOpen).Command.Execute(null);

            Assert.Equal(testToolWindowViewModel.MinimumWidth, _testing.VisibleContentAreasThatAreFloating.First().DesiredWidth);
            Assert.Equal(testToolWindowViewModel.MinimumHeight + DockedToolWindowContainer.ToolBoxHeaderHeight, _testing.VisibleContentAreasThatAreFloating.First().DesiredHeight);
        }

        [Fact]
        public void ChildToolWindow_SecondWindowDockedToTop_MinimumHeightOfPanelIncludesBothPanels()
        {
            var testToolWindowViewModel = new ToolWindowViewModel();
            _testing.ShowToolWindow(testToolWindowViewModel, WindowDockType.DockedTop, false);
            var testToolWindowViewModel2 = new ToolWindowViewModel();
            var topWindowPanel = _testing.VisibleContentAreasThatAreDockedOnTop.Single();
            Assert.NotNull(topWindowPanel.ParentPanel);

            topWindowPanel.ShowToolWindow(testToolWindowViewModel2, WindowDockType.DockedTop, false);

            Assert.Equal(testToolWindowViewModel.MinimumHeight + testToolWindowViewModel2.MinimumHeight + DockedToolWindowContainer.ToolBoxHeaderHeight, topWindowPanel.DesiredHeight);
        }

        [Fact]
        public void ToolWindowJustUndocked_CloseButtonExecuted_FloatingToolWindowsAreEmpty()
        {
            var testToolWindowViewModel = new ToolWindowViewModel();
            _testing.MainContent = testToolWindowViewModel;
            testToolWindowViewModel.ToolBarCommands.First(x => x.Icon == IconChar.LockOpen).Command.Execute(null);

            testToolWindowViewModel.ToolBarCommands.First(x => x.Icon == IconChar.Xmark).Command.Execute(null);

            Assert.Empty(_testing.VisibleContentAreasThatAreFloating);
        }

        [Fact]
        public void ToolWindowJustUndocked_DockButtonExecuted_WindowToDockSetToToolWindowViewModel()
        {
            var testToolWindowViewModel = new ToolWindowViewModel();
            _testing.MainContent = testToolWindowViewModel;
            testToolWindowViewModel.ToolBarCommands.First(x => x.Icon == IconChar.LockOpen).Command.Execute(null);

            testToolWindowViewModel.ToolBarCommands.First(x => x.Icon == IconChar.Lock).Command.Execute(null);

            Assert.Equal(testToolWindowViewModel, _testing.WindowToDock);
        }

        [Fact]
        public void DockButtonExecuted_DockSiteClicked_WindowIsNowInTopDockedList()
        {
            var testToolWindowViewModel = new ToolWindowViewModel();
            _testing.MainContent = testToolWindowViewModel;
            testToolWindowViewModel.ToolBarCommands.First(x => x.Icon == IconChar.LockOpen).Command.Execute(null);
            testToolWindowViewModel.ToolBarCommands.First(x => x.Icon == IconChar.Lock).Command.Execute(null);

            _testing.DockWindowThatIsCurrentlyFloating(testToolWindowViewModel, System.Windows.Controls.Dock.Top);

            Assert.Equal(testToolWindowViewModel, _testing.VisibleContentAreasThatAreDockedOnTop.Single().MainContent);
        }

        [Fact]
        public void DockButtonExecuted_DockSiteClicked_WindowIsNoLongerInFloatingWindowList()
        {
            var testToolWindowViewModel = new ToolWindowViewModel();
            _testing.MainContent = testToolWindowViewModel;
            testToolWindowViewModel.ToolBarCommands.First(x => x.Icon == IconChar.LockOpen).Command.Execute(null);
            testToolWindowViewModel.ToolBarCommands.First(x => x.Icon == IconChar.Lock).Command.Execute(null);

            _testing.DockWindowThatIsCurrentlyFloating(testToolWindowViewModel, System.Windows.Controls.Dock.Top);

            Assert.Empty(_testing.VisibleContentAreasThatAreFloating.Where(x => x.MainContent == testToolWindowViewModel));
        }

        [Fact]
        public void ToolWindowOpenInMainContent_UndockCommandExecutedFromToolWindow_FloatingToolViewHasCloseCommandInPosition0()
        {
            var testToolWindowViewModel = new ToolWindowViewModel();
            _testing.MainContent = testToolWindowViewModel;

            testToolWindowViewModel.ToolBarCommands.First(x => x.Icon == IconChar.LockOpen).Command.Execute(null);

            Assert.Equal(IconChar.Xmark, testToolWindowViewModel.ToolBarCommands[0].Icon);
        }

        [Fact]
        public void ToolWindowOpenInMainContent_UndockCommandExecutedFromToolWindow_FloatingToolViewDoesNotHaveUndockCommand()
        {
            var testToolWindowViewModel = new ToolWindowViewModel();
            Assert.Empty(testToolWindowViewModel.ToolBarCommands.Where(x => x.Icon == IconChar.LockOpen));
            _testing.MainContent = testToolWindowViewModel;
            Assert.Empty(_testing.VisibleContentAreasThatAreFloating.Where(x => x.MainContent == testToolWindowViewModel));

            testToolWindowViewModel.ToolBarCommands.First(x => x.Icon == IconChar.LockOpen).Command.Execute(null);

            Assert.Empty(testToolWindowViewModel.ToolBarCommands.Where(x => x.Icon == IconChar.LockOpen));
        }

        [Fact]
        public void ToolWindowOpenInMainContent_UndockCommandExecutedFromToolWindow_NewlyFloatingToolWindowHasSingleCloseCommand()
        {
            var testToolWindowViewModel = new ToolWindowViewModel();
            Assert.Empty(testToolWindowViewModel.ToolBarCommands.Where(x => x.Icon == IconChar.LockOpen));
            _testing.MainContent = testToolWindowViewModel;
            Assert.Empty(_testing.VisibleContentAreasThatAreFloating.Where(x => x.MainContent == testToolWindowViewModel));

            testToolWindowViewModel.ToolBarCommands.First(x => x.Icon == IconChar.LockOpen).Command.Execute(null);

            Assert.Single(_testing.VisibleContentAreasThatAreFloating.First(x => x.MainContent == testToolWindowViewModel).MainContent?.ToolBarCommands.Where(x => x.Icon == IconChar.Xmark) ?? []);
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

        [Fact]
        public void ShowToolWindowFloating_OneFloatingWindowExists()
        {
            var testToolWindowViewModel = new ToolWindowViewModel();
            Assert.Empty(_testing.VisibleContentAreasThatAreFloating);

            _testing.ShowToolWindow(testToolWindowViewModel, WindowDockType.Floating, false);

            Assert.Single(_testing.VisibleContentAreasThatAreFloating);
        }

        [Fact]
        public void ShowToolWindowFloating_HasDockCommandInPosition1()
        {
            var testToolWindowViewModel = new ToolWindowViewModel();

            _testing.ShowToolWindow(testToolWindowViewModel, WindowDockType.Floating, false);

            Assert.Equal(IconChar.Lock, testToolWindowViewModel.ToolBarCommands[1].Icon);
        }

        [Fact]
        public void ShowToolWindowFloating_HasCloseCommandInPosition0()
        {
            var testToolWindowViewModel = new ToolWindowViewModel();

            _testing.ShowToolWindow(testToolWindowViewModel, WindowDockType.Floating, false);

            Assert.Equal(IconChar.Xmark, testToolWindowViewModel.ToolBarCommands[0].Icon);
        }

        [Fact]
        public void ShowToolWindowFloating_PositionSetToCenterOfCanvas()
        {
            var testToolWindowViewModel = new ToolWindowViewModel();
            _testing.CanvasSize = new Size(1000, 900);

            _testing.ShowToolWindow(testToolWindowViewModel, WindowDockType.Floating, false);

            Assert.Equal(400, testToolWindowViewModel.X);
            Assert.Equal(350 - (DockedToolWindowContainer.ToolBoxHeaderHeight / 2), testToolWindowViewModel.Y);
        }
    }
}
