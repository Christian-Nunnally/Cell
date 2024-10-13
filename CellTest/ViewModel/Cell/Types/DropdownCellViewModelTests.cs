using Cell.Core.Data;
using Cell.Core.Execution;
using Cell.Model;
using Cell.Core.Persistence;
using Cell.ViewModel.Cells;
using CellTest.TestUtilities;
using Cell.ViewModel.Cells.Types;

namespace CellTest.ViewModel.Cell.Types
{
    public class DropdownCellViewModelTests
    {
        private readonly DictionaryFileIO _testFileIO;
        private readonly PersistedDirectory _persistedDirectory;
        private readonly CellLoader _cellLoader;
        private readonly CellTracker _cellTracker;
        private readonly PluginFunctionLoader _pluginFunctionLoader;
        private readonly UserCollectionLoader _userCollectionLoader;
        private readonly CellPopulateManager _cellPopulateManager;
        private readonly SheetModel _sheetModel;
        private readonly SheetTracker _sheetTracker;
        private readonly ApplicationSettings _applicationSettings;
        private readonly SheetViewModel _sheetViewModel;
        private readonly CellModel _cellModel;
        private readonly CellSelector _cellSelector;
        private readonly CellTriggerManager _cellTriggerManager;
        private readonly DropdownCellViewModel _testing;

        public DropdownCellViewModelTests()
        {
            _testFileIO = new DictionaryFileIO();
            _persistedDirectory = new PersistedDirectory("", _testFileIO);
            _cellLoader = new CellLoader(_persistedDirectory);
            _cellTracker = new CellTracker(_cellLoader);
            _pluginFunctionLoader = new PluginFunctionLoader(_persistedDirectory);
            _userCollectionLoader = new UserCollectionLoader(_persistedDirectory, _pluginFunctionLoader, _cellTracker);
            _cellPopulateManager = new CellPopulateManager(_cellTracker, _pluginFunctionLoader, _userCollectionLoader);
            _sheetModel = new SheetModel("sheet");
            _sheetTracker = new SheetTracker(_persistedDirectory, _cellLoader, _cellTracker, _pluginFunctionLoader, _userCollectionLoader);
            _applicationSettings = new ApplicationSettings();
            _cellSelector = new CellSelector(_cellTracker);
            _cellTriggerManager = new CellTriggerManager(_cellTracker, _pluginFunctionLoader, _userCollectionLoader);
            _sheetViewModel = new SheetViewModel(_sheetModel, _cellPopulateManager, _cellTriggerManager, _cellTracker, _cellSelector, _pluginFunctionLoader);
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
            _pluginFunctionLoader.CreateCellFunction("object", "TestFunction", "return new List<string> { \"Test1\", \"Test2\", \"Test3\" };");
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
            var assertionDialog = new TestDialogWindowViewModel() { ExpectedMessage = "passed" };
            _pluginFunctionLoader.CreateCellFunction("void", "testTrigger", "c.ShowDialog(\"passed\");");
            _cellModel.TriggerFunctionName = "testTrigger";

            _testing.SelectedItem = "Test";

            Assert.True(assertionDialog.WasShown);
        }
    }
}
