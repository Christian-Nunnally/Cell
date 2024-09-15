using Cell.Data;
using Cell.Execution;
using Cell.Model;
using Cell.Persistence;
using Cell.ViewModel.Cells;
using CellTest.TestUtilities;
using Cell.ViewModel.Cells.Types;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace CellTest.ViewModel.Cell.Types
{
    public class CornerCellViewModelTests
    {
        private TestFileIO _testFileIO;
        private PersistedDirectory _persistenceManager;
        private CellLoader _cellLoader;
        private CellTracker _cellTracker;
        private PluginFunctionLoader _pluginFunctionLoader;
        private UserCollectionLoader _userCollectionLoader;
        private CellPopulateManager _cellPopulateManager;
        private SheetModel _sheetModel;
        private SheetTracker _sheetTracker;
        private ApplicationSettings _applicationSettings;
        private SheetViewModel _sheetViewModel;
        private CellModel _cellModel;
        private CellSelector _cellSelector;

        private CornerCellViewModel CreateInstance()
        {
            _testFileIO = new TestFileIO();
            _persistenceManager = new PersistedDirectory("", _testFileIO);
            _cellLoader = new CellLoader(_persistenceManager);
            _cellTracker = new CellTracker(_cellLoader);
            _pluginFunctionLoader = new PluginFunctionLoader(_persistenceManager);
            _userCollectionLoader = new UserCollectionLoader(_persistenceManager, _pluginFunctionLoader, _cellTracker);
            _cellPopulateManager = new CellPopulateManager(_cellTracker, _pluginFunctionLoader, _userCollectionLoader);
            _sheetModel = new SheetModel("sheet");
            _sheetTracker = new SheetTracker(_persistenceManager, _cellLoader, _cellTracker, _pluginFunctionLoader, _userCollectionLoader);
            _applicationSettings = new ApplicationSettings();
            _cellSelector = new CellSelector();
            _sheetViewModel = new SheetViewModel(_sheetModel, _cellPopulateManager, _cellTracker, _sheetTracker, _cellSelector, _userCollectionLoader, _applicationSettings, _pluginFunctionLoader);
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