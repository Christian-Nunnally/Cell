using Cell.Core.Data;
using Cell.Core.Execution;
using Cell.Model;
using Cell.Core.Persistence;
using Cell.ViewModel.Cells;
using CellTest.TestUtilities;
using Cell.ViewModel.Cells.Types;
using Cell.Core.Common;
using Cell.Core.Data.Tracker;

namespace CellTest.ViewModel.Cell.Types
{
    public class DropdownCellViewModelTests
    {
        private readonly TestDialogFactory _testDialogFactory;
        private readonly DictionaryFileIO _testFileIO;
        private readonly PersistedDirectory _persistedDirectory;
        private readonly CellTracker _cellTracker;
        private readonly FunctionTracker _functionTracker;
        private readonly UserCollectionTracker _userCollectionTracker;
        private readonly CellPopulateManager _cellPopulateManager;
        private readonly SheetModel _sheetModel;
        private readonly SheetViewModel _sheetViewModel;
        private readonly CellModel _cellModel;
        private readonly CellSelector _cellSelector;
        private readonly CellTriggerManager _cellTriggerManager;
        private readonly DropdownCellViewModel _testing;

        public DropdownCellViewModelTests()
        {
            _testDialogFactory = new TestDialogFactory();
            _testFileIO = new DictionaryFileIO();
            _persistedDirectory = new PersistedDirectory("", _testFileIO);
            _cellTracker = new CellTracker();
            _functionTracker = new FunctionTracker();
            _userCollectionTracker = new UserCollectionTracker(_functionTracker, _cellTracker);
            _cellPopulateManager = new CellPopulateManager(_cellTracker, _functionTracker, _userCollectionTracker);
            _sheetModel = new SheetModel("sheet");
            _cellSelector = new CellSelector(_cellTracker);
            _cellTriggerManager = new CellTriggerManager(_cellTracker, _functionTracker, _userCollectionTracker, _testDialogFactory);
            _sheetViewModel = new SheetViewModel(_sheetModel, _cellPopulateManager, _cellTriggerManager, _cellTracker, _cellSelector, _functionTracker);
            _cellModel = new CellModel() { CellType = CellType.Dropdown };
            _cellTracker.AddCell(_cellModel);
            _testing = new DropdownCellViewModel(_cellModel, _sheetViewModel);
        }

        [Fact]
        public void BasicLaunchTest()
        {
        }

        [Fact]
        public void NoPopulateSet_PopulateFunctionSetToExistingFunction_DropdownItemsSetToFunctionResult()
        {
            _functionTracker.CreateCellFunction("object", "TestFunction", "return new List<string> { \"Test1\", \"Test2\", \"Test3\" };");
            Assert.Empty(_testing.CollectionDisplayStrings);

            _cellModel.PopulateFunctionName = "TestFunction";

            Assert.Equal(3, _testing.CollectionDisplayStrings.Count);
        }

        [Fact]
        public void SelectedItemNotSet_SelectedItemSet_CellTextSetToSelectedItem()
        {
            Assert.Equal("", _testing.Text);

            _testing.SelectedItem = "Test";

            Assert.Equal("Test", _testing.Text);
        }

        [Fact]
        public void SelectedItemChanged_TriggerFunctionTriggered()
        {
            var assertionDialog = _testDialogFactory.Expect("passed");
            _functionTracker.CreateCellFunction("void", "testTrigger", "c.ShowDialog(\"passed\");");
            _cellModel.TriggerFunctionName = "testTrigger";

            _testing.SelectedItem = "Test";

            Assert.Empty(Logger.Instance.Logs);
            Assert.True(assertionDialog.WasShown);
        }
    }
}
