using Cell.Core.Common;
using Cell.Core.Data.Tracker;
using Cell.Core.Execution.Functions;
using Cell.Model;

namespace CellTest.Core.Execution.Functions
{
    public class CellFunctionTests
    {
        private readonly CellFunction _testing;
        private readonly CellFunctionModel _functionModel;
        private readonly CellTracker _cellTracker;
        private readonly UserCollectionTracker _userCollectionTracker;
        private readonly CellModel _contextCell;
        private readonly TestingContext _testingContext;
        private readonly FunctionTracker _functionTracker;
        public CellFunctionTests()
        {
            _cellTracker = new CellTracker();
            _contextCell = new CellModel();
            _functionTracker = new FunctionTracker(Logger.Null);
            _userCollectionTracker = new UserCollectionTracker(_functionTracker, _cellTracker);
            _functionModel = new CellFunctionModel("testFunction", "", "void");
            _testingContext = new TestingContext(_cellTracker, _userCollectionTracker, _contextCell, _functionTracker, Logger.Null);
            _testing = new CellFunction(_functionModel, Logger.Null);
        }

        [Fact]
        public void BasicLaunchTest()
        {
        }

        [Fact]
        public void EmptyVoidFunction_Run_Success()
        {
            var result = _testing.Run(_testingContext);

            Assert.True(result.WasSuccess);
        }

        //[Fact]
        //public void InfiniteLoop_Run_TimeoutException()
        //{
        //    _testing.SetUserFriendlyCode("while(true) {}", new CellModel(), []);

        //    var result = _testing.Run(_testingContext);

        //    Assert.False(result.WasSuccess);
        //}
    }
}
