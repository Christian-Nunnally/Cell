using Cell.Core.Data;
using Cell.Core.Data.Tracker;
using System.Collections.ObjectModel;
using System.Windows;

namespace Cell.ViewModel.ToolWindow
{
    /// <summary>
    /// A tool window that allows the user to create a new user collection.
    /// </summary>
    public class CreateCollectionWindowViewModel : ToolWindowViewModel
    {
        private readonly ObservableCollection<UserCollection> _collections;
        private readonly UserCollectionTracker _userCollectionTracker;
        private bool _isBaseOnCheckBoxChecked;
        /// <summary>
        /// Creates a new instance of the <see cref="CreateCollectionWindowViewModel"/>.
        /// </summary>
        /// <param name="userCollectionTracker">The user collection loader to add the collection to.</param>
        public CreateCollectionWindowViewModel(UserCollectionTracker userCollectionTracker)
        {
            _userCollectionTracker = userCollectionTracker;
            _collections = _userCollectionTracker.UserCollections;
            CollectionBaseOptions = new ObservableCollection<string>(_collections.Select(x => x.Model.Name))
            {
                "---"
            };
        }

        /// <summary>
        /// Gets the names of all the collections that are options as a base collection of the new collection.
        /// </summary>
        public ObservableCollection<string> CollectionBaseOptions { get; set; }

        /// <summary>
        /// Gets the visibility of the collection base setting.
        /// </summary>
        public Visibility CollectionBaseSettingVisibility { get; private set; } = Visibility.Collapsed;

        /// <summary>
        /// Gets the visibility of the collection type setting.
        /// </summary>
        public Visibility CollectionTypeSettingVisibility { get; private set; } = Visibility.Visible;

        /// <summary>
        /// Gets the default height of this tool window when it is shown.
        /// </summary>
        public override double DefaultHeight => 250;

        /// <summary>
        /// Gets the default width of this tool window when it is shown.
        /// </summary>
        public override double DefaultWidth => 350;

        /// <summary>
        /// Gets or sets a value indicating whether the new collection should be based on another collection.
        /// </summary>
        public bool IsBaseOnCheckBoxChecked
        {
            get => _isBaseOnCheckBoxChecked; set
            {
                _isBaseOnCheckBoxChecked = value;
                CollectionBaseSettingVisibility = _isBaseOnCheckBoxChecked ? Visibility.Visible : Visibility.Collapsed;
                CollectionTypeSettingVisibility = _isBaseOnCheckBoxChecked ? Visibility.Collapsed : Visibility.Visible;
                NotifyPropertyChanged(nameof(CollectionBaseSettingVisibility));
                NotifyPropertyChanged(nameof(CollectionTypeSettingVisibility));
            }
        }

        /// <summary>
        /// The name of the collection the nwe collection should be a projection of, or empty if the new collection is not based on another collection.
        /// </summary>
        public string NewCollectionBaseName { get; set; } = string.Empty;

        /// <summary>
        /// The name to give the new collection.
        /// </summary>
        public string NewCollectionName { get; set; } = string.Empty;

        /// <summary>
        /// Gets the string displayed in top bar of this tool window.
        /// </summary>
        public override string ToolWindowTitle => "New collection";

        /// <summary>
        /// Creates a new collection with the current settings.
        /// </summary>
        public void AddCurrentCollection()
        {
            var collectionName = NewCollectionName;
            if (string.IsNullOrEmpty(collectionName)) return;
            if (_collections.Any(x => x.Model.Name == collectionName)) return;

            if (IsBaseOnCheckBoxChecked)
            {
                var basedOnCollection = NewCollectionBaseName;
                if (string.IsNullOrEmpty(basedOnCollection)) return;

                var baseCollection = _collections.FirstOrDefault(x => x.Model.Name == basedOnCollection);
                if (baseCollection is null) return;

                var userCollectionModel = 
                _userCollectionTracker.CreateCollection(collectionName, baseCollection.Model.Name);
            }
            else
            {
                _userCollectionTracker.CreateCollection(collectionName);
            }

            RequestClose?.Invoke();
        }
    }
}
