using Cell.Data;
using Cell.Execution;
using Cell.Model;
using Cell.Model.Plugin;
using Cell.Persistence;
using CellTest.TestUtilities;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace CellTest
{
    public class CellPopulateManagerTests
    {
        private DictionaryFileIO _testFileIO;
        private PersistedDirectory _persistedDirectory;
        private CellLoader _cellLoader;
        private CellTracker _cellTracker;
        private PluginFunctionLoader _pluginFunctionLoader;
        private UserCollectionLoader _userCollectionLoader;

        private CellPopulateManager CreateInstance()
        {
            _testFileIO = new DictionaryFileIO();
            _persistedDirectory = new PersistedDirectory("", _testFileIO);
            _cellLoader = new CellLoader(_persistedDirectory);
            _cellTracker = new CellTracker(_cellLoader);
            _pluginFunctionLoader = new PluginFunctionLoader(_persistedDirectory);
            _userCollectionLoader = new UserCollectionLoader(_persistedDirectory, _pluginFunctionLoader, _cellTracker);
            return new CellPopulateManager(_cellTracker, _pluginFunctionLoader, _userCollectionLoader);
        }

        [Fact]
        public void BasicLaunchTest()
        {
            var _ = CreateInstance();
        }

        [Fact]
        public void CellWithPopulateFunction_CellTextChanged_PopulateFunctionRunsToSetTheTextToItsResult()
        {
            var _ = CreateInstance();
            var cell = CellModelFactory.Create(0, 0, CellType.Label, "Sheet1", _cellTracker);
            var function = _pluginFunctionLoader.CreateCellFunction("object", "testHelloWorld", "return \"Hello world\";");
            cell.PopulateFunctionName = function.Model.Name;

            cell.Text += "1";

            Assert.Equal("Hello world", cell.Text);
        }

        [Fact]
        public void A1CellWithPopulateFunctionDependingOnA2_CellTextChanged_PopulateFunctionRunsToSetTheTextToItsResult()
        {
            var _ = CreateInstance();
            var a1 = CellModelFactory.Create(1, 1, CellType.Label, "Sheet1", _cellTracker);
            var a2 = CellModelFactory.Create(2, 1, CellType.Label, "Sheet1", _cellTracker);
            var function = _pluginFunctionLoader.CreateCellFunction("object", "testHelloWorld");
            function.SetUserFriendlyCode("return A2.Text;", a1, new Dictionary<string, string>());
            a1.PopulateFunctionName = function.Model.Name;

            a2.Text += "HelloWorld";

            Assert.Equal("HelloWorld", a1.Text);
        }

        [Fact]
        public void CellWithPopulateFunctionDependingOnCollection_ItemAddedToCollection_PopulateFunctionRunsToSetTheTextToItsResult()
        {
            var _ = CreateInstance();
            var cell = CellModelFactory.Create(0, 0, CellType.Label, "Sheet1", _cellTracker);
            var collection = _userCollectionLoader.CreateCollection("testList", nameof(TodoItem));
            var function = _pluginFunctionLoader.CreateCellFunction("object", "testHelloWorld");
            var collectionNameToDataTypeMap = new Dictionary<string, string> { { "testList", nameof(TodoItem) } };
            function.SetUserFriendlyCode("return testList.Count();", cell, collectionNameToDataTypeMap);
            cell.PopulateFunctionName = function.Model.Name;

            collection.Add(new TodoItem());

            Assert.Equal("1", cell.Text);
        }

        [Fact]
        public void A1PopulateFunctionReferencesCollectionNamedA2Text_A2SetToCollectionName_CollectionReferenceRecognizedByPopulateManagerForA1()
        {
            var testing = CreateInstance();
            var a1 = CellModelFactory.Create(1, 1, CellType.Label, "Sheet1", _cellTracker);
            var a2 = CellModelFactory.Create(2, 1, CellType.Label, "Sheet1", _cellTracker);
            var collection = _userCollectionLoader.CreateCollection("testList", "TodoItem");
            var function = _pluginFunctionLoader.CreateCellFunction("object", "testHelloWorld");
            var collectionNameToDataTypeMap = new Dictionary<string, string> { { "testList", nameof(TodoItem) } };
            function.SetUserFriendlyCode("return c.GetUserList<TodoItem>(A2.Text).Count();", a1, collectionNameToDataTypeMap);
            a1.PopulateFunctionName = function.Model.Name;

            a2.Text = "testList";

            Assert.Contains("testList", testing.GetAllCollectionSubscriptions(a1));
        }

        [Fact]
        public void A1PopulateFunctionReferencesCollectionNamedA2Text_A2SetToNewCollectionName_CollectionReferenceRecognizedByPopulateManagerForA1()
        {
            var testing = CreateInstance();
            var a1 = CellModelFactory.Create(1, 1, CellType.Label, "Sheet1", _cellTracker);
            var a2 = CellModelFactory.Create(2, 1, CellType.Label, "Sheet1", _cellTracker);
            var collection = _userCollectionLoader.CreateCollection("testList", nameof(TodoItem));
            var collection2 = _userCollectionLoader.CreateCollection("testList2", nameof(TodoItem));
            var function = _pluginFunctionLoader.CreateCellFunction("object", "testHelloWorld");
            var collectionNameToDataTypeMap = new Dictionary<string, string> { { "testList", nameof(TodoItem) } };
            function.SetUserFriendlyCode("return c.GetUserList<TodoItem>(A2.Text).Count();", a1, collectionNameToDataTypeMap);
            a1.PopulateFunctionName = function.Model.Name;
            a2.Text = "testList";
            Assert.Contains("testList", testing.GetAllCollectionSubscriptions(a1));

            a2.Text = "testList2";
            Assert.Contains("testList2", testing.GetAllCollectionSubscriptions(a1));
            Assert.DoesNotContain("testList", testing.GetAllCollectionSubscriptions(a1));
        }

        [Fact]
        public void A1PopulateFunctionReferencesCollectionNamedA2Text_ItemAddedToCollection_PopulateFunctionRunsToSetTheTextToItsResult()
        {
            var _ = CreateInstance();
            var a1 = CellModelFactory.Create(1, 1, CellType.Label, "Sheet1", _cellTracker);
            var a2 = CellModelFactory.Create(2, 1, CellType.Label, "Sheet1", _cellTracker);
            var collection = _userCollectionLoader.CreateCollection("testList", nameof(TodoItem));
            var function = _pluginFunctionLoader.CreateCellFunction("object", "testHelloWorld");
            var collectionNameToDataTypeMap = new Dictionary<string, string> { { "testList", nameof(TodoItem) } };
            function.SetUserFriendlyCode("return c.GetUserList<TodoItem>(A2.Text).Count();", a1, collectionNameToDataTypeMap);
            a1.PopulateFunctionName = function.Model.Name;
            a2.Text = "testList";

            _userCollectionLoader.GetCollection("testList")!.Add(new TodoItem());

            Assert.Equal("1", a1.Text);
        }
    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.