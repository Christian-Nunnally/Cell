using Cell.Core.Data;
using Cell.Model;
using Cell.Core.Persistence;
using Cell.ViewModel.ToolWindow;
using CellTest.TestUtilities;
using System.Collections.ObjectModel;
using Cell.Core.Execution.Functions;
using Cell.ViewModel.Application;
using Cell.Core.Common;

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
        private UserCollectionLoader _userCollectionLoader;
        private CodeEditorWindowViewModel _testing;

        public CodeEditorWindowViewModelTests()
        {
            _testFileIO = new DictionaryFileIO();
            _persistedDirectory = new PersistedDirectory("", _testFileIO);
            _cellLoader = new CellLoader(_persistedDirectory);
            _cellTracker = new CellTracker(_cellLoader);
            _cellsToEdit = [];
            _pluginFunctionLoader = new PluginFunctionLoader(_persistedDirectory);
            _functionBeingEdited = _pluginFunctionLoader.CreateCellFunction("void", "TestFunction");
            _cellContext = new CellModel();
            _userCollectionLoader = new UserCollectionLoader(_persistedDirectory, _pluginFunctionLoader, _cellTracker);
            var testingContext = new TestingContext(_cellTracker, _userCollectionLoader, new DialogFactory(), _cellContext);
            _testing = new CodeEditorWindowViewModel(_functionBeingEdited, _cellContext, new Dictionary<string, string>(), testingContext);
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
        public void TestSetInEditor_TestCodeRuns()
        {
            _testing.CurrentTextInEditor = "Test";
            Assert.Equal("Test", _testing.CurrentTextInEditor);

            _testing.TestCode();
        }

        [Fact]
        public void CodeToShowDialog_TestCode_LogShowsTestResults()
        {
            _testing.CurrentTextInEditor = "c.ShowDialog(\"test\");";

            _testing.TestCode();

            Assert.Contains("Pretending to show dialog", Logger.Instance.Logs.Single());
        }
    }
}
