using Cell.Core.Data.Tracker;
using Cell.Model;
using Cell.ViewModel.Application;
using System.Collections.ObjectModel;

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
        public void SetTopWindow()
        {
            _testing.VisibleContentAreasThatAreFloating.Add(new WindowDockPanelViewModel());
        }
    }
}
