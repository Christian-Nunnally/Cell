using Cell.Common;
using Cell.ViewModel.Cells.Types;
using System.Text.Json.Serialization;

namespace Cell.Model
{
    /// <summary>
    /// A single cell in a sheet.
    /// 
    /// Contains the text, style, sheet, row, column, index, and other properties of the cell.
    /// </summary>
    public class CellModel : PropertyChangedBase
    {
        /// <summary>
        /// A valid cell that can be used to represent a null cell
        /// </summary>
        public static readonly CellModel Null = new();
        private CellStyleModel _cellStyle = new();
        private CellLocationModel _cellLocation = new();
        private CellModelCustomPropertiesModel _customProperties = new();
        private CellType _cellType = CellType.None;
        private double _height;
        private string _id = Utilities.GenerateUnqiueId(12);
        private int _index = 0;
        private string _mergedWith = string.Empty;
        private string _populateFunctionName = string.Empty;
        private object? _populateResult;
        private string _text = string.Empty;
        private string _triggerFunctionName = string.Empty;
        private double _width;

        /// <summary>
        /// The type of cell this is such as a label, textbox, or date.
        /// 
        /// Corners, rows, and columns are also special <see cref="CellType"/>'s.
        /// </summary>
        public CellType CellType
        {
            get => _cellType;
            set 
            {
                if (_cellType == value) return;
                _cellType = value;
                NotifyPropertyChanged(nameof(CellType)); 
            }
        }

        /// <summary>
        /// Interprets the text of this cell as a date. If the text in the cell is not a date, gives you the minimum date value of all time.
        /// </summary>
        [JsonIgnore]
        public DateTime Date 
        { 
            get => DateTime.TryParse(Text, out var value) ? value : DateTime.MinValue; 
            set => Text = value.ToString(); 
        }

        /// <summary>
        /// The current height of the cell.
        /// </summary>
        public double Height
        {
            get => _height;
            set 
            {
                if (_height == value) return;
                _height = value; 
                NotifyPropertyChanged(nameof(Height)); 
            }
        }

        /// <summary>
        /// The ID of the cell. This uniquely identifies the cell between all others in the application, and is the name of the file the cell is saved to.
        /// </summary>
        public string ID
        {
            get => _id;
            set 
            {
                if (_id == null) return;
                _id = value; 
                NotifyPropertyChanged(nameof(ID));
            }
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
            set 
            {
                if (_index == value) return;
                _index = value; 
                NotifyPropertyChanged(nameof(Index));
            }
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
            set 
            {
                if (_mergedWith == value) return;
                _mergedWith = value; 
                NotifyPropertyChanged(nameof(MergedWith)); 
            }
        }

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
            set 
            {
                if (_populateResult == value) return;
                _populateResult = value; 
                NotifyPropertyChanged(nameof(PopulateResult)); 
            }
        }

        /// <summary>
        /// Gets the text of the selected item in a list cell.
        /// </summary>
        public string SelectedItem => Properties[nameof(ListCellViewModel.SelectedItem)];

        /// <summary>
        /// A dictionary of string properties that can be set on the cell.
        /// 
        /// You can set a string property by calling `cell.SetStringProperty("key", value)` where "key" is whatever you want to name the string and `value` is the string you want to store.
        /// 
        /// You get a string property by calling `var result = cell.GetStringProperty("key")` where "key" is the key you used to set the property. `result` will be an empty string if the property with that name has never been set on this cell.
        /// </summary>
        public CellModelCustomPropertiesModel Properties 
        { 
            get => _customProperties;
            set
            {
                if (_customProperties == value) return;
                _customProperties = value;
                NotifyPropertyChanged(nameof(Properties));
            }
        }

        /// <summary>
        /// The style of this cell. Contains all of the visual properties of the cell, such as the font, font size, and background color.
        /// </summary>
        public CellStyleModel Style
        {
            get => _cellStyle;
            set
            {
                if (_cellStyle == value) return;
                if (_cellStyle != null) _cellStyle.CellModel = null;
                _cellStyle = value;
                if (_cellStyle != null) _cellStyle.CellModel = this;
                NotifyPropertyChanged(nameof(Style));
            }
        }

        /// <summary>
        /// The location of the cell in a sheet.
        /// </summary>
        public CellLocationModel Location
        {
            get => _cellLocation;
            set
            {
                if (_cellLocation == value) return;
                if (_cellLocation != null) _cellLocation.CellModel = null;
                _cellLocation = value;
                if (_cellLocation != null) _cellLocation.CellModel = this;
                NotifyPropertyChanged(nameof(Location));
            }
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
            set 
            {
                if (_width == value) return;
                _width = value; 
                NotifyPropertyChanged(nameof(Width)); 
            }
        }
    }
}
