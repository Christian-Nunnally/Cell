using Cell.Core.Common;
using Cell.Core.Data;
using Cell.Core.Data.Tracker;
using Cell.Core.Execution.Functions;
using Cell.Core.Execution.References;
using Cell.Core.Execution.SyntaxWalkers.UserCollections;
using Cell.Model;
using Cell.ViewModel.Application;
using Microsoft.CodeAnalysis.CSharp;
using System.ComponentModel;
using System.IO;
using System.Text.Json;

namespace Cell.Core.Persistence.Loader
{
    /// <summary>
    /// Loads and saves user collections from a project.
    /// </summary>
    public class UserCollectionLoader
    {
        private readonly CellTracker _cellTracker;
        private readonly Dictionary<string, UserCollection> _collections = [];
        private readonly PersistedDirectory _collectionsDirectory;
        private readonly Dictionary<UserCollectionModel, string> _collectionToNameMap = [];
        private readonly Dictionary<string, string> _dataTypeForCollectionMap = [];
        private readonly FunctionTracker _functionTracker;
        private readonly UserCollectionTracker _userCollectionTracker;
        private bool _shouldSaveAddedCollections = true;
        /// <summary>
        /// Creates a new instance of <see cref="UserCollectionLoader"/>.
        /// </summary>
        /// <param name="collectionsDirectory">A directory to store and load collections from.</param>
        /// <param name="userCollectionTracker">Tracker to add loaded collections to, as well as persist changes to the collections of.</param>
        /// <param name="functionTracker">The plugin function tracker used to get sort functions for collections.</param>
        /// <param name="cellTracker">The cell tracker that needs to be provided to sort functions.</param>
        public UserCollectionLoader(PersistedDirectory collectionsDirectory, UserCollectionTracker userCollectionTracker, FunctionTracker functionTracker, CellTracker cellTracker)
        {
            _collectionsDirectory = collectionsDirectory;
            _userCollectionTracker = userCollectionTracker;
            _functionTracker = functionTracker;
            _cellTracker = cellTracker;

            _userCollectionTracker.UserCollectionAdded += UserCollectionTrackerUserCollectionAdded;
            _userCollectionTracker.UserCollectionRemoved += UserCollectionTrackerUserCollectionRemoved;
        }

        /// <summary>
        /// Loads all collections from disk.
        /// </summary>
        public async Task LoadCollectionsAsync()
        {
            // do no do this on background.
            await Task.Run(LoadCollections);
        }

        /// <summary>
        /// Does the necessary work after a collection has been renamed.
        /// </summary>
        /// <param name="oldName">The old collection name.</param>
        /// <param name="newName">The new collection name.</param>
        public void ProcessCollectionRename(string oldName, string newName)
        {
            if (!_collections.TryGetValue(oldName, out var collection)) return;
            if (_collections.ContainsKey(newName)) return;
            _collections.Remove(oldName);
            _collections.Add(newName, collection);
            _collectionsDirectory.MoveDirectory(oldName, newName);

            var collectionRenamer = new CollectionReferenceRenameRewriter(oldName, newName);
            foreach (var function in _functionTracker.CellFunctions)
            {
                if (function.CollectionDependencies.OfType<ConstantCollectionReference>().Select(x => x.ConstantCollectionName).Contains(oldName))
                {
                    function.Model.Code = collectionRenamer.Visit(CSharpSyntaxTree.ParseText(function.Model.Code).GetRoot())?.ToFullString() ?? "";
                }
            }
        }

        private void DeleteItem(string collectionName, string idToRemove)
        {
            var path = Path.Combine(collectionName, "Items", idToRemove);
            _collectionsDirectory.DeleteFile(path);
        }

