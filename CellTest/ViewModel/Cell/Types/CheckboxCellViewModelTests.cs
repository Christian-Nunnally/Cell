using Cell.Data;
using Cell.Execution;
using Cell.Model;
using Cell.Persistence;
using Cell.ViewModel.Cells;
using Cell.ViewModel.Cells.Types;
using CellTest.TestUtilities;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

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

        private CheckboxCellViewModel CreateInstance()
        {
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
            return new CheckboxCellViewModel(_cellModel, _sheetViewModel);
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

            testing.IsChecked = true;

            propertyChangedTester.AssertPropertyChanged(nameof(testing.IsChecked));
        }
    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.