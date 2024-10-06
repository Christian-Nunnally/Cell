using Cell.Common;
using System.Collections.ObjectModel;

namespace Cell.Model
{
    /// <summary>
    /// A model for a sheet, which is a collection of cells. Not actually persisted to disk, instead is uses the corner cell models custom properties to store the information about the sheet, such as it's ordering.
    /// </summary>
    public class SheetModel : PropertyChangedBase
    {
        public static readonly SheetModel Null = new("");
        public string OldName;
        private CellModel? _cornerCell;
        private string _name = string.Empty;
        public SheetModel(string sheetName)
        {
            Name = sheetName;
            if (Name != sheetName) throw new ArgumentException("Invalid sheet name");
            OldName = sheetName;
        }

        public ObservableCollection<CellModel> Cells { get; set; } = [];

        public CellModel? CornerCell
        {
            get => _cornerCell;
            set
            {
                _cornerCell = value;
                NotifyPropertyChanged(nameof(Order));
            }
        }

        public bool IsVisibleInTopBar
        {
            get => CornerCell?.Properties.GetBooleanProperty("IsVisibleInTopBar", true) ?? true;
            set
            {
                if (CornerCell is not null)
                {
                    if (value == IsVisibleInTopBar) return;
                    CornerCell.Properties.SetBooleanProperty("IsVisibleInTopBar", value);
                    NotifyPropertyChanged(nameof(IsVisibleInTopBar));
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (value == _name) return;
                if (!IsValidSheetName(value)) return;
                OldName = _name;
                _name = value;
                NotifyPropertyChanged(nameof(Name));
            }
        }

        public int Order
        {
            get => (int)(CornerCell?.Properties.GetNumericProperty("SheetOrder") ?? 0);
            set
            {
                if (CornerCell is not null)
                {
                    if (value == Order) return;
                    CornerCell.Properties.SetNumericProperty("SheetOrder", value);
                    NotifyPropertyChanged(nameof(Order));
                }
            }
        }

        /// <summary>
        /// Checks a string to see if it is a valid sheet name. Invalid sheet names are those that are empty, longer than 60 characters, or contain characters other than letters, numbers, and spaces.
        /// </summary>
        /// <param name="stringToTest">The string to test.</param>
        /// <returns>True if the string is a valid sheet name.</returns>
        public static bool IsValidSheetName(string stringToTest)
        {
            if (string.IsNullOrWhiteSpace(stringToTest)) return false;
            if (stringToTest.Length > 60) return false;
            if (!stringToTest.All("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ123456789 ".Contains)) return false;
            return true;
        }
    }
}
