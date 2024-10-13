using Cell.Core.Common;
using Cell.Model;
using Cell.ViewModel.Cells.Types;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Cell.Core.Persistence.Migration
{
    /// <summary>
    /// A single cell in a sheet.
    /// 
    /// Contains the text, style, sheet, row, column, index, and other properties of the cell.
    /// </summary>
    public class CellModelOld : PropertyChangedBase
    {
        /// <summary>
        /// A valid cell that can be used to represent a null cell
        /// </summary>
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
        private object? _populateResult;
        private int _row;
        private string _sheetName = string.Empty;
        private string _text = string.Empty;
        private string _triggerFunctionName = string.Empty;
        private double _width;
        /// <summary>
        /// Creates a new instance of a <see cref="CellModel"/>.
        /// </summary>
        public CellModelOld()
        {
            _cellStyle.PropertyChanged += StylePropertyChangedHandler;
        }

        /// <summary>
        /// A dictionary of custom boolean properties that can be set on the cell.
        /// </summary>
        public Dictionary<string, bool> BooleanProperties { get; set; } = [];

        /// <summary>
        /// The type of cell this is such as a label, textbox, or date.
        /// 
        /// Corners, rows, and columns are also special <see cref="CellType"/>'s.
        /// </summary>
        public CellType CellType
        {
            get => _cellType;
            set { if (_cellType != value) { _cellType = value; NotifyPropertyChanged(nameof(CellType)); } }
        }

        /// <summary>
        /// The column this cell is in. 1 = A, 2 = B, etc.
        /// </summary>
        public int Column
        {
            get => _column;
            set { if (_column != value) { _column = value; NotifyPropertyChanged(nameof(Column)); } }
        }

        /// <summary>
        /// Interprets the text of this cell as a date. If the text in the cell is not a date, gives you the minimum date value of all time.
        /// </summary>
        [JsonIgnore]
        public DateTime Date { get => DateTime.TryParse(Text, out var value) ? value : DateTime.MinValue; set => Text = value.ToString(); }

        /// <summary>
        /// Contains the last error this cell encountered when running its populate or trigger functions.
        /// </summary>
        [JsonIgnore]
        public string ErrorText
        {
            get => _errorText;
            set { if (_errorText != value) { _errorText = value; NotifyPropertyChanged(nameof(ErrorText)); } }
        }

        /// <summary>
        /// The current height of the cell.
        /// </summary>
        public double Height
        {
            get => _height;
            set { if (_height != value) { _height = value; NotifyPropertyChanged(nameof(Height)); } }
        }

        /// <summary>
        /// The ID of the cell. This uniquely identifies the cell between all others in the application, and is the name of the file the cell is saved to.
        /// </summary>
        public string ID
        {
            get => _id;
            set { if (_id != null) { _id = value; NotifyPropertyChanged(nameof(ID)); } }
        }

        /// <summary>
        /// The 'index' of the cell. This is used to set which 'index' in a list this cell should represent.
        /// 
        /// This can be accessed from a function using `c.Index` or `cell.Index` (the former is preferred).
        /// 
        /// From a function this is intended to be used when accessing collections or ranges, such as:
        /// 
        /// `myCustomList[c.Index]` (To get an index out of a list) or
        /// `A1..A10[c.Index]` (To select a specific cell in a range based on the index).
        /// </summary>
        public int Index
        {
            get => _index;
            set { if (_index != value) { _index = value; NotifyPropertyChanged(nameof(Index)); } }
        }

        /// <summary>
        /// Interprets the text of this cell as an integer. If the text in the cell is not an integer, gives you 0.
        /// </summary>
        [JsonIgnore]
        public int Int { get => int.TryParse(Text, out var value) ? value : 0; set => Text = value.ToString(); }

        /// <summary>
        /// The <see cref="ID"/> of the cell this cell is merged with, if any.
        /// 
        /// If this ID is set to the ID of this cell, that means this cell is the "merge parent" (the top left cell in a merged range).
        /// </summary>
        public string MergedWith
        {
            get => _mergedWith;
            set { if (_mergedWith != value) { _mergedWith = value; NotifyPropertyChanged(nameof(MergedWith)); } }
        }

        /// <summary>
        /// A dictionary of numeric properties that can be set on the cell.
        /// 
        /// You can set a numeric property by calling `cell.SetNumericProperty("key", value)` where "key" is whatever you want to name the number and `value` is the number you want to store.
        /// 
        /// You get a numeric property by calling `var result = cell.GetNumericProperty("key")` where "key" is the key you used to set the property. `result` will contain the value, or 0 if the property with that name has never been set on this cell.
        /// </summary>
        public Dictionary<string, double> NumericProperties { get; set; } = [];

        /// <summary>
        /// The name of the populate function that is called to set the text of this cell. If it is empty, no function is called and the text can be set manually.
        /// </summary>
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

        /// <summary>
        /// The result of the last populate function run for this cell.
        /// 
        /// This value is not persisted.
        /// </summary>
        [JsonIgnore]
        public object? PopulateResult
        {
            get => _populateResult;
            set { if (_populateResult != value) { _populateResult = value; NotifyPropertyChanged(nameof(PopulateResult)); } }
        }

        /// <summary>
        /// The row this cell is in. 1 = 1, 2 = 2, etc.
        /// </summary>
        public int Row
        {
            get => _row;
            set { if (_row != value) { _row = value; NotifyPropertyChanged(nameof(Row)); } }
        }

        /// <summary>
        /// Gets the text of the selected item in a list cell.
        /// </summary>
        public string SelectedItem => GetStringProperty(nameof(ListCellViewModel.SelectedItem));

        /// <summary>
        /// The name of the sheet this cell is in.
        /// </summary>
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

        /// <summary>
        /// A dictionary of string properties that can be set on the cell.
        /// 
        /// You can set a string property by calling `cell.SetStringProperty("key", value)` where "key" is whatever you want to name the string and `value` is the string you want to store.
        /// 
        /// You get a string property by calling `var result = cell.GetStringProperty("key")` where "key" is the key you used to set the property. `result` will be an empty string if the property with that name has never been set on this cell.
        /// </summary>
        public Dictionary<string, string> StringProperties { get; set; } = [];

        /// <summary>
        /// The style of this cell. Contains all of the visual properties of the cell, such as the font, font size, and background color.
        /// </summary>
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

        private void LocationPropertyChangedHandler(object? sender, PropertyChangedEventArgs e)
        {
            throw new NotImplementedException("needed?");
        }

        /// <summary>
        /// The text of the cell. This is the main value all cells will display.
        /// 
        /// There are useful properties such as <see cref="Int"/>, <see cref="Date"/>, and <see cref="Value"/> that can be used to interpret the text as a integer (5), date (12-2-2024), or double (1.23).
        /// </summary>
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

        /// <summary>
        /// The name of the trigger function that is called when the cell is changed, or when an event happens from this cell like a checkmark change or a button press. If it is empty, no function is called when the cell is changed.
        /// </summary>
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

        /// <summary>
        /// The user-friendly name of the cell. This is the column name and row number of the cell, like A1 instead of 1,1.
        /// </summary>
        public string UserFriendlyCellName => $"{ColumnCellViewModel.GetColumnName(Column)}{Row}";

        /// <summary>
        /// The text of the cell interpreted as a double. If the text in the cell is not a double, gives you 0.
        /// </summary>
        [JsonIgnore]
        public double Value { get => double.TryParse(Text, out var value) ? value : 0; set => Text = value.ToString(); }

        /// <summary>
        /// The current width of the cell.
        /// </summary>
        public double Width
        {
            get => _width;
            set { if (_width != value) { _width = value; NotifyPropertyChanged(nameof(Width)); } }
        }

        /// <summary>
        /// Gets the value of a custom boolean property on the cell.
        /// </summary>
        /// <param name="key">The user provided name of the property</param>
        /// <returns>True or false</returns>
        public bool GetBooleanProperty(string key) => BooleanProperties.TryGetValue(key, out var value) && value;

        /// <summary>
        /// Gets the value of a custom boolean property on the cell. and returns the default value if the property has not been set.
        /// </summary>
        /// <param name="key">The user provided name of the property</param>
        /// <param name="defaultValue">The value to return if this property has never been set on this cell.</param>
        /// <returns></returns>
        public bool GetBooleanProperty(string key, bool defaultValue)
        {
            return BooleanProperties.TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// Gets the value of a custom numeric property on the cell. Returns a default value if the property has not been set.
        /// </summary>
        /// <param name="key">The name of the property to get the value of.</param>
        /// <param name="defaultValue">The value to return if the property has not been set.</param>
        /// <returns></returns>
        public double GetNumericProperty(string key, double defaultValue = 0) => NumericProperties.TryGetValue(key, out var value) ? value : defaultValue;

        /// <summary>
        /// Gets a custom string property stored on the cell.
        /// </summary>
        /// <param name="key">The name of the custom property to get.</param>
        /// <returns></returns>
        public string GetStringProperty(string key) => StringProperties.TryGetValue(key, out var value) ? value : string.Empty;

        /// <summary>
        /// Sets both the background and the content background of the cell to the same color.
        /// </summary>
        /// <param name="color">The color to set the backgrounds to.</param>
        public void SetBackgrounds(string color)
        {
            Style.BackgroundColor = color;
            Style.ContentBackgroundColor = color;
        }

        /// <summary>
        /// Sets the value of a custom boolean property on the cell.
        /// </summary>
        /// <param name="key">The name of the custom property to set.</param>
        /// <param name="value">The value to set it to.</param>
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

        /// <summary>
        /// Sets both the border and the content border of the cell to the same color.
        /// </summary>
        /// <param name="color">The color to set the borders to.</param>
        public void SetBorders(string color)
        {
            Style.BorderColor = color;
            Style.ContentBorderColor = color;
        }

        /// <summary>
        /// Sets the color of both the backgrounds and the borders of the cell at the same time.
        /// </summary>
        /// <param name="color">The color to set the entire cell to (excluding the foreground)</param>
        public void SetColor(string color)
        {
            SetBackgrounds(color);
            SetBorders(color);
        }

        /// <summary>
        /// Sets the value of a custom numeric property on the cell.
        /// </summary>
        /// <param name="key">The custom name to give the property</param>
        /// <param name="value">The value to set the property to.</param>
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

        /// <summary>
        /// Sets the value of a custom string property on the cell.
        /// </summary>
        /// <param name="key">The custom name to give the property</param>
        /// <param name="value">The value to set the property to</param>
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

        /// <summary>
        /// Converts the cell to a string (gets the text of the cell).
        /// </summary>
        /// <returns>The <see cref="Text"/> of this cell.</returns>
        public override string ToString() => Text;

        private void StylePropertyChangedHandler(object? sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(Style));
        }
    }
}
