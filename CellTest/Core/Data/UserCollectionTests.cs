using Cell.Core.Common;
using Cell.Core.Data;
using Cell.Core.Data.Tracker;
using Cell.Core.Execution;
using Cell.Model;
using CellTest.TestUtilities;

namespace CellTest.Core.Data
{
    public class UserCollectionTests
    {
        private const string TestCollectionName = "TestCollection";
        private const string TestSortFunctionName = "TestSortFunction";
        private readonly TestDialogFactory _testDialogFactory;
        private readonly UserCollectionTracker _userCollectionTracker;
        private readonly CellPopulateManager _cellPopulateManager;
        private readonly CellTriggerManager _cellTriggerManager;
        private readonly FunctionTracker _functionTracker;
        private readonly CellTracker _cellTracker;
        private readonly UserCollection _testing;

        public UserCollectionTests()
        {
            _testDialogFactory = new TestDialogFactory();
            _functionTracker = new FunctionTracker(Logger.Null);
            _cellTracker = new CellTracker();
            _userCollectionTracker = new UserCollectionTracker(_functionTracker, _cellTracker);
            _cellTriggerManager = new CellTriggerManager(_cellTracker, _functionTracker, _userCollectionTracker, _testDialogFactory, Logger.Null);
            _cellPopulateManager = new CellPopulateManager(_cellTracker, _functionTracker, _userCollectionTracker, Logger.Null);
            _testing = _userCollectionTracker.CreateCollection(TestCollectionName, string.Empty);
        }

        [Fact]
        public void BasicLaunchTest()
        {
        }

        [Fact]
        public void NoItemInCollection_AddItem_ItemFoundInCollection()
        {
            var testItem = new UserItem();
            Assert.DoesNotContain(testItem, _testing.Items);

            _testing.Add(testItem);

            Assert.Contains(testItem, _testing.Items);
        }

        [Fact]
        public void NoItemInCollection_AddItem_ItemIdAddedToModel()
        {
            var testItem = new UserItem();
            Assert.DoesNotContain(testItem, _testing.Items);

            _testing.Add(testItem);

            Assert.Contains(testItem.ID, _testing.Model.ItemIDs);
        }

        [Fact]
        public void NoItemInCollection_AddItem_ModelItemIdsPropertyChangeNotified()
        {
            var testItem = new UserItem();
            Assert.DoesNotContain(testItem, _testing.Items);
            var modelPropertyChangedTester = new PropertyChangedTester(_testing.Model);

            _testing.Add(testItem);

            modelPropertyChangedTester.AssertPropertyChanged(nameof(_testing.Model.ItemIDs));
        }

        [Fact]
        public void OneItemInCollection_RemoveItem_ModelItemIdsRemovedFromModel()
        {
            var testItem = new UserItem();
            _testing.Add(testItem);
            Assert.Contains(testItem.ID, _testing.Model.ItemIDs);
            
            _testing.Remove(testItem);

            Assert.DoesNotContain(testItem.ID, _testing.Model.ItemIDs);
        }

        [Fact]
        public void OneItemInCollection_RemoveItem_ModelItemIdsPropertyChangeNotified()
        {
            var testItem = new UserItem();
            _testing.Add(testItem);
            var modelPropertyChangedTester = new PropertyChangedTester(_testing.Model);
            modelPropertyChangedTester.AssertPropertyNotChanged(nameof(_testing.Model.ItemIDs));

            _testing.Remove(testItem);

            modelPropertyChangedTester.AssertPropertyChanged(nameof(_testing.Model.ItemIDs));
        }

        [Fact]
        public void CollectionBasedOnOtherCollection_AddItemToBaseCollection_ItemFoundInLeafCollection()
        {
            var filteredCollection = _userCollectionTracker.CreateCollection("FilteredCollection", TestCollectionName);
            var testItem = new UserItem();
            Assert.DoesNotContain(testItem, filteredCollection.Items);

            _testing.Add(testItem);

            Assert.Contains(testItem, filteredCollection.Items);
        }

