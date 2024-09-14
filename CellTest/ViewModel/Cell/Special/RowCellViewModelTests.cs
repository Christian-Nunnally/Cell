using Cell.Data;
using Cell.Execution;
using Cell.Model;
using Cell.Persistence;
using Cell.ViewModel.Cells;
using Cell.ViewModel.Cells.Types;
using Cell.ViewModel.Cells.Types.Special;
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
        private CellSelector _cellSelector;

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
            _cellSelector = new CellSelector();
            _sheetViewModel = new SheetViewModel(_sheetModel, _cellPopulateManager, _cellTracker, _sheetTracker, _cellSelector, _userCollectionLoader, _applicationSettings, _pluginFunctionLoader);
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
            _sheetViewModel = new SheetViewModel(sheet, _cellPopulateManager, _cellTracker, _sheetTracker, _cellSelector, _userCollectionLoader, _applicationSettings, _pluginFunctionLoader);
            var testing = _sheetViewModel.CellViewModels.OfType<RowCellViewModel>().Single();
            var labelCell = _sheetViewModel.CellViewModels.OfType<LabelCellViewModel>().Single();
            function.SetUserFriendlyCode("return SheetName_B_A1;", labelCell.Model, x => x, []);

            testing.AddRowAbove();

            Assert.Equal("return SheetName_B_A2;", _pluginFunctionLoader.ObservableFunctions.Single().GetUserFriendlyCode(labelCell.Model, x => x, []));
        }

        [Fact]
        public void A1PopulateFunctionReferencingSheetNameA1_AddRowBelow_FunctionStillReferencesA1()
        {
            var _ = CreateInstance();
            var function = _pluginFunctionLoader.CreateFunction("object", "testFunction", "");
            SheetFactory.CreateSheet("SheetName", 1, 1, _cellTracker);
            var sheet = _sheetTracker.Sheets.Single();
            _sheetViewModel = new SheetViewModel(sheet, _cellPopulateManager, _cellTracker, _sheetTracker, _cellSelector, _userCollectionLoader, _applicationSettings, _pluginFunctionLoader);
            var testing = _sheetViewModel.CellViewModels.OfType<RowCellViewModel>().Single();
            var labelCell = _sheetViewModel.CellViewModels.OfType<LabelCellViewModel>().Single();
            function.SetUserFriendlyCode("return SheetName_B_A1;", labelCell.Model, x => x, []);

            testing.AddRowBelow();

            Assert.Equal("return SheetName_B_A1;", _pluginFunctionLoader.ObservableFunctions.Single().GetUserFriendlyCode(labelCell.Model, x => x, []));
        }

        [Fact]
        public void A1PopulateFunctionReferencingSheetRangeA1ToA1_AddRowAbove_FunctionNowReferencesA2ToA2()
        {
            var _ = CreateInstance();
            var function = _pluginFunctionLoader.CreateFunction("object", "testFunction", "");
            SheetFactory.CreateSheet("SheetName", 1, 1, _cellTracker);
            var sheet = _sheetTracker.Sheets.Single();
            _sheetViewModel = new SheetViewModel(sheet, _cellPopulateManager, _cellTracker, _sheetTracker, _cellSelector, _userCollectionLoader, _applicationSettings, _pluginFunctionLoader);
            var testing = _sheetViewModel.CellViewModels.OfType<RowCellViewModel>().Single();
            var labelCell = _sheetViewModel.CellViewModels.OfType<LabelCellViewModel>().Single();
            function.SetUserFriendlyCode("return SheetName_B_A1..B_A1;", labelCell.Model, x => x, []);

            testing.AddRowAbove();

            Assert.Equal("return SheetName_B_A2..B_A2;", _pluginFunctionLoader.ObservableFunctions.Single().GetUserFriendlyCode(labelCell.Model, x => x, []));
        }

        [Fact]
        public void A1PopulateFunctionReferencingSheetRangeA1ToA1_AddRowBelow_FunctionNowReferencesA1ToA2()
        {
            var _ = CreateInstance();
            var function = _pluginFunctionLoader.CreateFunction("object", "testFunction", "");
            SheetFactory.CreateSheet("SheetName", 1, 1, _cellTracker);
            var sheet = _sheetTracker.Sheets.Single();
            _sheetViewModel = new SheetViewModel(sheet, _cellPopulateManager, _cellTracker, _sheetTracker, _cellSelector, _userCollectionLoader, _applicationSettings, _pluginFunctionLoader);
            var testing = _sheetViewModel.CellViewModels.OfType<RowCellViewModel>().Single();
            var labelCell = _sheetViewModel.CellViewModels.OfType<LabelCellViewModel>().Single();
            function.SetUserFriendlyCode("return SheetName_B_A1..B_A1;", labelCell.Model, x => x, []);

            testing.AddRowBelow();

            Assert.Equal("return SheetName_B_A1..B_A2;", _pluginFunctionLoader.ObservableFunctions.Single().GetUserFriendlyCode(labelCell.Model, x => x, []));
        }

        [Fact]
        public void SingleRow_AddRowAbove_NewRowGetsIndexOfExistingRow()
        {
            var _ = CreateInstance();
            var function = _pluginFunctionLoader.CreateFunction("object", "testFunction", "");
            SheetFactory.CreateSheet("SheetName", 1, 1, _cellTracker);
            var sheet = _sheetTracker.Sheets.Single();
            _sheetViewModel = new SheetViewModel(sheet, _cellPopulateManager, _cellTracker, _sheetTracker, _cellSelector, _userCollectionLoader, _applicationSettings, _pluginFunctionLoader);
            var testing = _sheetViewModel.CellViewModels.OfType<RowCellViewModel>().Single();
            var labelCell = _cellTracker.AllCells.Where(x => !x.CellType.IsSpecial()).Single();
            labelCell.Index = 4;

            testing.AddRowAbove();

            var addedLabel = _cellTracker.AllCells.Where(x => !x.CellType.IsSpecial() && x != labelCell).Single();
            Assert.Equal(4, addedLabel.Index);
            Assert.Equal(5, labelCell.Index);
        }

        [Fact]
        public void SingleRow_AddRowBelow_NewRowIndexSetToNextIndex()
        {
            var _ = CreateInstance();
            var function = _pluginFunctionLoader.CreateFunction("object", "testFunction", "");
            SheetFactory.CreateSheet("SheetName", 1, 1, _cellTracker);
            var sheet = _sheetTracker.Sheets.Single();
            _sheetViewModel = new SheetViewModel(sheet, _cellPopulateManager, _cellTracker, _sheetTracker, _cellSelector, _userCollectionLoader, _applicationSettings, _pluginFunctionLoader);
            var testing = _sheetViewModel.CellViewModels.OfType<RowCellViewModel>().Single();
            var labelCell = _cellTracker.AllCells.Where(x => !x.CellType.IsSpecial()).Single();
            labelCell.Index = 1;

            testing.AddRowBelow();

            var addedLabel = _cellTracker.AllCells.Where(x => !x.CellType.IsSpecial() && x != labelCell).Single();
            Assert.Equal(2, addedLabel.Index);
        }
    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.