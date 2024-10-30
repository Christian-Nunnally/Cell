﻿using Cell.Core.Data;
using Cell.Core.Data.Tracker;
using Cell.Core.Execution;
using Cell.Model;
using Cell.ViewModel.Cells;
using Cell.ViewModel.Cells.Types;
using CellTest.TestUtilities;

namespace CellTest.ViewModel.Cell.Types
{
    public class ColumnCellViewModelTests
    {
        private readonly TestDialogFactory _testDialogFactory;
        private readonly CellTracker _cellTracker;
        private readonly FunctionTracker _functionTracker;
        private readonly UserCollectionTracker _userCollectionTracker;
        private readonly CellPopulateManager _cellPopulateManager;
        private readonly CellTriggerManager _cellTriggerManager;
        private readonly SheetModel _sheetModel;
        private readonly SheetViewModel _sheetViewModel;
        private readonly CellModel _cellModel;
        private readonly CellSelector _cellSelector;
        private readonly ColumnCellViewModel _testing;

        public ColumnCellViewModelTests()
        {
            _testDialogFactory = new TestDialogFactory();
            _cellTracker = new CellTracker();
            _functionTracker = new FunctionTracker();
            _userCollectionTracker = new UserCollectionTracker(_functionTracker, _cellTracker);
            _cellPopulateManager = new CellPopulateManager(_cellTracker, _functionTracker, _userCollectionTracker);
            _cellTriggerManager = new CellTriggerManager(_cellTracker, _functionTracker, _userCollectionTracker, _testDialogFactory);
            _sheetModel = new SheetModel("sheet");
            _cellSelector = new CellSelector(_cellTracker);
            _sheetViewModel = new SheetViewModel(_sheetModel, _cellPopulateManager, _cellTriggerManager, _cellTracker, _cellSelector, _functionTracker);
            _cellModel = new CellModel();
            _testing = new ColumnCellViewModel(_cellModel, _sheetViewModel);
        }


        [Fact]
        public void BasicLaunchTest()
        {
        }

        [Fact]
        public void SimpleTest_ModelTextChanged_ViewModelTextChangedNotified()
        {
            var propertyChangedTester = new PropertyChangedTester(_testing);

            _cellModel.Text = "wololo";

            propertyChangedTester.AssertPropertyChanged(nameof(_testing.Text));
        }

        [Fact]
        public void SimpleTest_ModelFontSizeChanged_ViewModelFontSizeChangedNotified()
        {
            var propertyChangedTester = new PropertyChangedTester(_testing);

            _cellModel.Style.FontSize = 20;

            propertyChangedTester.AssertPropertyChanged(nameof(_testing.FontSize));
        }
    }
}
