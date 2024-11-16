using Cell.Core.Data;
using Cell.Core.Execution;
using Cell.Model;
using Cell.ViewModel.Cells;
using CellTest.TestUtilities;
using Cell.ViewModel.Cells.Types;
using Cell.Core.Data.Tracker;
using Cell.Core.Common;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace CellTest.ViewModel.Cell.Types
{
    public class CornerCellViewModelTests
    {
        private TestDialogFactory _testDialogFactory;
        private CellTracker _cellTracker;
        private FunctionTracker _functionTracker;
        private UserCollectionTracker _userCollectionTracker;
        private CellPopulateManager _cellPopulateManager;
        private SheetModel _sheetModel;
        private SheetViewModel _sheetViewModel;
        private CellModel _cellModel;
        private CellSelector _cellSelector;
        private CellTriggerManager _cellTriggerManager;

        private CornerCellViewModel CreateInstance()
        {
            _testDialogFactory = new TestDialogFactory();
            _cellTracker = new CellTracker();
            _functionTracker = new FunctionTracker(Logger.Null);
            _userCollectionTracker = new UserCollectionTracker(_functionTracker, _cellTracker);
            _cellPopulateManager = new CellPopulateManager(_cellTracker, _functionTracker, _userCollectionTracker, Logger.Null);
            _sheetModel = new SheetModel("sheet");
            _cellSelector = new CellSelector(_cellTracker);
            _cellTriggerManager = new CellTriggerManager(_cellTracker, _functionTracker, _userCollectionTracker, _testDialogFactory, Logger.Null);
            _sheetViewModel = new SheetViewModel(_sheetModel, _cellPopulateManager, _cellTriggerManager, _cellTracker, _cellSelector, _functionTracker);
            _cellModel = new CellModel();
            return new CornerCellViewModel(_cellModel, _sheetViewModel);
        }

        [Fact]
        public void BasicLaunchTest()
        {
            var _ = CreateInstance();
        }

        [Fact]
        public void SimpleTest_ModelTextChanged_ViewModelTextChangedNotified()
        {
            var testing = CreateInstance();
            var propertyChangedTester = new PropertyChangedTester(testing);

            _cellModel.Text = "wololo";

            propertyChangedTester.AssertPropertyChanged(nameof(testing.Text));
        }

        [Fact]
        public void SimpleTest_ViewModelBackgroundHexSet_ViewModelBackgroundHexChangedChangedNotified()
        {
            var testing = CreateInstance();
            var propertyChangedTester = new PropertyChangedTester(testing);

            testing.BackgroundColorHex = "#FF0000";

            propertyChangedTester.AssertPropertyChanged(nameof(testing.BackgroundColorHex));
        }
    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.