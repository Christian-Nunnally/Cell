using Cell.Core.Data;
using Cell.Model;
using Cell.Core.Persistence;
using Cell.ViewModel.ToolWindow;
using CellTest.TestUtilities;
using System.Collections.ObjectModel;
using Cell.Core.Execution.Functions;

namespace CellTest.ViewModel.ToolWindow
{
    public class CodeEditorWindowViewModelTests
    {
        private CellTracker _cellTracker;
        private DictionaryFileIO _testFileIO;
        private PersistedDirectory _persistedDirectory;
        private CellLoader _cellLoader;
        private ObservableCollection<CellModel> _cellsToEdit;
        private PluginFunctionLoader _pluginFunctionLoader;
        private CellFunction _functionBeingEdited;
        private CellModel _cellContext;
        private CodeEditorWindowViewModel _testing;

        public CodeEditorWindowViewModelTests()
        {
            _testFileIO = new DictionaryFileIO();
            _persistedDirectory = new PersistedDirectory("", _testFileIO);
            _cellLoader = new CellLoader(_persistedDirectory);
            _cellTracker = new CellTracker(_cellLoader);
            _cellsToEdit = [];
            _pluginFunctionLoader = new PluginFunctionLoader(_persistedDirectory);
            _functionBeingEdited = _pluginFunctionLoader.CreateCellFunction("Test", "TestFunction");
            _cellContext = new CellModel();
            _testing = new CodeEditorWindowViewModel(_functionBeingEdited, _cellContext, new Dictionary<string, string>());
        }

        [Fact]
        public void BasicLaunchTest()
        {
        }

        [Fact]
        public void NoTextInEditor_SetTestInEditorToTest_TextInEditorSet()
        {
            Assert.Empty(_testing.CurrentTextInEditor);

            _testing.CurrentTextInEditor = "Test";

            Assert.Equal("Test", _testing.CurrentTextInEditor);
        }

        [Fact]
        public void TestSetInEditor_TestCodeRun_()
        {
            _testing.CurrentTextInEditor = "Test";
            Assert.Equal("Test", _testing.CurrentTextInEditor);

            _testing.TestCode();
        }
    }
}
