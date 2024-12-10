using Cell.Core.Common;
using Cell.Core.Data.Tracker;
using Cell.ViewModel.ToolWindow;

namespace CellTest.ViewModel.ToolWindow
{
    public class FunctionManagerWindowViewModelTests
    {
        private readonly FunctionTracker _functionTracker;
        private readonly FunctionManagerWindowViewModel _testing;

        public FunctionManagerWindowViewModelTests()
        {
            _functionTracker = new FunctionTracker(Logger.Null);
            _testing = new FunctionManagerWindowViewModel(_functionTracker);
        }

        [Fact]
        public void BasicLaunchTest()
        {
        }

        [Fact]
        public void EmptyTracker_CreateNewTriggerFunction_OneFunctionCreated()
        {
            Assert.Empty(_functionTracker.Functions);

            _testing.CreateNewTriggerFunction();

            Assert.Single(_functionTracker.Functions);
        }

        [Fact]
        public void EmptyTracker_CreateNewPopulateFunction_OneFunctionCreated()
        {
            Assert.Empty(_functionTracker.Functions);

            _testing.CreateNewPopulateFunction();

            Assert.Single(_functionTracker.Functions);
        }

        [Fact]
        public void FunctionSelected_CreateCopyOfSelectedFunction_SecondFunctionCreatedInTracker()
        {
            _testing.CreateNewPopulateFunction();
            _testing.SelectedFunction = _testing.FilteredFunctions.First();
            Assert.Single(_functionTracker.Functions);

            _testing.CreateCopyOfSelectedFunction();

            Assert.Equal(2, _functionTracker.Functions.Count);
        }
    }
}
