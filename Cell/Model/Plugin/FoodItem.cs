namespace Cell.Model.Plugin
{
    /// <summary>
    /// A model for a food item.
    /// </summary>
    public class FoodItem : PluginModel
    {
        private int _daysWillStayFresh = 0;
        private string _localCategory = string.Empty;
        private DateTime _purchasedDate = DateTime.MinValue;
        private string _localNotes = string.Empty;
        private string _name = "";
        private string _unit = string.Empty;

        /// <summary>
        /// Gets or sets the category of the food item.
        /// </summary>
        public string Category
        {
            get => _localCategory;
            set
            {
                if (value == _localCategory) return;
                _localCategory = value;
                NotifyPropertyChanged(nameof(Category));
            }
        }

        /// <summary>
        /// Gets or sets the date the food item was purchased.
        /// </summary>
        public DateTime PurchasedDate
        {
            get => _purchasedDate;
            set
            {
                if (value == _purchasedDate) return;
                _purchasedDate = value;
                NotifyPropertyChanged(nameof(PurchasedDate));
            }
        }

        /// <summary>
        /// Gets or sets the number of days the food item will stay fresh.
        /// </summary>
        public int DaysWillStayFresh
        {
            get => _daysWillStayFresh;
            set
            {
                if (value == _daysWillStayFresh) return;
                _daysWillStayFresh = value;
                NotifyPropertyChanged(nameof(DaysWillStayFresh));
            }
        }

        /// <summary>
        /// Gets or sets the name of the food item.
        /// </summary>
        public string Name
        {
            get => _unit;
            set
            {
                if (value == _name) return;
                _name = value;
                NotifyPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// Gets or sets any notes for this food item.
        /// </summary>
        public string Notes
        {
            get => _localNotes;
            set
            {
                if (value == _localNotes) return;
                _localNotes = value;
                NotifyPropertyChanged(nameof(Notes));
            }
        }

        /// <summary>
        /// Gets or sets the unit of the food item.
        /// </summary>
        public string Unit
        {
            get => _unit;
            set
            {
                if (value == _unit) return;
                _unit = value;
                NotifyPropertyChanged(nameof(Unit));
            }
        }

        /// <summary>
        /// Gets the string representation of this food item.
        /// </summary>
        /// <returns>The string representation of this food item.</returns>
        override public string ToString()
        {
            return $"{Category} - {DaysWillStayFresh:C} - {PurchasedDate:MM-dd-yyyy} - {Notes}";
        }
    }
}
