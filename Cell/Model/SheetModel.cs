using Cell.Common;
using System.Collections.ObjectModel;

namespace Cell.Model
{
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
            get => CornerCell?.GetBooleanProperty("IsVisibleInTopBar", true) ?? true;
            set
            {
                if (CornerCell is not null)
                {
                    if (value == IsVisibleInTopBar) return;
                    CornerCell.SetBooleanProperty("IsVisibleInTopBar", value);
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
            get => (int)(CornerCell?.GetNumericProperty("SheetOrder") ?? 0);
            set
            {
                if (CornerCell is not null)
                {
                    if (value == Order) return;
                    CornerCell.SetNumericProperty("SheetOrder", value);
                    NotifyPropertyChanged(nameof(Order));
                }
            }
        }

        public static bool IsValidSheetName(string sheetName)
        {
            if (string.IsNullOrWhiteSpace(sheetName)) return false;
            if (sheetName.Length > 60) return false;
            if (!sheetName.All("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ123456789".Contains)) return false;
            return true;
        }
    }
}
