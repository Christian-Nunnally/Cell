namespace Cell.Model.Plugin
{
    /// <summary>
    /// A model for a transaction item, which is a single transaction in a ledger.
    /// </summary>
    public class TransactionItem : PluginModel
    {
        private double _localAmount = 0;
        private string _localCategory = string.Empty;
        private DateTime _localDate = DateTime.MinValue;
        private string _localNotes = string.Empty;
        /// <summary>
        /// The amount of the transaction.
        /// </summary>
        public double Amount
        {
            get => _localAmount;
            set 
            { 
                if (value == _localAmount) return; 
                _localAmount = value; 
                NotifyPropertyChanged(nameof(Amount)); 
            }
        }

        /// <summary>
        /// The category of the transaction.
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
        /// The date of the transaction.
        /// </summary>
        public DateTime Date
        {
            get => _localDate;
            set 
            { 
                if (value == _localDate) return; 
                _localDate = value; 
                NotifyPropertyChanged(nameof(Date)); 
            }
        }

        /// <summary>
        /// Any notes about the transaction.
        /// </summary>
        public string Notes
        {
            get => _localNotes;
            set { if (value == _localNotes) return; _localNotes = value; NotifyPropertyChanged(nameof(Notes)); }
        }

        /// <summary>
        /// Gets a string representation of the transaction item.
        /// </summary>
        /// <returns>A string representation of the transaction item.</returns>
        override public string ToString()
        {
            return $"{Category} - {Amount:C} - {Date:MM-dd-yyyy} - {Notes}";
        }
    }
}
