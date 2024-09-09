using Cell.Data;
using Cell.Execution;
using Cell.Model;
using Cell.Persistence;
using Cell.ViewModel.Cells;
using Cell.ViewModel.Cells.Types;
using Cell.ViewModel.Cells.Types.Special;
using Cell.ViewModel.ToolWindow;
using CellTest.TestUtilities;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace CellTest
{
    public class RowCellViewModelTests
    {
        private TestFileIO _testFileIO;
        private PersistenceManager _persistenceManager;
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

        private RowCellViewModel CreateInstance()
        {
            _testFileIO = new TestFileIO();
            _persistenceManager = new PersistenceManager("", _testFileIO);
            _cellLoader = new CellLoader(_persistenceManager);
            _cellTracker = new CellTracker(_cellLoader);
            _pluginFunctionLoader = new PluginFunctionLoader(_persistenceManager);
            _userCollectionLoader = new UserCollectionLoader(_persistenceManager, _pluginFunctionLoader, _cellTracker);
            _cellPopulateManager = new CellPopulateManager(_cellTracker, _pluginFunctionLoader, _userCollectionLoader);
            _sheetModel = new SheetModel("sheet");
            _sheetTracker = new SheetTracker(_persistenceManager, _cellLoader, _cellTracker, _pluginFunctionLoader, _userCollectionLoader);
            _applicationSettings = new ApplicationSettings();
            _sheetViewModel = new SheetViewModel(_sheetModel, _cellPopulateManager, _cellTracker, _sheetTracker, _userCollectionLoader, _applicationSettings, _pluginFunctionLoader);
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

        [Fact]
        public void A1PopulateFunctionReferencingSheetNameA1_AddRowAbove_FunctionNowReferencesA2()
        {
            var _ = CreateInstance();
            var function = _pluginFunctionLoader.CreateFunction("object", "testFunction", "");
            SheetFactory.CreateSheet("SheetName", 1, 1, _cellTracker);
            var sheet = _sheetTracker.Sheets.Single();
            _sheetViewModel = new SheetViewModel(sheet, _cellPopulateManager, _cellTracker, _sheetTracker, _userCollectionLoader, _applicationSettings, _pluginFunctionLoader);
            var testing = _sheetViewModel.CellViewModels.OfType<RowCellViewModel>().Single();
            var labelCell = _sheetViewModel.CellViewModels.OfType<LabelCellViewModel>().Single();
            function.SetUserFriendlyCode("return SheetName_B_A1;", labelCell.Model, x => x, []);

            testing.AddRowAbove();

            Assert.Equal("return SheetName_B_A2;", _pluginFunctionLoader.ObservableFunctions.Single().GetUserFriendlyCode(labelCell.Model, x => x, []));
        }
    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.