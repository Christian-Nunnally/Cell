namespace Cell.Model.Plugin
{
    public class FoodItem : PluginModel
    {
        private int _daysWillStayFresh = 0;
        private string _localCategory = string.Empty;
        private DateTime _localDate = DateTime.MinValue;
        private string _localNotes = string.Empty;
        private string _name = "";
        private string _unit = string.Empty;
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
            set
            {
                if (value == _localCategory) return;
                _localCategory = value;
                NotifyPropertyChanged(nameof(Category));
            }
        }

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

        override public string ToString()
        {
            return $"{Category} - {DaysWillStayFresh:C} - {Date:MM-dd-yyyy} - {Notes}";
        }
    }
}
