namespace Cell.Model.Plugin
{
    /// <summary>
    /// A budget category item that represents a category of spending in a budget.
    /// </summary>
    public class BudgetCategoryItem : PluginModel
    {
        private double _localAmount = 0;
        private string _localCategory = string.Empty;
        private int _localPeriodLength = 1;
        private string _localPeriodType = "Days";
        private int _localPriority = 0;
        private bool _localRollover = false;
        private DateTime _localStartDate = DateTime.MinValue;
        /// <summary>
        /// The goal amount for this budget category.
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
        /// The category of this budget item.
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
        /// The length of the period for this category. For example, if the period type is Days and the period length is 7, then the period is 7 days long.
        /// </summary>
        public int PeriodLength
        {
            get => _localPeriodLength;
            set
            {
                if (value == _localPeriodLength) return;
                _localPeriodLength = value;
                NotifyPropertyChanged(nameof(PeriodLength));
            }
        }

        /// <summary>
        /// The type of period for this category. Can be Days, Weeks, Months, or Years.
        /// </summary>
        public string PeriodType
        {
            get => _localPeriodType;
            set
            {
                if (value == _localPeriodType) return;
                _localPeriodType = value;
                NotifyPropertyChanged(nameof(PeriodType));
            }
        }

        /// <summary>
        /// The priority of this category. Lower numbers are higher priority.
        /// </summary>
        public int Priority
        {
            get => _localPriority;
            set
            {
                if (value == _localPriority) return;
                _localPriority = value;
                NotifyPropertyChanged(nameof(Priority));
            }
        }

        /// <summary>
        /// Whether or not the category should allow rolling over any remaining amount when the next period starts.
        /// </summary>
        public bool Rollover
        {
            get => _localRollover;
            set
            {
                if (value == _localRollover) return;
                _localRollover = value;
                NotifyPropertyChanged(nameof(Rollover));
            }
        }

        /// <summary>
        /// The start date for this category.
        /// </summary>
        public DateTime StartDate
        {
            get => _localStartDate;
            set
            {
                if (value == _localStartDate) return;
                _localStartDate = value;
                NotifyPropertyChanged(nameof(StartDate));
            }
        }

        /// <summary>
        /// Gets the period of this category given a date. The date is used to determine the period length in the case of months where the length can vary.
        /// </summary>
        /// <param name="forDate">The date to consider as the start of the period.</param>
        /// <returns>The length of the period in days.</returns>
        public TimeSpan GetPeriod(DateTime forDate)
        {
            if (PeriodType == "Days") return TimeSpan.FromDays(PeriodLength);
            else if (PeriodType == "Weeks") return TimeSpan.FromDays(PeriodLength * 7);
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
            else if (PeriodType == "Years") return TimeSpan.FromDays(PeriodLength * 365);
            return TimeSpan.FromDays(0);
        }

        /// <summary>
        /// The string version of this item.
        /// </summary>
        /// <returns>A string representation of this item.</returns>
        override public string ToString() => Category;
    }
}