        [Fact]
        public void CollectionWithSortFunctionAndBasedOnOtherCollection_TwoItemsAddedToBaseCollection_ItemsInFilteredCollectionAreSorted()
        {
            var filteredCollection = _userCollectionTracker.CreateCollection("FilteredCollection", TestCollectionName);
            var sortFunction = _functionTracker.CreateCellFunction("object", TestSortFunctionName);
            var sortCode = $"return int.Parse({TestCollectionName}[c.Index][\"Priority\"]);";
            var collectionNames = new List<string> { TestCollectionName };
            sortFunction.SetUserFriendlyCode(sortCode, CellModel.Null, collectionNames);
            filteredCollection.Model.SortAndFilterFunctionName = TestSortFunctionName;
            var testItem1 = new UserItem();
            testItem1["Priority"] = "1";
            var testItem2 = new UserItem();
            testItem2["Priority"] = "2";
            Assert.DoesNotContain(testItem1, filteredCollection.Items);

            _testing.Add(testItem1);
            _testing.Add(testItem2);

            Assert.Equal(testItem1, filteredCollection.Items[0]);
            Assert.Equal(testItem2, filteredCollection.Items[1]);
        }

        [Fact]
        public void CollectionWithReversedSortFunctionAndBasedOnOtherCollection_TwoItemsAddedToBaseCollection_ItemsInFilteredCollectionAreSorted()
        {
            var filteredCollection = _userCollectionTracker.CreateCollection("FilteredCollection", TestCollectionName);
            var sortFunction = _functionTracker.CreateCellFunction("object", TestSortFunctionName);
            var sortCode = $"return {TestCollectionName}[c.Index][\"Priority\"];";
            var collectionNames = new List<string> { TestCollectionName };
            sortFunction.SetUserFriendlyCode(sortCode, CellModel.Null, collectionNames);
            filteredCollection.Model.SortAndFilterFunctionName = TestSortFunctionName;
            var testItem1 = new UserItem();
            testItem1["Priority"] = "1";
            var testItem2 = new UserItem();
            testItem2["Priority"] = "2";
            Assert.DoesNotContain(testItem1, filteredCollection.Items);

            _testing.Add(testItem1);
            _testing.Add(testItem2);

            Assert.Equal(testItem2, filteredCollection.Items[0]);
            Assert.Equal(testItem1, filteredCollection.Items[1]);
        }

        [Fact]
        public void CollectionWithSortFunction_ItemAdded_ItemAddedToCollection()
        {
            var sortFunction = _functionTracker.CreateCellFunction("object", TestSortFunctionName);
            var sortCode = $"return {TestCollectionName}[c.Index][\"Priority\"];";
            var collectionNames = new List<string> { TestCollectionName };
            sortFunction.SetUserFriendlyCode(sortCode, CellModel.Null, collectionNames);
            _testing.Model.SortAndFilterFunctionName = TestSortFunctionName;
            var testItem1 = new UserItem();
            testItem1["Priority"] = "1";
            Assert.DoesNotContain(testItem1, _testing.Items);

            _testing.Add(testItem1);

            Assert.Equal(testItem1, _testing.Items[0]);
        }

        [Fact]
        public void CollectionWithSortFunction_TwoItemsAdded_ItemAddedToCollectionAndSorted()
        {
            var sortFunction = _functionTracker.CreateCellFunction("object", TestSortFunctionName);
            var sortCode = $"return {TestCollectionName}[c.Index][\"Priority\"];";
            var collectionNames = new List<string> { TestCollectionName };
            sortFunction.SetUserFriendlyCode(sortCode, CellModel.Null, collectionNames);
            _testing.Model.SortAndFilterFunctionName = TestSortFunctionName;
            var testItem1 = new UserItem();
            testItem1["Priority"] = "1";
            var testItem2 = new UserItem();
            testItem2["Priority"] = "2";
            Assert.DoesNotContain(testItem1, _testing.Items);

            _testing.Add(testItem1);
            _testing.Add(testItem2);

            Assert.Equal(testItem1, _testing.Items[0]);
            Assert.Equal(testItem2, _testing.Items[1]);
        }

