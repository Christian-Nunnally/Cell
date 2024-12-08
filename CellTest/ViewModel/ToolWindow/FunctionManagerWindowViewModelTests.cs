using Cell.Core.Common;
using Cell.Core.Data.Tracker;
using Cell.ViewModel.ToolWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
    }
}
