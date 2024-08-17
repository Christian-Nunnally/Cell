
namespace Cell.Model.Plugin
{
    public class TransactionItem : PluginModel
    {
        public string Category
        {
            get => _localCategory;
            set { if (value == _localCategory) return; _localCategory = value; OnPropertyChanged(nameof(Category)); }
        }
        private string _localCategory = string.Empty;

        public double Amount
        {
            get => _localAmount;
            set { if (value == _localAmount) return; _localAmount = value; OnPropertyChanged(nameof(Amount)); }
        }
        private double _localAmount = 0;

        public DateTime Date
        {
            get => _localDate;
            set { if (value == _localDate) return; _localDate = value; OnPropertyChanged(nameof(Date)); }
        }
        private DateTime _localDate = DateTime.MinValue;

        public string Notes
        {
            get => _localNotes;
            set { if (value == _localNotes) return; _localNotes = value; OnPropertyChanged(nameof(Notes)); }
        }
        private string _localNotes = string.Empty;

        override public string ToString()
        {
            return $"{Category} - {Amount:C} - {Date:MM-dd-yyyy} - {Notes}";
        }
    }
}
