using Cell.Core.Data;
using Cell.Core.Execution;
using Cell.Model;
using Cell.Core.Persistence;
using Cell.ViewModel.Cells;
using Cell.ViewModel.Cells.Types;
using CellTest.TestUtilities;

namespace CellTest.ViewModel.Cell.Types
{
    public class CheckboxCellViewModelTests
    {
        private DictionaryFileIO _testFileIO;
        private PersistedDirectory _persistedDirectory;
        private CellLoader _cellLoader;
        private CellTracker _cellTracker;
        private PluginFunctionLoader _pluginFunctionLoader;
        private UserCollectionLoader _userCollectionLoader;
        private CellPopulateManager _cellPopulateManager;
        private CellTriggerManager _cellTriggerManager;
        private SheetModel _sheetModel;
        private SheetViewModel _sheetViewModel;
        private CellModel _cellModel;
        private CellSelector _cellSelector;
        private CheckboxCellViewModel _testing;

        public CheckboxCellViewModelTests()
        {
            TestDialogWindowViewModel.Reset();
            _testFileIO = new DictionaryFileIO();
            _persistedDirectory = new PersistedDirectory("", _testFileIO);
            _cellLoader = new CellLoader(_persistedDirectory);
            _cellTracker = new CellTracker(_cellLoader);
            _pluginFunctionLoader = new PluginFunctionLoader(_persistedDirectory);
            _userCollectionLoader = new UserCollectionLoader(_persistedDirectory, _pluginFunctionLoader, _cellTracker);
            _cellPopulateManager = new CellPopulateManager(_cellTracker, _pluginFunctionLoader, _userCollectionLoader);
            _cellTriggerManager = new CellTriggerManager(_cellTracker, _pluginFunctionLoader, _userCollectionLoader);
            _sheetModel = new SheetModel("sheet");
            _cellSelector = new CellSelector(_cellTracker);
            _sheetViewModel = new SheetViewModel(_sheetModel, _cellPopulateManager, _cellTriggerManager, _cellTracker, _cellSelector, _pluginFunctionLoader);
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
            var assertionDialog = new TestDialogWindowViewModel() { ExpectedMessage = "passed" };
            _pluginFunctionLoader.CreateCellFunction("void", "testTrigger", "c.ShowDialog(\"passed\");");
            _cellModel.TriggerFunctionName = "testTrigger";
            Assert.False(_testing.IsChecked);

            _cellModel.Check(true);

            Assert.False(assertionDialog.WasShown);
        }

        [Fact]
        public void ModelTextSetToFalse_IsCheckedSetToTrueOnViewModel_TriggerFunctionTriggered()
        {
            var assertionDialog = new TestDialogWindowViewModel() { ExpectedMessage = "passed" };
            _pluginFunctionLoader.CreateCellFunction("void", "testTrigger", "c.ShowDialog(\"passed\");");
            _cellModel.TriggerFunctionName = "testTrigger";
            Assert.False(_testing.IsChecked);

            _testing.IsChecked = true;

            Assert.True(assertionDialog.WasShown);
        }
    }
}
