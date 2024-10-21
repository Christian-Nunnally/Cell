using Cell.Core.Data;
using Cell.Model;
using Cell.Core.Persistence;
using Cell.ViewModel.ToolWindow;
using CellTest.TestUtilities;
using Cell.Core.Execution.Functions;
using Cell.Core.Common;
using Cell.Model.Plugin;

namespace CellTest.ViewModel.ToolWindow
{
    public class CodeEditorWindowViewModelTests
    {
        private readonly CellTracker _cellTracker;
        private readonly DictionaryFileIO _testFileIO;
        private readonly PersistedDirectory _persistedDirectory;
        private readonly CellLoader _cellLoader;
        private readonly PluginFunctionLoader _pluginFunctionLoader;
        private readonly CellFunction _functionBeingEdited;
        private readonly CellModel _cellContext;
        private readonly UserCollectionLoader _userCollectionLoader;
        private readonly CodeEditorWindowViewModel _testing;
        private readonly TestingContext _testingContext;
        private readonly Dictionary<string, string> _collectionNameMap;

        public CodeEditorWindowViewModelTests()
        {
            Logger.Instance.Clear();
            _testFileIO = new DictionaryFileIO();
            _persistedDirectory = new PersistedDirectory("", _testFileIO);
            _cellLoader = new CellLoader(_persistedDirectory);
            _cellTracker = new CellTracker(_cellLoader);
            _pluginFunctionLoader = new PluginFunctionLoader(_persistedDirectory);
            _functionBeingEdited = _pluginFunctionLoader.CreateCellFunction("void", "TestFunction");
            _cellContext = new CellModel();
            _userCollectionLoader = new UserCollectionLoader(_persistedDirectory, _pluginFunctionLoader, _cellTracker);
            _testingContext = new TestingContext(_cellTracker, _userCollectionLoader, _cellContext, _pluginFunctionLoader);
            _collectionNameMap = [];
            _testing = new CodeEditorWindowViewModel(_functionBeingEdited, _cellContext, _collectionNameMap, _testingContext);
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

        [Fact]
        public void CollectionExistsWithOneItemAndTestCodeGetsItem_TestCode_LogShowsDialogContainedItemTitle()
        {
            var realCollection = _userCollectionLoader.CreateCollection("TestCollection", "TodoItem", "");
            _collectionNameMap.Add("TestCollection", "TodoItem");
            var todoItem = new TodoItem
            {
                Title = "test passed"
            };
            realCollection.Add(todoItem);
            _testing.CurrentTextInEditor = "c.ShowDialog(TestCollection.First().Title);";

            _testing.TestCode();

            Assert.Contains("test passed", Logger.Instance.Logs.Single());
        }

        [Fact]
        public void CodeAddsItemToUserCollection_TestCode_ItemAddedInTest()
        {
            _userCollectionLoader.CreateCollection("TestCollection", "TodoItem", "");
            _collectionNameMap.Add("TestCollection", "TodoItem");
            _testing.CurrentTextInEditor = "TestCollection.Add(new TodoItem() { Title = \"test passed\" }); c.ShowDialog(TestCollection.First().Title);";

            _testing.TestCode();

            Assert.Contains("test passed", Logger.Instance.Logs.Single());
        }

        [Fact]
        public void CodeAddsItemToUserCollection_TestCode_ItemNotAddedInRealCollection()
        {
            var realCollection = _userCollectionLoader.CreateCollection("TestCollection", "TodoItem", "");
            _collectionNameMap.Add("TestCollection", "TodoItem");
            _testing.CurrentTextInEditor = "TestCollection.Add(new TodoItem() { Title = \"test passed\" }); c.ShowDialog(TestCollection.First().Title);";

            _testing.TestCode();

            Assert.Empty(realCollection.Items);
        }
    }
}
