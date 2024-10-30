using Cell.Core.Common;
using Cell.Core.Data;
using Cell.Core.Data.Tracker;
using Cell.Core.Execution.References;
using Cell.ViewModel.Application;

namespace Cell.ViewModel.Data
{
    /// <summary>
    /// A view model for a user collection that is displayed in a list.
    /// </summary>
    public class UserCollectionListItemViewModel : PropertyChangedBase
    {
        public UserCollectionListItemViewModel(UserCollection underlyingCollection, FunctionTracker functionTracker, UserCollectionTracker userCollectionTracker)
        {
            Collection = underlyingCollection;
            _functionTracker = functionTracker;
            _userCollectionTracker = userCollectionTracker;
        }

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
                var usagesWithinFunctions = _functionTracker.CellFunctions.Sum(x => x.CollectionDependencies.OfType<ConstantCollectionReference>().Count(x => x.ConstantCollectionName == Collection.Model.Name));
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
                ApplicationViewModel.Instance.DialogFactory.ShowYesNo("Change Collection Name", $"Do you want to change the collection name from '{oldName}' to '{newName}'?", () =>
                {
                    _userCollectionTracker.ProcessCollectionRename(oldName, newName);
                    Collection.Model.Name = newName;
                    NotifyPropertyChanged(nameof(Name));
                });
            }
            get
            {
                return Collection.Model.Name;
            }
        }
    }
}
