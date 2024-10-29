using Cell.Model;
using CellTest.TestUtilities;
using Cell.ViewModel.Execution;
using Cell.Core.Execution.Functions;

namespace CellTest.ViewModel.Execution
{
    public class CellFunctionViewModelTests
    {
        private readonly CellFunctionModel _cellFunctionModel;
        private readonly CellFunction _cellFunction;
        private readonly CellFunctionViewModel _testing;

        public CellFunctionViewModelTests()
        {
            _cellFunctionModel = new CellFunctionModel("Test", "return \"Hello world\";", "object");
            _cellFunction = new CellFunction(_cellFunctionModel);
            _testing = new CellFunctionViewModel(_cellFunction);
        }

        [Fact]
        public void BasicLaunchTest()
        {
        }

        [Fact]
        public void ValidFunctionWhereWasLastCompileSuccesfulIsTrue_InvalidCodeEntered_WasLastCompileSuccessNotifiesChange()
        {
            Assert.True(_testing.WasLastCompileSuccesful);
            var propertyChanged = new PropertyChangedTester(_testing);

            _cellFunction.Model.Code = "return 1";

            propertyChanged.AssertPropertyChanged(nameof(_testing.WasLastCompileSuccesful), 1);
            Assert.False(_testing.WasLastCompileSuccesful);
        }
    }
}
