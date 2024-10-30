using Cell.Core.Data.Tracker;
using Cell.Core.Execution;
using Cell.Model;
using Cell.ViewModel.ToolWindow;
using System.Collections.ObjectModel;

namespace CellTest.ViewModel.ToolWindow
{
    public class CellContentEditWindowViewModelTests
    {
        private readonly CellTracker _cellTracker;
        private readonly ObservableCollection<CellModel> _cellsToEdit;
        private readonly FunctionTracker _functionTracker;
        private readonly UserCollectionTracker _userCollectionTracker;
        private readonly CellContentEditWindowViewModel _testing;

        public CellContentEditWindowViewModelTests()
        {
            _cellTracker = new CellTracker();
            _cellsToEdit = [];
            _functionTracker = new FunctionTracker();
            _userCollectionTracker = new UserCollectionTracker(_functionTracker, _cellTracker);
            // Enables populate tests.
            var _ = new CellPopulateManager(_cellTracker, _functionTracker, _userCollectionTracker);
            _testing = new CellContentEditWindowViewModel(_cellsToEdit, _functionTracker);
        }

        [Fact]
        public void MultiUseUserInputTextSet_CellTextUnchangedBecauseInputNotSubmitted()
        {
            var cellBeingEdited = new CellModel();
            _cellsToEdit.Add(cellBeingEdited);
            _testing.MultiUseUserInputText = "Test";

            Assert.NotEqual("Test", cellBeingEdited.Text);
        }

        [Fact]
        public void CellAddedToEditList_HandleBeingShown_CellTextDisplayedInEditBox()
        {
            var cellBeingEdited = new CellModel() { Text = "Testing" };
            _cellsToEdit.Add(cellBeingEdited);

            _testing.HandleBeingShown();

            Assert.Equal("Testing", _testing.MultiUseUserInputText);
        }

        [Fact]
        public void CellWithPopulateFunctionAddedToEditList_HandleBeingShown_EqualsSignWithFunctionNameDisplayedInEditBox()
        {
            var cellBeingEdited = new CellModel() { Text = "Testing", PopulateFunctionName = "Populate" };
            _cellsToEdit.Add(cellBeingEdited);

            _testing.HandleBeingShown();

            Assert.Equal("=Populate", _testing.MultiUseUserInputText);
        }

        [Fact]
        public void ToolWindowShown_CellAddedToEditList_CellTextDisplayedInEditBox()
        {
            var cellBeingEdited = new CellModel() { Text = "Testing" };
            _testing.HandleBeingShown();

            _cellsToEdit.Add(cellBeingEdited);

            Assert.Equal("Testing", _testing.MultiUseUserInputText);
        }

        [Fact]
        public void ToolWindowShown_PlainTextSubmittedToMultiUserUserInputTextbox_CellTextSet()
        {
            var cellBeingEdited = new CellModel();
            _cellsToEdit.Add(cellBeingEdited);
            _testing.HandleBeingShown();
            
            _testing.MultiUseUserInputText = "Test";
            _testing.SubmitMultiUseUserInputText();

            Assert.Equal("Test", cellBeingEdited.Text);
        }

        [Fact]
        public void ToolWindowShown_EqualSignTextSubmittedToMultiUserUserInputTextbox_CellTextNotSet()
        {
            var cellBeingEdited = new CellModel();
            _cellsToEdit.Add(cellBeingEdited);
            _testing.HandleBeingShown();

            _testing.MultiUseUserInputText = "=Test";
            _testing.SubmitMultiUseUserInputText();

            Assert.NotEqual("=Test", cellBeingEdited.Text);
            Assert.NotEqual("Test", cellBeingEdited.Text);
        }

        [Fact]
        public void ToolWindowShown_EqualSignTextSubmittedToMultiUserUserInputTextbox_PopulateFunctionSet()
        {
            var cellBeingEdited = new CellModel();
            _cellsToEdit.Add(cellBeingEdited);
            _testing.HandleBeingShown();

            _testing.MultiUseUserInputText = "=Test";
            _testing.SubmitMultiUseUserInputText();

            Assert.Equal("Test", cellBeingEdited.PopulateFunctionName);
        }

        [Fact]
        public void PopulateFunctionExists_PopulateFunctionSet_CellTextSetToPopulateResult()
        {
            var _ = _functionTracker.CreateCellFunction("object", "Test", "return \"Hello world\";");
            var cellBeingEdited = new CellModel();
            _cellTracker.AddCell(cellBeingEdited);
            _cellsToEdit.Add(cellBeingEdited);
            _testing.HandleBeingShown();

            _testing.MultiUseUserInputText = "=Test";
            _testing.SubmitMultiUseUserInputText();

            Assert.Equal("Hello world", cellBeingEdited.Text);
        }

        [Fact]
        public void TriggerFunctionSet_TriggerFunctionChangedToEmptyString_TriggerFunctionEmptied()
        {
            var _ = _functionTracker.CreateCellFunction("void", "Test", "");
            var cellBeingEdited = new CellModel();
            _cellsToEdit.Add(cellBeingEdited);
            _testing.HandleBeingShown();
            _testing.TriggerFunctionNameTextboxText = "Test";
            Assert.NotEmpty(cellBeingEdited.TriggerFunctionName);

            _testing.TriggerFunctionNameTextboxText = "";

            Assert.Empty(cellBeingEdited.TriggerFunctionName);
            Assert.Empty(_testing.TriggerFunctionNameTextboxText);
        }
    }
}
