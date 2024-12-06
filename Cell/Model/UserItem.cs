using Cell.Core.Common;

namespace Cell.Model
{
    /// <summary>
    /// A model for a user created item, basically a dictionary of properties with a unique ID.
    /// </summary>
    public class UserItem : PropertyChangedBase
    {
        private string _id = Utilities.GenerateUnqiueId(12);
        private UserItemDates? _dates;
        private UserItemNumbers? _numbers;
        private UserItemBools? _bools;

        /// <summary>
        /// The unique ID of this plugin data item. Should be unique across all plugin data items of any type.
        /// </summary>
        public string ID
        {
            get => _id;
            set
            {
                if (value == _id) return;
                _id = value;
                NotifyPropertyChanged(nameof(ID));
            }
        }

        /// <summary>
        /// This items properties.
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = [];

        /// <summary>
        /// A null object for this class.
        /// </summary>
        public static UserItem Null { get; internal set; } = new();

        /// <summary>
        /// Copies the public properties to a new plugin model with a new ID and returns the new model.
        /// </summary>
        /// <returns>A new model with identical properties of this model.</returns>
        public virtual UserItem Clone()
        {
            var clone = new UserItem
            {
                Properties = Properties.ToDictionary()
            };
            return clone;
        }

        /// <summary>
        /// Returns the ID of this object.
        /// </summary>
        /// <returns>The object unique ID.</returns>
        override public string ToString() => ID;

        /// <summary>
        /// Gets the property with the given name.
        /// </summary>
        /// <param name="key">The name of the property to get.</param>
        /// <returns>The value of the property.</returns>
        public string this[string key]
        {
            get => Properties.TryGetValue(key, out var value) ? value : string.Empty;
            set
            {
                if (Properties.TryGetValue(key, out var existingValue) && existingValue == value) return;
                Properties[key] = value;
                NotifyPropertyChanged(key);
            }
        }

        /// <summary>
        /// Gets the date properties of this user item.
        /// </summary>
        public UserItemDates Date
        {
            get => _dates ??= new UserItemDates(this); 
        }

        /// <summary>
        /// Gets the number properties of this user item.
        /// </summary>
        public UserItemNumbers Num
        {
            get => _numbers ??= new UserItemNumbers(this);
        }


        /// <summary>
        /// Gets the boolean properties of this user item.
        /// </summary>
        public UserItemBools Bool
        {
            get => _bools ??= new UserItemBools(this);
        }
    }

    /// <summary>
    /// Accessor for the date properties of a user item.
    /// </summary>
    public class UserItemDates
    {
        private readonly UserItem _userItem;

        /// <summary>
        /// Constructs a new instance of <see cref="UserItemDates"/>.
        /// </summary>
        /// <param name="userItem">The underlying user item.</param>
        public UserItemDates(UserItem userItem)
        {
            _userItem = userItem;
        }

        /// <summary>
        /// Gets the property with the given name as a date.
        /// </summary>
        /// <param name="key">The name of the property to get.</param>
        /// <returns>The value of the property as a date.</returns>
        public DateTime this[string key]
        {
            get => DateTime.TryParse(_userItem[key], out var result) ? result : DateTime.MinValue;
            set => _userItem[key] = value.ToString();
        }
    }

    /// <summary>
    /// Accessor for the number properties of a user item.
    /// </summary>
    public class UserItemNumbers
    {
        private readonly UserItem _userItem;

        /// <summary>
        /// Constructs a new instance of <see cref="UserItemNumbers"/>.
        /// </summary>
        /// <param name="userItem">The underlying user item.</param>
        public UserItemNumbers(UserItem userItem)
        {
            _userItem = userItem;
        }

        /// <summary>
        /// Gets the property with the given name as a number (double).
        /// </summary>
        /// <param name="key">The name of the property to get.</param>
        /// <returns>The value of the property as a number.</returns>
        public double this[string key]
        {
            get => double.TryParse(_userItem[key], out var result) ? result : 0;
            set => _userItem[key] = value.ToString();
        }
    }

    /// <summary>
    /// Accessor for the boolean properties of a user item.
    /// </summary>
    public class UserItemBools
    {
        private readonly UserItem _userItem;

        /// <summary>
        /// Constructs a new instance of <see cref="UserItemBools"/>.
        /// </summary>
        /// <param name="userItem">The underlying user item.</param>
        public UserItemBools(UserItem userItem)
        {
            _userItem = userItem;
        }

        /// <summary>
        /// Gets the property with the given name as a bool.
        /// </summary>
        /// <param name="key">The name of the property to get.</param>
        /// <returns>The value of the property as a bool.</returns>
        public bool this[string key]
        {
            get => bool.TryParse(_userItem[key], out var result) ? result : false;
            set => _userItem[key] = value.ToString();
        }
    }
}
