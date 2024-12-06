using Cell.Core.Execution;
using Cell.Model;
using Cell.Core.Persistence;
using CellTest.TestUtilities;
using Cell.Core.Persistence.Loader;
using Cell.Core.Data.Tracker;
using Cell.Core.Common;

namespace CellTest.Core.Execution
{
    public class CellPopulateManagerTests
    {
        private readonly DictionaryFileIO _testFileIO;
        private readonly PersistedDirectory _persistedDirectory;
        private readonly CellLoader _cellLoader;
        private readonly CellTracker _cellTracker;
        private readonly FunctionTracker _functionTracker;
        private readonly UserCollectionTracker _userCollectionTracker;
        private readonly CellPopulateManager testing;

        public CellPopulateManagerTests()
        {
            _testFileIO = new DictionaryFileIO();
            _persistedDirectory = new PersistedDirectory("", _testFileIO);
            _cellTracker = new CellTracker();
            _cellLoader = new CellLoader(_persistedDirectory, _cellTracker);
            _functionTracker = new FunctionTracker(Logger.Null);
            _userCollectionTracker = new UserCollectionTracker(_functionTracker, _cellTracker);
            testing = new CellPopulateManager(_cellTracker, _functionTracker, _userCollectionTracker, Logger.Null);
        }

        [Fact]
        public void BasicLaunchTest()
        {
        }

        [Fact]
        public void LabelCellWithPopulateFunction_CellTextChanged_PopulateFunctionRunsToSetTheTextToItsResult()
        {
            var cell = CellModelFactory.Create(0, 0, CellType.Label, "Sheet1", _cellTracker);
            var function = _functionTracker.CreateCellFunction("object", "testHelloWorld", "return \"Hello world\";");
            cell.PopulateFunctionName = function.Model.Name;

            cell.Text += "1";

            Assert.Equal("Hello world", cell.Text);
        }

        [Fact]
        public void A1CellWithPopulateFunctionDependingOnA2_CellTextChanged_PopulateFunctionRunsToSetTheTextToItsResult()
        {
            var a1 = CellModelFactory.Create(1, 1, CellType.Label, "Sheet1", _cellTracker);
            var a2 = CellModelFactory.Create(2, 1, CellType.Label, "Sheet1", _cellTracker);
            var function = _functionTracker.CreateCellFunction("object", "testHelloWorld");
            function.SetUserFriendlyCode("return A2.Text;", a1, []);
            a1.PopulateFunctionName = function.Model.Name;

            a2.Text += "HelloWorld";

            Assert.Equal("HelloWorld", a1.Text);
        }

        [Fact]
        public void CellWithPopulateFunctionDependingOnCollection_ItemAddedToCollection_PopulateFunctionRunsToSetTheTextToItsResult()
        {
            var cell = CellModelFactory.Create(0, 0, CellType.Label, "Sheet1", _cellTracker);
            var collection = _userCollectionTracker.CreateCollection("testList");
            var function = _functionTracker.CreateCellFunction("object", "testHelloWorld");
            var collectionNames = new List<string> { "testList" };
            function.SetUserFriendlyCode("return testList.Count();", cell, collectionNames);
            cell.PopulateFunctionName = function.Model.Name;

            collection.Add(new UserItem());

            Assert.Equal("1", cell.Text);
        }

        [Fact]
        public void A1PopulateFunctionReferencesCollectionNamedA2Text_A2SetToCollectionName_CollectionReferenceRecognizedByPopulateManagerForA1()
        {
            var a1 = CellModelFactory.Create(1, 1, CellType.Label, "Sheet1", _cellTracker);
            var a2 = CellModelFactory.Create(2, 1, CellType.Label, "Sheet1", _cellTracker);
            var collection = _userCollectionTracker.CreateCollection("testList");
            var function = _functionTracker.CreateCellFunction("object", "testHelloWorld");
            var collectionNames = new List<string> { "testList" };
            function.SetUserFriendlyCode("return c.GetUserList(A2.Text).Count();", a1, collectionNames);
            a1.PopulateFunctionName = function.Model.Name;

            a2.Text = "testList";

            Assert.Contains("testList", testing.GetAllCollectionSubscriptions(a1));
        }

        [Fact]
        public void A1PopulateFunctionReferencesCollectionNamedA2Text_A2SetToNewCollectionName_CollectionReferenceRecognizedByPopulateManagerForA1()
        {
            var a1 = CellModelFactory.Create(1, 1, CellType.Label, "Sheet1", _cellTracker);
            var a2 = CellModelFactory.Create(2, 1, CellType.Label, "Sheet1", _cellTracker);
            var collection = _userCollectionTracker.CreateCollection("testList");
            var collection2 = _userCollectionTracker.CreateCollection("testList2");
            var function = _functionTracker.CreateCellFunction("object", "testHelloWorld");
            var collectionNames = new List<string> { "testList" };
            function.SetUserFriendlyCode("return c.GetUserList(A2.Text).Count();", a1, collectionNames);
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
            var a1 = CellModelFactory.Create(1, 1, CellType.Label, "Sheet1", _cellTracker);
            var a2 = CellModelFactory.Create(2, 1, CellType.Label, "Sheet1", _cellTracker);
            var collection = _userCollectionTracker.CreateCollection("testList");
            var function = _functionTracker.CreateCellFunction("object", "testHelloWorld");
            var collectionNames = new List<string> { "testList" };
            function.SetUserFriendlyCode("return c.GetUserList(A2.Text).Count();", a1, collectionNames);
            a1.PopulateFunctionName = function.Model.Name;
            a2.Text = "testList";

            _userCollectionTracker.GetCollection("testList")!.Add(new UserItem());

            Assert.Equal("1", a1.Text);
        }
    }
}
