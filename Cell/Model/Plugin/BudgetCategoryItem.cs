namespace Cell.Model.Plugin
{
    public class BudgetCategoryItem : PluginModel
    {
        private double _localAmount = 0;
        private string _localCategory = string.Empty;
        private int _localPeriodLength = 1;
        private string _localPeriodType = "Days";
        private int _localPriority = 0;
        private bool _localRollover = false;
        private DateTime _localStartDate = DateTime.MinValue;
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

        public int PeriodLength
        {
            get => _localPeriodLength;
            set { if (value == _localPeriodLength) return; _localPeriodLength = value; NotifyPropertyChanged(nameof(PeriodLength)); }
        }

        public string PeriodType
        {
            get => _localPeriodType;
            set { if (value == _localPeriodType) return; _localPeriodType = value; NotifyPropertyChanged(nameof(PeriodType)); }
        }

        public int Priority
        {
            get => _localPriority;
            set { if (value == _localPriority) return; _localPriority = value; NotifyPropertyChanged(nameof(Priority)); }
        }

        public bool Rollover
        {
            get => _localRollover;
            set { if (value == _localRollover) return; _localRollover = value; NotifyPropertyChanged(nameof(Rollover)); }
        }

        public DateTime StartDate
        {
            get => _localStartDate;
            set { if (value == _localStartDate) return; _localStartDate = value; NotifyPropertyChanged(nameof(StartDate)); }
        }

        public TimeSpan GetPeriod(DateTime forDate)
        {
            if (PeriodType == "Days")
                return TimeSpan.FromDays(PeriodLength);
            else if (PeriodType == "Weeks")
                return TimeSpan.FromDays(PeriodLength * 7);
            else if (PeriodType == "Months")
            {
                var currentMonth = forDate.Month;
                var currentYear = forDate.Year;
                var totalDays = 0;
                for (int i = 0; i < PeriodLength; i++)
                {
                    totalDays += DateTime.DaysInMonth(currentYear, currentMonth);
                    currentMonth++;
                    if (currentMonth > 12)
                    {
                        currentMonth = 1;
                        currentYear++;
                    }
                }
                return TimeSpan.FromDays(totalDays);
            }
            else if (PeriodType == "Years")
                return TimeSpan.FromDays(PeriodLength * 365);
            return TimeSpan.FromSeconds(0);
        }

        override public string ToString()
        {
            return $"{Priority} {Category} - {Amount} - {StartDate:MM-dd-yyyy} - {PeriodLength} {PeriodType}";
        }
    }
}
