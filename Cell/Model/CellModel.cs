using Cell.Persistence;
using Cell.Plugin;
using System.Text.Json;

namespace Cell.Model
{
    public class CellModel : PropertyChangedBase
    {
        public event Action<CellModel, EditContext>? OnCellEdited;
        public event Action<CellModel>? AfterCellEdited;

        #region Layout Properties

        public double Width
        {
            get { return width; }
            set { if (width != value) { width = value; OnPropertyChanged(nameof(Width)); } }
        }
        private double width;

        public double Height
        {
            get { return height; }
            set { if (height != value) { height = value; OnPropertyChanged(nameof(Height)); } }
        }
        private double height;

        #endregion

        #region Cell Properties

        public CellType CellType
        {
            get { return cellType; }
            set { cellType = value; OnPropertyChanged(nameof(CellType)); }
        }
        private CellType cellType = CellType.None;

        public string ID
        {
            get { return id; }
            set { id = value; OnPropertyChanged(nameof(ID)); }
        }
        private string id = Utilities.GenerateUnqiueId(12);

        public int Column
        {
            get { return column; }
            set { if (column != value) { column = value; OnPropertyChanged(nameof(Column)); } }
        }
        private int column;

        public int Row
        {
            get { return row; }
            set { if (row != value) { row = value; OnPropertyChanged(nameof(Row)); }; }
        }
        private int row;

        public string SheetName
        {
            get { return sheetName; }
            set { sheetName = value; OnPropertyChanged(nameof(SheetName)); }
        }
        private string sheetName = string.Empty;

