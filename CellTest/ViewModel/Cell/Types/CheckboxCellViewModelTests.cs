using Cell.Core.Data;
using Cell.Core.Execution;
using Cell.Model;
using Cell.Core.Persistence;
using Cell.ViewModel.Cells;
using Cell.ViewModel.Cells.Types;
using CellTest.TestUtilities;
using Cell.Core.Common;

namespace CellTest.ViewModel.Cell.Types
{
    public class CheckboxCellViewModelTests
    {
        private readonly TestDialogFactory _testDialogFactory;
        private readonly DictionaryFileIO _testFileIO;
        private readonly PersistedDirectory _persistedDirectory;
        private readonly CellTracker _cellTracker;
        private readonly FunctionTracker _functionTracker;
        private readonly UserCollectionLoader _userCollectionLoader;
        private readonly CellPopulateManager _cellPopulateManager;
        private readonly CellTriggerManager _cellTriggerManager;
        private readonly SheetModel _sheetModel;
        private readonly SheetViewModel _sheetViewModel;
        private readonly CellModel _cellModel;
        private readonly CellSelector _cellSelector;
        private readonly CheckboxCellViewModel _testing;

        public CheckboxCellViewModelTests()
        {
            _testDialogFactory = new TestDialogFactory();
            _testFileIO = new DictionaryFileIO();
            _persistedDirectory = new PersistedDirectory("", _testFileIO);
            _cellTracker = new CellTracker();
            _functionTracker = new FunctionTracker();
            _userCollectionLoader = new UserCollectionLoader(_persistedDirectory, _functionTracker, _cellTracker);
            _cellPopulateManager = new CellPopulateManager(_cellTracker, _functionTracker, _userCollectionLoader);
            _cellTriggerManager = new CellTriggerManager(_cellTracker, _functionTracker, _userCollectionLoader, _testDialogFactory);
            _sheetModel = new SheetModel("sheet");
            _cellSelector = new CellSelector(_cellTracker);
            _sheetViewModel = new SheetViewModel(_sheetModel, _cellPopulateManager, _cellTriggerManager, _cellTracker, _cellSelector, _functionTracker);
            _cellModel = new CellModel();
            _testing = new CheckboxCellViewModel(_cellModel, _sheetViewModel);
        }


        [Fact]
        public void BasicLaunchTest()
        {
        }

        [Fact]
        public void IsCheckedSetFalse_IsCheckedSetTrue_ViewModelIsCheckedChangedNotified()
        {
            var propertyChangedTester = new PropertyChangedTester(_testing);
            Assert.False(_testing.IsChecked);

            _testing.IsChecked = true;

            propertyChangedTester.AssertPropertyChanged(nameof(_testing.IsChecked));
        }

        [Fact]
        public void ModelTextSetToFalse_ModelTextChanged_ViewModelIsCheckedChangedNotified()
        {
            var propertyChangedTester = new PropertyChangedTester(_testing);
            Assert.False(_testing.IsChecked);

            _cellModel.Check(true);

            propertyChangedTester.AssertPropertyChanged(nameof(_testing.IsChecked));
        }

        [Fact]
        public void ModelTextSetToFalse_ModelTextChanged_TriggerFunctionNotTriggered()
        {
            var assertionDialog = _testDialogFactory.Expect("passed");
            _functionTracker.CreateCellFunction("void", "testTrigger", "c.ShowDialog(\"passed\");");
            _cellModel.TriggerFunctionName = "testTrigger";
            Assert.False(_testing.IsChecked);

            _cellModel.Check(true);

            Assert.False(assertionDialog.WasShown);
        }

        [Fact]
        public void ModelTextSetToFalse_IsCheckedSetToTrueOnViewModel_TriggerFunctionTriggered()
        {
            var assertionDialog = _testDialogFactory.Expect("passed");
            _functionTracker.CreateCellFunction("void", "testTrigger", "c.ShowDialog(\"passed\");");
            _cellModel.TriggerFunctionName = "testTrigger";
            Assert.False(_testing.IsChecked);

            _testing.IsChecked = true;

            Assert.Empty(Logger.Instance.Logs);
            Assert.True(assertionDialog.WasShown);
        }
    }
}
