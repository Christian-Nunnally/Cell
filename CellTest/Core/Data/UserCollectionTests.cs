﻿using Cell.Core.Data;
using Cell.Core.Execution;
using Cell.Model;
using Cell.Model.Plugin;
using Cell.Core.Persistence;
using CellTest.TestUtilities;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace CellTest.Core.Data
{
    public class UserCollectionTests
    {
        private const string TestCollectionName = "TestCollection";
        private const string TestSortFunctionName = "TestSortFunction";
        private static DictionaryFileIO _testFileIO;
        private PersistedDirectory _persistedDirectory;
        private UserCollectionLoader _userCollectionLoader;
        private CellPopulateManager _cellPopulateManager;
        private CellLoader _cellLoader;
        private CellTriggerManager _cellTriggerManager;
        private PluginFunctionLoader _pluginFunctionLoader;
        private CellTracker _cellTracker;

        private UserCollection CreateTestInstance()
        {
            _testFileIO = new DictionaryFileIO();
            _persistedDirectory = new PersistedDirectory("", _testFileIO);
            _pluginFunctionLoader = new PluginFunctionLoader(_persistedDirectory);
            _cellLoader = new CellLoader(_persistedDirectory);
            _cellTracker = new CellTracker(_cellLoader);
            _userCollectionLoader = new UserCollectionLoader(_persistedDirectory, _pluginFunctionLoader, _cellTracker);
            _cellTriggerManager = new CellTriggerManager(_cellTracker, _pluginFunctionLoader, _userCollectionLoader);
            _cellPopulateManager = new CellPopulateManager(_cellTracker, _pluginFunctionLoader, _userCollectionLoader);
            return _userCollectionLoader.CreateCollection(TestCollectionName, nameof(TodoItem), string.Empty);
        }

        [Fact]
        public void BasicLaunchTest()
        {
            var _ = CreateTestInstance();
        }

        [Fact]
        public void NoItemInCollection_AddItem_ItemFoundInCollection()
        {
            var testing = CreateTestInstance();
            var testItem = new TodoItem();
            Assert.DoesNotContain(testItem, testing.Items);

            testing.Add(testItem);

            Assert.Contains(testItem, testing.Items);
        }

        [Fact]
        public void CollectionBasedOnOtherCollection_AddItemToBaseCollection_ItemFoundInLeafCollection()
        {
            var testing = CreateTestInstance();
            var filteredCollection = _userCollectionLoader.CreateCollection("FilteredCollection", nameof(TodoItem), TestCollectionName);
            var testItem = new TodoItem();
            Assert.DoesNotContain(testItem, filteredCollection.Items);

            testing.Add(testItem);

            Assert.Contains(testItem, filteredCollection.Items);
        }

        [Fact]
        public void CollectionWithSortFunctionAndBasedOnOtherCollection_TwoItemsAddedToBaseCollection_ItemsInFilteredCollectionAreSorted()
        {
            var testing = CreateTestInstance();
            var filteredCollection = _userCollectionLoader.CreateCollection("FilteredCollection", nameof(TodoItem), TestCollectionName);
            var sortFunction = _pluginFunctionLoader.CreateCellFunction("object", TestSortFunctionName);
            var sortCode = $"return -{TestCollectionName}[c.SortIndex].Priority;";
            var collectionNameToDataTypeMap = new Dictionary<string, string> { { TestCollectionName, nameof(TodoItem) } };
            sortFunction.SetUserFriendlyCode(sortCode, CellModel.Null, collectionNameToDataTypeMap);
            filteredCollection.Model.SortAndFilterFunctionName = TestSortFunctionName;
            var testItem1 = new TodoItem() { Priority = 1 };
            var testItem2 = new TodoItem() { Priority = 2 };
            Assert.DoesNotContain(testItem1, filteredCollection.Items);

            testing.Add(testItem1);
            testing.Add(testItem2);

            Assert.Equal(testItem1, filteredCollection.Items[0]);
            Assert.Equal(testItem2, filteredCollection.Items[1]);
        }

        [Fact]
        public void CollectionWithReversedSortFunctionAndBasedOnOtherCollection_TwoItemsAddedToBaseCollection_ItemsInFilteredCollectionAreSorted()
        {
            var testing = CreateTestInstance();
            var filteredCollection = _userCollectionLoader.CreateCollection("FilteredCollection", nameof(TodoItem), TestCollectionName);
            var sortFunction = _pluginFunctionLoader.CreateCellFunction("object", TestSortFunctionName);
            var sortCode = $"return {TestCollectionName}[c.SortIndex].Priority;";
            var collectionNameToDataTypeMap = new Dictionary<string, string> { { TestCollectionName, nameof(TodoItem) } };
            sortFunction.SetUserFriendlyCode(sortCode, CellModel.Null, collectionNameToDataTypeMap);
            filteredCollection.Model.SortAndFilterFunctionName = TestSortFunctionName;
            var testItem1 = new TodoItem() { Priority = 1 };
            var testItem2 = new TodoItem() { Priority = 2 };
            Assert.DoesNotContain(testItem1, filteredCollection.Items);

            testing.Add(testItem1);
            testing.Add(testItem2);

            Assert.Equal(testItem2, filteredCollection.Items[0]);
            Assert.Equal(testItem1, filteredCollection.Items[1]);
        }

        [Fact]
        public void CollectionWithSortFunction_ItemAdded_ItemAddedToCollection()
        {
            var testing = CreateTestInstance();
            var sortFunction = _pluginFunctionLoader.CreateCellFunction("object", TestSortFunctionName);
            var sortCode = $"return {TestCollectionName}[c.SortIndex].Priority;";
            var collectionNameToDataTypeMap = new Dictionary<string, string> { { TestCollectionName, nameof(TodoItem) } };
            sortFunction.SetUserFriendlyCode(sortCode, CellModel.Null, collectionNameToDataTypeMap);
            testing.Model.SortAndFilterFunctionName = TestSortFunctionName;
            var testItem1 = new TodoItem() { Priority = 1 };
            Assert.DoesNotContain(testItem1, testing.Items);

            testing.Add(testItem1);

            Assert.Equal(testItem1, testing.Items[0]);
        }

        [Fact]
        public void CollectionWithSortFunction_TwoItemsAdded_ItemAddedToCollectionAndSorted()
        {
            var testing = CreateTestInstance();
            var sortFunction = _pluginFunctionLoader.CreateCellFunction("object", TestSortFunctionName);
            var sortCode = $"return {TestCollectionName}[c.SortIndex].Priority;";
            var collectionNameToDataTypeMap = new Dictionary<string, string> { { TestCollectionName, nameof(TodoItem) } };
            sortFunction.SetUserFriendlyCode(sortCode, CellModel.Null, collectionNameToDataTypeMap);
            testing.Model.SortAndFilterFunctionName = TestSortFunctionName;
            var testItem1 = new TodoItem() { Priority = 1 };
            var testItem2 = new TodoItem() { Priority = 2 };
            Assert.DoesNotContain(testItem1, testing.Items);

            testing.Add(testItem1);
            testing.Add(testItem2);

            Assert.Equal(testItem1, testing.Items[0]);
            Assert.Equal(testItem2, testing.Items[1]);
        }

        [Fact]
        public void CollectionWithSortFunctionAndTwoItems_SortFunctionChanged_ItemOrderChanged()
        {
            var testing = CreateTestInstance();
            var sortFunction = _pluginFunctionLoader.CreateCellFunction("object", TestSortFunctionName);
            var sortFunction2 = _pluginFunctionLoader.CreateCellFunction("object", TestSortFunctionName + "2");
            var sortCode = $"return {TestCollectionName}[c.SortIndex].Priority;";
            var sortCode2 = $"return -{TestCollectionName}[c.SortIndex].Priority;";
            var collectionNameToDataTypeMap = new Dictionary<string, string> { { TestCollectionName, nameof(TodoItem) } };
            sortFunction.SetUserFriendlyCode(sortCode, CellModel.Null, collectionNameToDataTypeMap);
            sortFunction2.SetUserFriendlyCode(sortCode2, CellModel.Null, collectionNameToDataTypeMap);
            testing.Model.SortAndFilterFunctionName = TestSortFunctionName;
            var testItem1 = new TodoItem() { Priority = 1 };
            var testItem2 = new TodoItem() { Priority = 2 };
            testing.Add(testItem1);
            testing.Add(testItem2);
            Assert.Equal(testItem1, testing.Items[0]);
            Assert.Equal(testItem2, testing.Items[1]);

            testing.Model.SortAndFilterFunctionName = TestSortFunctionName + "2";

            Assert.Equal(testItem2, testing.Items[0]);
            Assert.Equal(testItem1, testing.Items[1]);
        }

        [Fact]
        public void CollectionWithSortFunctionAndTwoItems_SortPropertyChangedOnOneItem_ItemSortUpdated()
        {
            var testing = CreateTestInstance();
            var sortFunction = _pluginFunctionLoader.CreateCellFunction("object", TestSortFunctionName);
            var sortCode = $"return {TestCollectionName}[c.SortIndex].Priority;";
            var collectionNameToDataTypeMap = new Dictionary<string, string> { { TestCollectionName, nameof(TodoItem) } };
            sortFunction.SetUserFriendlyCode(sortCode, CellModel.Null, collectionNameToDataTypeMap);
            testing.Model.SortAndFilterFunctionName = TestSortFunctionName;
            var testItem1 = new TodoItem() { Priority = 1 };
            var testItem2 = new TodoItem() { Priority = 2 };
            testing.Add(testItem1);
            testing.Add(testItem2);
            Assert.Equal(testItem1, testing.Items[0]);
            Assert.Equal(testItem2, testing.Items[1]);

            testItem1.Priority = 3;

            Assert.Equal(testItem2, testing.Items[0]);
            Assert.Equal(testItem1, testing.Items[1]);
        }

        [Fact]
        public void CollectionWithReversedSortFunction_TwoItemsAdded_ItemAddedToCollectionAndSorted()
        {
            var testing = CreateTestInstance();
            var sortFunction = _pluginFunctionLoader.CreateCellFunction("object", TestSortFunctionName);
            var sortCode = $"return -{TestCollectionName}[c.SortIndex].Priority;";
            var collectionNameToDataTypeMap = new Dictionary<string, string> { { TestCollectionName, nameof(TodoItem) } };
            sortFunction.SetUserFriendlyCode(sortCode, CellModel.Null, collectionNameToDataTypeMap);
            testing.Model.SortAndFilterFunctionName = TestSortFunctionName;
            var testItem1 = new TodoItem() { Priority = 1 };
            var testItem2 = new TodoItem() { Priority = 2 };
            Assert.DoesNotContain(testItem1, testing.Items);

            testing.Add(testItem1);
            testing.Add(testItem2);

            Assert.Equal(testItem2, testing.Items[0]);
            Assert.Equal(testItem1, testing.Items[1]);
        }

        [Fact]
        public void CollectionWithTwoItems_SortedCollectionCreated_ItemsAreSortedInSortedCollection()
        {
            var testing = CreateTestInstance();
            var sortFunction = _pluginFunctionLoader.CreateCellFunction("object", TestSortFunctionName);
            var sortCode = $"return {TestCollectionName}[c.SortIndex].Priority;";
            var collectionNameToDataTypeMap = new Dictionary<string, string> { { TestCollectionName, nameof(TodoItem) } };
            sortFunction.SetUserFriendlyCode(sortCode, CellModel.Null, collectionNameToDataTypeMap);
            testing.Model.SortAndFilterFunctionName = TestSortFunctionName;
            var testItem1 = new TodoItem() { Priority = 1 };
            var testItem2 = new TodoItem() { Priority = 2 };
            testing.Add(testItem1);
            testing.Add(testItem2);

            var sortedCollection = _userCollectionLoader.CreateCollection("SortedCollection", nameof(TodoItem), TestCollectionName);

            Assert.Equal(testItem2, sortedCollection.Items[0]);
            Assert.Equal(testItem1, sortedCollection.Items[1]);
        }
    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.