        [Fact]
        public void CollectionWithSortFunctionAndTwoItems_SortFunctionChanged_ItemOrderChanged()
        {
            var sortFunction = _functionTracker.CreateCellFunction("object", TestSortFunctionName);
            var sortFunction2 = _functionTracker.CreateCellFunction("object", TestSortFunctionName + "2");
            var sortCode = $"return int.Parse({TestCollectionName}[c.Index][\"Priority\"]);";
            var sortCode2 = $"return -int.Parse({TestCollectionName}[c.Index][\"Priority\"]);";
            var collectionNames = new List<string> { TestCollectionName };
            sortFunction.SetUserFriendlyCode(sortCode, CellModel.Null, collectionNames);
            sortFunction2.SetUserFriendlyCode(sortCode2, CellModel.Null, collectionNames);
            _testing.Model.SortAndFilterFunctionName = TestSortFunctionName;
            var testItem1 = new UserItem();
            testItem1["Priority"] = "1";
            var testItem2 = new UserItem();
            testItem2["Priority"] = "2";
            _testing.Add(testItem1);
            _testing.Add(testItem2);
            Assert.Equal(testItem1, _testing.Items[0]);
            Assert.Equal(testItem2, _testing.Items[1]);

            _testing.Model.SortAndFilterFunctionName = TestSortFunctionName + "2";

            Assert.Equal(testItem2, _testing.Items[0]);
            Assert.Equal(testItem1, _testing.Items[1]);
        }

        [Fact]
        public void CollectionWithSortFunctionAndTwoItems_SortPropertyChangedOnOneItem_ItemSortUpdated()
        {
            var sortFunction = _functionTracker.CreateCellFunction("object", TestSortFunctionName);
            var sortCode = $"return {TestCollectionName}[c.Index][\"Priority\"];";
            var collectionNames = new List<string> { TestCollectionName };
            sortFunction.SetUserFriendlyCode(sortCode, CellModel.Null, collectionNames);
            _testing.Model.SortAndFilterFunctionName = TestSortFunctionName;
            var testItem1 = new UserItem();
            testItem1["Priority"] = "1";
            var testItem2 = new UserItem();
            testItem2["Priority"] = "2";
            _testing.Add(testItem1);
            _testing.Add(testItem2);
            Assert.Equal(testItem1, _testing.Items[0]);
            Assert.Equal(testItem2, _testing.Items[1]);

            testItem1["Priority"] = "3";

            Assert.Equal(testItem2, _testing.Items[0]);
            Assert.Equal(testItem1, _testing.Items[1]);
        }

        [Fact]
        public void CollectionWithReversedSortFunction_TwoItemsAdded_ItemAddedToCollectionAndSorted()
        {
            var sortFunction = _functionTracker.CreateCellFunction("object", TestSortFunctionName);
            var sortCode = $"return -{TestCollectionName}[c.Index][\"Priority\"];";
            var collectionNames = new List<string> { TestCollectionName };
            sortFunction.SetUserFriendlyCode(sortCode, CellModel.Null, collectionNames);
            _testing.Model.SortAndFilterFunctionName = TestSortFunctionName;
            var testItem1 = new UserItem();
            testItem1["Priority"] = "1";
            var testItem2 = new UserItem();
            testItem2["Priority"] = "2";
            Assert.DoesNotContain(testItem1, _testing.Items);

            _testing.Add(testItem1);
            _testing.Add(testItem2);

            Assert.Equal(testItem2, _testing.Items[0]);
            Assert.Equal(testItem1, _testing.Items[1]);
        }

        [Fact]
        public void CollectionWithTwoItems_SortedCollectionCreated_ItemsAreSortedInSortedCollection()
        {
            var sortFunction = _functionTracker.CreateCellFunction("object", TestSortFunctionName);
            var sortCode = $"return {TestCollectionName}[c.Index][\"Priority\"];";
            var collectionNames = new List<string> { TestCollectionName };
            sortFunction.SetUserFriendlyCode(sortCode, CellModel.Null, collectionNames);
            _testing.Model.SortAndFilterFunctionName = TestSortFunctionName;
            var testItem1 = new UserItem();
            testItem1["Priority"] = "1";
            var testItem2 = new UserItem();
            testItem2["Priority"] = "2";
            _testing.Add(testItem1);
            _testing.Add(testItem2);

            var sortedCollection = _userCollectionTracker.CreateCollection("SortedCollection", TestCollectionName);

            Assert.Equal(testItem2, sortedCollection.Items[0]);
            Assert.Equal(testItem1, sortedCollection.Items[1]);
        }
    }
}
