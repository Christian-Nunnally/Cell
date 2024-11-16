using Cell.Core.Data;
using Cell.Core.Execution;
using Cell.Model;
using Cell.ViewModel.Cells;
using Cell.ViewModel.Cells.Types;
using CellTest.TestUtilities;
using Cell.ViewModel.Application;
using Cell.Core.Data.Tracker;
using Cell.Core.Common;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace CellTest.ViewModel.Cell.Types
{
    public class RowCellViewModelTests
    {
        private CellTracker _cellTracker;
        private FunctionTracker _functionTracker;
        private UserCollectionTracker _userCollectionTracker;
        private CellPopulateManager _cellPopulateManager;
        private CellTriggerManager _cellTriggerManager;
        private SheetModel _sheetModel;
        private SheetViewModel _sheetViewModel;
        private CellModel _cellModel;
        private CellSelector _cellSelector;
        private DialogFactoryBase _testDialogFactory;

        private RowCellViewModel CreateInstance()
        {
            _testDialogFactory = new TestDialogFactory();
            _cellTracker = new CellTracker();
            _functionTracker = new FunctionTracker(Logger.Null);
            _userCollectionTracker = new UserCollectionTracker(_functionTracker, _cellTracker);
            _cellPopulateManager = new CellPopulateManager(_cellTracker, _functionTracker, _userCollectionTracker, Logger.Null);
            _cellTriggerManager = new CellTriggerManager(_cellTracker, _functionTracker, _userCollectionTracker, _testDialogFactory, Logger.Null);
            _sheetModel = new SheetModel("sheet");
            _cellSelector = new CellSelector(_cellTracker);
            _sheetViewModel = new SheetViewModel(_sheetModel, _cellPopulateManager, _cellTriggerManager, _cellTracker, _cellSelector, _functionTracker);
            _cellModel = new CellModel();
            return new RowCellViewModel(_cellModel, _sheetViewModel);
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
        public void SimpleTest_ModelFontSizeChanged_ViewModelFontSizeChangedNotified()
        {
            var testing = CreateInstance();
            var propertyChangedTester = new PropertyChangedTester(testing);

            _cellModel.Style.FontSize = 20;

            propertyChangedTester.AssertPropertyChanged(nameof(testing.FontSize));
        }
    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.