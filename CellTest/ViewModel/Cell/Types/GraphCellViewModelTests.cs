using Cell.Core.Data;
using Cell.Core.Execution;
using Cell.Model;
using Cell.ViewModel.Cells;
using Cell.ViewModel.Cells.Types;
using CellTest.TestUtilities;
using Cell.ViewModel.Application;
using Cell.Core.Data.Tracker;
using Cell.Core.Common;

namespace CellTest.ViewModel.Cell.Types
{
    public class GraphCellViewModelTests
    {
        private readonly CellTracker _cellTracker;
        private readonly FunctionTracker _functionTracker;
        private readonly UserCollectionTracker _userCollectionTracker;
        private readonly CellPopulateManager _cellPopulateManager;
        private readonly CellTriggerManager _cellTriggerManager;
        private readonly SheetModel _sheetModel;
        private readonly SheetViewModel _sheetViewModel;
        private readonly CellModel _cellModel;
        private readonly CellSelector _cellSelector;
        private readonly DialogFactoryBase _testDialogFactory;
        private readonly GraphCellViewModel _testing;
        private readonly UndoRedoManager _undoRedoManager;

        public GraphCellViewModelTests()
        {
            _testDialogFactory = new TestDialogFactory();
            _cellTracker = new CellTracker();
            _functionTracker = new FunctionTracker(Logger.Null);
            _userCollectionTracker = new UserCollectionTracker(_functionTracker, _cellTracker);
            _cellPopulateManager = new CellPopulateManager(_cellTracker, _functionTracker, _userCollectionTracker, Logger.Null);
            _cellTriggerManager = new CellTriggerManager(_cellTracker, _functionTracker, _userCollectionTracker, _testDialogFactory, Logger.Null);
            _sheetModel = new SheetModel("sheet");
            _cellSelector = new CellSelector(_cellTracker);
            _undoRedoManager = new UndoRedoManager(_cellTracker, _functionTracker);
            _sheetViewModel = new SheetViewModel(_sheetModel, _cellPopulateManager, _cellTriggerManager, _cellTracker, _cellSelector, _undoRedoManager, _functionTracker);
            _cellModel = new CellModel
            {
                CellType = CellType.Graph
            };
            _cellTracker.AddCell(_cellModel);
            _testing = new GraphCellViewModel(_cellModel, _sheetViewModel);
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

        [Fact]
        public void SimpleTest_ModelContentBorderColorChanged_ViewModelBorderColorChangedNotified()
        {
            var propertyChangedTester = new PropertyChangedTester(_testing);

            _cellModel.Style.ContentBorderColor = "#efefef";

            propertyChangedTester.AssertPropertyChanged(nameof(_testing.ContentBorderColor));
        }

        [Fact]
        public void NoFunctionSet_FunctionSetThatReturnsTwoPointsInList_GraphPointsUpdated()
        {
            var propertyChangedTester = new PropertyChangedTester(_testing);
            var twoPointsFunction = _functionTracker.GetOrCreateFunction("object", "TwoPoints");
            var code = "return new List<int> { 1, 2 };";
            twoPointsFunction.SetUserFriendlyCode(code, _testing.Model, new Dictionary<string, string>());

            _cellModel.PopulateFunctionName = "TwoPoints";

            propertyChangedTester.AssertPropertyChanged(nameof(_testing.DataPoints));
        }
    }
}
