namespace Cell.Model.Plugin
{
    public class TransactionItem : PluginModel
    {
        private double _localAmount = 0;
        private string _localCategory = string.Empty;
        private DateTime _localDate = DateTime.MinValue;
        private string _localNotes = string.Empty;
        public double Amount
        {
            get => _localAmount;
            set { if (value == _localAmount) return; _localAmount = value; NotifyPropertyChanged(nameof(Amount)); }
        }

        public string Category
        {
            get => _localCategory;
            set { if (value == _localCategory) return; _localCategory = value; NotifyPropertyChanged(nameof(Category)); }
        }

        public DateTime Date
        {
            get => _localDate;
            set { if (value == _localDate) return; _localDate = value; NotifyPropertyChanged(nameof(Date)); }
        }

        public string Notes
        {
            get => _localNotes;
            set { if (value == _localNotes) return; _localNotes = value; NotifyPropertyChanged(nameof(Notes)); }
        }

        override public string ToString()
        {
            return $"{Category} - {Amount:C} - {Date:MM-dd-yyyy} - {Notes}";
        }
    }
}