        public string MergedWith
        {
            get { return mergedWith; }
            set { mergedWith = value; OnPropertyChanged(nameof(MergedWith)); }
        }
        private string mergedWith = string.Empty;

        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                OnPropertyChanged(nameof(Text));
            }
        }
        private string text = string.Empty;

        public string Value
        {
            get { return valueLocal; }
            set
            {
                var oldValue = valueLocal;
                valueLocal = value;
                OnPropertyChanged(nameof(Value));
                OnPropertyChanged(nameof(Text));
                OnCellEdited?.Invoke(this, new EditContext(nameof(Value), valueLocal, oldValue));
                AfterCellEdited?.Invoke(this);
            }
        }
        private string valueLocal = string.Empty;

        public int Index
        {
            get { return index; }
            set { index = value; OnPropertyChanged(nameof(Index)); }
        }
        private int index = 0;

        #endregion

        #region Style Properties

        public string BackgroundColorHex
        {
            get { return backgroundColorHex; }
            set { if (backgroundColorHex == value) return; backgroundColorHex = value; OnPropertyChanged(nameof(BackgroundColorHex)); }
        }
        private string backgroundColorHex = "#1e1e1e";

        public string ForegroundColorHex
        {
            get { return foregroundColorHex; }
            set { if (foregroundColorHex == value) return; foregroundColorHex = value; OnPropertyChanged(nameof(ForegroundColorHex)); }
        }
        private string foregroundColorHex = "#ffffff";

        public string BorderColorHex
        {
            get { return borderColorHex; }
            set { if (borderColorHex == value) return; borderColorHex = value; OnPropertyChanged(nameof(BorderColorHex)); }
        }
        private string borderColorHex = "#2d2d30";

        public string BorderThicknessString
        {
            get { return borderThickness; }
            set { if (borderThickness == value) return; borderThickness = value; OnPropertyChanged(nameof(BorderThicknessString)); }
        }
        private string borderThickness = "1,1,1,1";

        public double FontSize
        {
            get { return fontSize; }
            set { if (fontSize == value) return; fontSize = value; OnPropertyChanged(nameof(FontSize)); }
        }
        private double fontSize = 10;

        public string FontFamily
        {
            get { return font; }
            set { if (font == value) return; font = value; OnPropertyChanged(nameof(FontFamily)); }
        }
        private string font = "Consolas";

        #endregion

        #region Plugin Properties

        public string PopulateFunctionName
        {
            get { return populateFunctionName; }
            set 
            {
                if (populateFunctionName == value) return;
                if (PluginFunctionLoader.TryGetFunction(PluginFunctionLoader.PopulateFunctionsDirectoryName, populateFunctionName, out var function))
                {
                    function.StopListeningForDependencyChanges(this);
                }
                populateFunctionName = value;
                if (PluginFunctionLoader.TryGetFunction(PluginFunctionLoader.PopulateFunctionsDirectoryName, populateFunctionName, out var function2))
                {
                    function2.StartListeningForDependencyChanges(this);
                    UpdateDependencySubscriptions(function2);
                }

                OnPropertyChanged(nameof(PopulateFunctionName)); 
            }
        }

        public void UpdateDependencySubscriptions(PluginFunction function)
        {
            CellUpdateManager.UnsubscribeFromCellValueUpdates(this);
            for (int i = 0; i < function.SheetDependencies.Count; i++)
            {
                var sheetName = function.SheetDependencies[i];
                var row = function.RowDependencies[i];
                var column = function.ColumnDependencies[i];
                sheetName = string.IsNullOrWhiteSpace(sheetName) ? SheetName : sheetName;
                CellUpdateManager.SubscribeToCellValueUpdates(this, sheetName, row, column);
            }
        }

        private string populateFunctionName = string.Empty;

        public string TriggerFunctionName
        {
            get { return triggerFunctionName; }
            set { triggerFunctionName = value; OnPropertyChanged(nameof(TriggerFunctionName)); }
        }
        private string triggerFunctionName = string.Empty;

        #endregion

        #region String Properties

        public Dictionary<string, string> StringProperties
        {
            get { return stringProperties; }
            set { stringProperties = value; OnPropertyChanged(nameof(StringProperties)); }
        }
        private Dictionary<string, string> stringProperties = [];

        internal string GetStringProperty(string key)
        {
            if (StringProperties.TryGetValue(key, out var value)) return value;
            return "";
        }

        internal void SetStringProperty(string key, string value)
        {
            if (StringProperties.TryGetValue(key, out var currentValue) && currentValue != value)
            {
                StringProperties[key] = value;
                OnPropertyChanged(nameof(StringProperties));
            }
            else StringProperties.Add(key, value);
        }

        #endregion

        #region Boolean Properties

        public Dictionary<string, bool> BooleanProperties
        {
            get { return booleanProperties; }
            set { booleanProperties = value; OnPropertyChanged(nameof(BooleanProperties)); }
        }
        private Dictionary<string, bool> booleanProperties = [];

        internal bool GetBooleanProperty(string key)
        {
            if (BooleanProperties.TryGetValue(key, out var value)) return value;
            return false;
        }

        internal void SetBooleanProperty(string key, bool value)
        {
            if (BooleanProperties[key] != value)
            {
                BooleanProperties[key] = value;
                OnPropertyChanged(nameof(BooleanProperties));
            }
        }

        #endregion

        #region Numeric Properties

        public Dictionary<string, double> NumericProperties
        {
            get { return numericProperties; }
            set { numericProperties = value; OnPropertyChanged(nameof(NumericProperties)); }
        }
        private Dictionary<string, double> numericProperties = [];

        internal double GetNumericProperty(string key)
        {
            if (NumericProperties.TryGetValue(key, out var value)) return value;
            return 0;
        }

        internal void SetNumericProperty(string key, double value)
        {
            if (NumericProperties[key] != value)
            {
                NumericProperties[key] = value;
                OnPropertyChanged(nameof(NumericProperties));
            }
        }

        #endregion

        #region Serialization

        public static string SerializeModel(CellModel model) => JsonSerializer.Serialize(model);

        public static CellModel DeserializeModel(string serializedModel) => JsonSerializer.Deserialize<CellModel>(serializedModel) ?? throw new InvalidOperationException("DeserializeModel failed.");

        #endregion
    }
}