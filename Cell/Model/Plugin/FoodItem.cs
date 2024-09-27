namespace Cell.Model.Plugin
{
    public class FoodItem : PluginModel
    {
        private int _daysWillStayFresh = 0;
        private string _unit = string.Empty;
        private string _localCategory = string.Empty;
        private DateTime _localDate = DateTime.MinValue;
        private string _localNotes = string.Empty;
        public int DaysWillStayFresh
        {
            get => _daysWillStayFresh;
            set 
            { 
                if (value == _daysWillStayFresh) return;
                _daysWillStayFresh = value; 
                OnPropertyChanged(nameof(DaysWillStayFresh)); 
            }
        }
        public string Unit
        {
            get => _unit;
            set
            {
                if (value == _unit) return;
                _unit = value;
                OnPropertyChanged(nameof(Unit));
            }
        }

        private string _name = "";
        public string Name
        {
            get => _unit;
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        //private string _name = 0;
        //public string Name
        //{
        //    get => _unit;
        //    set
        //    {
        //        if (value == _name) return;
        //        _name = value;
        //        OnPropertyChanged(nameof(Name));
        //    }
        //}

        public string Category
        {
            get => _localCategory;
            set { if (value == _localCategory) return; _localCategory = value; OnPropertyChanged(nameof(Category)); }
        }

        public DateTime Date
        {
            get => _localDate;
            set { if (value == _localDate) return; _localDate = value; OnPropertyChanged(nameof(Date)); }
        }

        public string Notes
        {
            get => _localNotes;
            set { if (value == _localNotes) return; _localNotes = value; OnPropertyChanged(nameof(Notes)); }
        }

        override public string ToString()
        {
            return $"{Category} - {DaysWillStayFresh:C} - {Date:MM-dd-yyyy} - {Notes}";
        }
    }
}
