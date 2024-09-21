using Cell.Common;
using Cell.ViewModel.Cells.Types;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Cell.Model
{
    public class CellModel : PropertyChangedBase
    {
        public static readonly CellModel Null = new();
        private CellStyleModel _cellStyle = new();
        private CellType _cellType = CellType.None;
        private int _column;
        private string _errorText = string.Empty;
        private double _height;
        private string _id = Utilities.GenerateUnqiueId(12);
        private int _index = 0;
        private string _mergedWith = string.Empty;
        private string _populateFunctionName = string.Empty;
        private int _row;
        private string _sheetName = string.Empty;
        private string _text = string.Empty;
        private string _triggerFunctionName = string.Empty;
        private double _width;
        public CellModel()
        {
            _cellStyle.PropertyChanged += StylePropertyChangedHandler;
        }

        public Dictionary<string, bool> BooleanProperties { get; set; } = [];

        public CellType CellType
        {
            get => _cellType;
            set { if (_cellType != value) { _cellType = value; NotifyPropertyChanged(nameof(CellType)); } }
        }

        public int Column
        {
            get => _column;
            set { if (_column != value) { _column = value; NotifyPropertyChanged(nameof(Column)); } }
        }

        [JsonIgnore]
        public DateTime Date { get => DateTime.TryParse(Text, out var value) ? value : DateTime.MinValue; set => Text = value.ToString(); }

        [JsonIgnore]
        public string ErrorText
        {
            get => _errorText;
            set { if (_errorText != value) { _errorText = value; NotifyPropertyChanged(nameof(ErrorText)); } }
        }

        public double Height
        {
            get => _height;
            set { if (_height != value) { _height = value; NotifyPropertyChanged(nameof(Height)); } }
        }

        public string ID
        {
            get => _id;
            set { if (_id != null) { _id = value; NotifyPropertyChanged(nameof(ID)); } }
        }

        public int Index
        {
            get => _index;
            set { if (_index != value) { _index = value; NotifyPropertyChanged(nameof(Index)); } }
        }

        [JsonIgnore]
        public int Int { get => int.TryParse(Text, out var value) ? value : 0; set => Text = value.ToString(); }

        public string MergedWith
        {
            get => _mergedWith;
            set { if (_mergedWith != value) { _mergedWith = value; NotifyPropertyChanged(nameof(MergedWith)); } }
        }

        public Dictionary<string, double> NumericProperties { get; set; } = [];

        public string PopulateFunctionName
        {
            get => _populateFunctionName;
            set
            {
                if (_populateFunctionName == value) return;
                _populateFunctionName = value;
                NotifyPropertyChanged(nameof(PopulateFunctionName));
            }
        }

        public int Row
        {
            get => _row;
            set { if (_row != value) { _row = value; NotifyPropertyChanged(nameof(Row)); } }
        }

        public string SelectedItem => GetStringProperty(nameof(ListCellViewModel.SelectedItem));

        public string SheetName
        {
            get => _sheetName;
            set
            {
                if (_sheetName == value) return;
                _sheetName = value;
                NotifyPropertyChanged(nameof(SheetName));
            }
        }

        public Dictionary<string, string> StringProperties { get; set; } = [];

        public CellStyleModel Style
        {
            get => _cellStyle;
            set
            {
                if (_cellStyle == value) return;
                if (_cellStyle != null) _cellStyle.PropertyChanged -= StylePropertyChangedHandler;
                _cellStyle = value;
                if (_cellStyle != null) _cellStyle.PropertyChanged += StylePropertyChangedHandler;
                NotifyPropertyChanged(nameof(Style));
            }
        }

        public string Text
        {
            get => _text;
            set
            {
                if (_text == value) return;
                _text = value;
                NotifyPropertyChanged(nameof(Text));
            }
        }

        public string TriggerFunctionName
        {
            get => _triggerFunctionName;
            set
            {
                if (_triggerFunctionName == value) return;
                _triggerFunctionName = value;
                NotifyPropertyChanged(nameof(TriggerFunctionName));
            }
        }

        public string UserFriendlyCellName => $"{ColumnCellViewModel.GetColumnName(Column)}{Row}";

        [JsonIgnore]
        public double Value { get => double.TryParse(Text, out var value) ? value : 0; set => Text = value.ToString(); }

        public double Width
        {
            get => _width;
            set { if (_width != value) { _width = value; NotifyPropertyChanged(nameof(Width)); } }
        }

        public bool GetBooleanProperty(string key) => BooleanProperties.TryGetValue(key, out var value) && value;

        public bool GetBooleanProperty(string key, bool defaultValue)
        {
            return BooleanProperties.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public double GetNumericProperty(string key, double defaultValue = 0) => NumericProperties.TryGetValue(key, out var value) ? value : defaultValue;

        public string GetStringProperty(string key) => StringProperties.TryGetValue(key, out var value) ? value : string.Empty;

        public void SetBackgrounds(string color)
        {
            Style.BackgroundColor = color;
            Style.ContentBackgroundColor = color;
        }

        public void SetBooleanProperty(string key, bool value)
        {
            if (BooleanProperties.TryGetValue(key, out var currentValue))
            {
                if (currentValue == value) return;
                BooleanProperties[key] = value;
            }
            else BooleanProperties.Add(key, value);
            NotifyPropertyChanged(nameof(BooleanProperties), key);
        }

        public void SetBorders(string color)
        {
            Style.BorderColor = color;
            Style.ContentBorderColor = color;
        }

        public void SetColor(string color)
        {
            SetBackgrounds(color);
            SetBorders(color);
        }

        public void SetItems(IEnumerable<object> objects)
        {
            SetStringProperty(nameof(DropdownCellViewModel.CommaSeperatedItems), string.Join(',', objects));
        }

        // API extensions (move to api model object)
        public void SetItems(string commaSeperatedItems)
        {
            SetStringProperty(nameof(DropdownCellViewModel.CommaSeperatedItems), commaSeperatedItems);
        }

        public void SetNumericProperty(string key, double value)
        {
            if (NumericProperties.TryGetValue(key, out var currentValue) && currentValue != value)
            {
                if (currentValue == value) return;
                NumericProperties[key] = value;
            }
            else NumericProperties.Add(key, value);
            NotifyPropertyChanged(nameof(NumericProperties));
            NotifyPropertyChanged(key);
        }

        public void SetStringProperty(string key, string value)
        {
            if (StringProperties.TryGetValue(key, out var currentValue))
            {
                if (currentValue == value) return;
                StringProperties[key] = value;
            }
            else StringProperties.Add(key, value);
            NotifyPropertyChanged(nameof(StringProperties));
            NotifyPropertyChanged(key);
        }

        public override string ToString() => Text;

        private void StylePropertyChangedHandler(object? sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(Style));
        }
    }
}
