using Cell.Core.Common;
using Cell.Core.Data.Tracker;
using Cell.Model;
using Cell.ViewModel.ToolWindow;
using CellTest.TestUtilities;

namespace CellTest.ViewModel.ToolWindow
{
    public class SheetManagerWindowViewModelTests
    {
        private readonly CellTracker _cellTracker;
        private readonly SheetTracker _sheetTracker;
        private readonly TestDialogFactory _testDialogFactory;
        private readonly SheetManagerWindowViewModel _testing;

        public SheetManagerWindowViewModelTests()
        {
            _cellTracker = new CellTracker();
            _sheetTracker = new SheetTracker(_cellTracker);
            _testDialogFactory = new TestDialogFactory();
            _testing = new SheetManagerWindowViewModel(_sheetTracker, _testDialogFactory);
        }

        [Fact]
        public void BasicLaunchTest()
        {
        }

        [Fact]
        public void UntrackedSheet_MoveSheetDownInOrder_ThrowsException()
        {
            var sheet = new SheetModel("Untracked");

            Assert.Throws<CellError>(() => _testing.MoveSheetDownInOrder(sheet));
        }

        [Fact]
        public void SingleTrackedSheet_MoveSheetDownInOrder_DoesNotChangeOrder()
        {
            CellModelFactory.Create(0, 0, CellType.Label, "TrackedSheet", _cellTracker);
            var sheet = _sheetTracker.Sheets.First();
            var currentOrder = sheet.Order;

            _testing.MoveSheetDownInOrder(sheet);

            Assert.Equal(currentOrder, sheet.Order);
        }

        [Fact]
        public void TwoTrackedSheets_SecondSheetIsSecondInOrder()
        {
            CellModelFactory.Create(0, 0, CellType.Corner, "TrackedSheet1", _cellTracker);
            CellModelFactory.Create(0, 0, CellType.Corner, "TrackedSheet2", _cellTracker);
            var sheet1 = _sheetTracker.Sheets.First();
            var sheet2 = _sheetTracker.Sheets.Last();

            Assert.True(sheet1.Order < sheet2.Order);
        }
    }
}
