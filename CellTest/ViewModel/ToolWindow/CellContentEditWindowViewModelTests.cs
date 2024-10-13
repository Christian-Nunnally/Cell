using Cell.Data;
using Cell.Execution;
using Cell.Model;
using Cell.Persistence;
using Cell.ViewModel.ToolWindow;
using CellTest.TestUtilities;
using System.Collections.ObjectModel;

namespace CellTest.ViewModel.ToolWindow
{
    public class CellContentEditWindowViewModelTests
    {
        private readonly CellTracker _cellTracker;
        private readonly DictionaryFileIO _testFileIO;
        private readonly PersistedDirectory _persistedDirectory;
        private readonly CellLoader _cellLoader;
        private readonly ObservableCollection<CellModel> _cellsToEdit;
        private readonly PluginFunctionLoader _pluginFunctionLoader;
        private readonly UserCollectionLoader _userCollectionLoader;
        private readonly CellPopulateManager _cellPopulateManager;
        private readonly CellContentEditWindowViewModel _testing;

        public CellContentEditWindowViewModelTests()
        {
            _testFileIO = new DictionaryFileIO();
            _persistedDirectory = new PersistedDirectory("", _testFileIO);
            _cellLoader = new CellLoader(_persistedDirectory);
            _cellTracker = new CellTracker(_cellLoader);
            _cellsToEdit = [];
            _pluginFunctionLoader = new PluginFunctionLoader(_persistedDirectory);
            _userCollectionLoader = new UserCollectionLoader(_persistedDirectory, _pluginFunctionLoader, _cellTracker);
            // Enables populate tests.
            _cellPopulateManager = new CellPopulateManager(_cellTracker, _pluginFunctionLoader, _userCollectionLoader);
            _testing = new CellContentEditWindowViewModel(_cellsToEdit);
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
            var function = _pluginFunctionLoader.CreateCellFunction("object", "Test", "return \"Hello world\";");
            var cellBeingEdited = new CellModel();
            _cellTracker.AddCell(cellBeingEdited, false);
            _cellsToEdit.Add(cellBeingEdited);
            _testing.HandleBeingShown();

            _testing.MultiUseUserInputText = "=Test";
            _testing.SubmitMultiUseUserInputText();

            Assert.Equal("Hello world", cellBeingEdited.Text);
        }
    }
}