        private void LoadCollection(PersistedDirectory collectionDirectory)
        {
            var text = collectionDirectory.LoadFile("collection") ?? throw new CellError($"Error while loading 'collection' file for {collectionDirectory.GetFullPath()}");
            var model = JsonSerializer.Deserialize<UserCollectionModel>(text) ?? throw new CellError($"Error while loading {collectionDirectory.GetFullPath()}");
            var sortContext = new Context(_cellTracker, _userCollectionTracker, new DialogFactory(), CellModel.Null);
            var collection = new UserCollection(model, _functionTracker, sortContext);
            var itemsDirectory = collectionDirectory.FromDirectory("Items");

            collection.IsSortOnInsertEnabled = false;
            foreach (var itemId in model.ItemIDs)
            {
                collection.Add(new UnloadedItem(itemsDirectory, itemId).Load());
            }
            collection.IsSortOnInsertEnabled = true;

            _shouldSaveAddedCollections = false;
            _userCollectionTracker.StartTrackingCollection(collection);
            _shouldSaveAddedCollections = true;
        }

        private void LoadCollections()
        {
            foreach (var directory in _collectionsDirectory.GetDirectories())
            {
                LoadCollection(_collectionsDirectory.FromDirectory(directory));
            }
        }

        private UserItem LoadItem(string path)
        {
            var text = _collectionsDirectory.LoadFile(path) ?? throw new CellError($"Failed to load {path} because it is not a valid {nameof(UserItem)}");
            return JsonSerializer.Deserialize<UserItem>(text) ?? throw new CellError($"Failed to load {path} because it is not a valid {nameof(UserItem)}. File contents = {text}");
        }

        private void SaveCollection(UserCollection collection)
        {
            SaveCollectionSettings(collection.Model);
            foreach (var item in collection.Items) SaveItem(collection.Model.Name, item.ID, item);
        }

        private void SaveCollectionSettings(UserCollectionModel model)
        {
            var path = Path.Combine(model.Name, "collection");
            var serializedModel = JsonSerializer.Serialize(model);
            _collectionsDirectory.SaveFile(path, serializedModel);
        }

        private void SaveItem(string collectionName, string id, UserItem model)
        {
            var path = Path.Combine(collectionName, "Items", id);
            var serializedModel = JsonSerializer.Serialize(model);
            _collectionsDirectory.SaveFile(path, serializedModel);
        }

        private void UserCollectionItemAdded(UserCollection collection, UserItem model)
        {
            if (collection.IsFilteredView) return;
            SaveItem(collection.Model.Name, model.ID, model);
        }

        private void UserCollectionItemChanged(UserCollection collection, UserItem model)
        {
            if (collection.IsFilteredView) return;
            SaveItem(collection.Model.Name, model.ID, model);
        }

        private void UserCollectionItemRemoved(UserCollection collection, UserItem model)
        {
            if (collection.IsFilteredView) return;
            DeleteItem(collection.Model.Name, model.ID);
        }

        private void UserCollectionModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not UserCollectionModel model) return;
            SaveCollectionSettings(model);
            if (e.PropertyName == nameof(UserCollectionModel.Name))
            {
                var oldName = _collectionToNameMap[model];
                _collectionsDirectory.MoveDirectory(oldName, model.Name);
                _collectionToNameMap[model] = model.Name;
            }
        }

        private void UserCollectionTrackerUserCollectionAdded(UserCollection collection)
        {
            collection.ItemAdded += UserCollectionItemAdded;
            collection.ItemRemoved += UserCollectionItemRemoved;
            collection.ItemPropertyChanged += UserCollectionItemChanged;
            collection.Model.PropertyChanged += UserCollectionModelPropertyChanged;
            _collectionToNameMap.Add(collection.Model, collection.Model.Name);
            if (_shouldSaveAddedCollections) SaveCollection(collection);
        }

        private void UserCollectionTrackerUserCollectionRemoved(UserCollection collection)
        {
            collection.ItemAdded -= UserCollectionItemAdded;
            collection.ItemRemoved -= UserCollectionItemRemoved;
            collection.ItemPropertyChanged -= UserCollectionItemChanged;
            collection.Model.PropertyChanged -= UserCollectionModelPropertyChanged;
            _collectionToNameMap.Remove(collection.Model);
            _collectionsDirectory.DeleteDirectory(collection.Model.Name);
        }
    }
}
