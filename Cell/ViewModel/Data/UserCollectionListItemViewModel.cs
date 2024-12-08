using Cell.Core.Common;
using Cell.Core.Data;
using Cell.Core.Data.Tracker;
using Cell.Core.Execution.References;
using Cell.Core.Execution.SyntaxWalkers.UserCollections;
using Cell.ViewModel.Application;
using Microsoft.CodeAnalysis.CSharp;

namespace Cell.ViewModel.Data
{
    /// <summary>
    /// A view model for a user collection that is displayed in a list.
    /// </summary>
    public class UserCollectionListItemViewModel : PropertyChangedBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="UserCollectionListItemViewModel"/>.
        /// </summary>
        /// <param name="underlyingCollection">The collection this view model represents.</param>
        /// <param name="functionTracker">Used to get information about what functions are using this collection.</param>
        /// <param name="userCollectionTracker">Used to find collections that might relate ot this collection, like base collections.</param>
        public UserCollectionListItemViewModel(UserCollection underlyingCollection, FunctionTracker functionTracker, UserCollectionTracker userCollectionTracker)
        {
            Collection = underlyingCollection;
            _functionTracker = functionTracker;
            _userCollectionTracker = userCollectionTracker;
        }


        /// <summary>
        /// The underlying collection this view model represents.
        /// </summary>
        public UserCollection Collection { get; private set; }

        private readonly FunctionTracker _functionTracker;
        private readonly UserCollectionTracker _userCollectionTracker;

        /// <summary>
        /// Gets the number of times this collection is used in other collections or functions.
        /// </summary>
        public int UsageCount
        {
            get
            {
                var usagesWithinFunctions = _functionTracker.Functions.Sum(x => x.CollectionDependencies.OfType<ConstantCollectionReference>().Count(x => x.ConstantCollectionName == Collection.Model.Name));
                var collectionsUsingThisCollectionAsABase = _userCollectionTracker.UserCollections.Count(x => x.Model.BasedOnCollectionName == Collection.Model.Name);
                return usagesWithinFunctions + collectionsUsingThisCollectionAsABase;
            }
        }

        /// <summary>
        /// Gets or renames the name of this collection.
        /// </summary>
        public string Name
        {
            set
            {
                if (Collection.Model.Name == value) return;
                var oldName = Collection.Model.Name;
                var newName = value;
                ApplicationViewModel.Instance.DialogFactory?.ShowYesNo("Change Collection Name", $"Do you want to change the collection name from '{oldName}' to '{newName}'?", () =>
                {
                    Collection.Model.Name = newName;
                    NotifyPropertyChanged(nameof(Name));

                    var collectionRenamer = new CollectionReferenceRenameRewriter(oldName, newName);
                    foreach (var function in _functionTracker.Functions)
                    {
                        if (function.CollectionDependencies.OfType<ConstantCollectionReference>().Select(x => x.ConstantCollectionName).Contains(oldName))
                        {
                            function.Model.Code = collectionRenamer.Visit(CSharpSyntaxTree.ParseText(function.Model.Code).GetRoot())?.ToFullString() ?? "";
                        }
                    }
                });
            }
            get
            {
                return Collection.Model.Name;
            }
        }
    }
}
