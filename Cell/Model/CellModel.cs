using Cell.Persistence;
using Cell.Plugin;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                if (text == value) return;
                var oldValue = text;
                text = value;
                OnPropertyChanged(nameof(Text));
                OnCellEdited?.Invoke(this, new EditContext(nameof(Text), text, oldValue));
                AfterCellEdited?.Invoke(this);
            }
        }
        private string text = string.Empty;

        public string ErrorText
        {
            get { return errorText; }
            set { if (errorText != value) { errorText = value; OnPropertyChanged(nameof(ErrorText)); } }
        }
        private string errorText = string.Empty;

        [JsonIgnore]
        public double Value
        {
            get => double.TryParse(Text, out var value) ? value : 0;
            set => Text = value.ToString();
        }

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

        public bool IsFontBold
        {
            get { return isFontBold; }
            set { if (isFontBold == value) return; isFontBold = value; OnPropertyChanged(nameof(IsFontBold)); }
        }
        private bool isFontBold = false;

        public bool IsFontItalic
        {
            get { return isFontItalic; }
            set { if (isFontItalic == value) return; isFontItalic = value; OnPropertyChanged(nameof(IsFontItalic)); }
        }
        private bool isFontItalic = false;

        public bool IsFontStrikethrough
        {
            get { return isFontStrikethrough; }
            set { if (isFontStrikethrough == value) return; isFontStrikethrough = value; OnPropertyChanged(nameof(IsFontStrikethrough)); }
        }
        private bool isFontStrikethrough = false;

        public HorizontalAlignment HorizontalAlignment
        {
            get { return horizontalAlignment; }
            set { if (horizontalAlignment == value) return; horizontalAlignment = value; OnPropertyChanged(nameof(HorizontalAlignment)); }
        }
        private HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center;

        public VerticalAlignment VerticalAlignment
        {
            get { return verticalAlignment; }
            set { if (verticalAlignment == value) return; verticalAlignment = value; OnPropertyChanged(nameof(VerticalAlignment)); }
        }
        private VerticalAlignment verticalAlignment = VerticalAlignment.Center;

        public TextAlignment TextAlignmentForView
        {
            get { return textAlignment; }
            set { if (textAlignment == value) return; textAlignment = value; OnPropertyChanged(nameof(TextAlignment)); }
        }
        private TextAlignment textAlignment = TextAlignment.Center;

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
                    var method = function2.CompiledMethod;
                    UpdateDependencySubscriptions(function2);
                }

                OnPropertyChanged(nameof(PopulateFunctionName));
            }
        }

        public bool NeedsUpdateDependencySubscriptionsToBeCalled;

        public bool UpdateDependencySubscriptions()
        {
            if (string.IsNullOrWhiteSpace(populateFunctionName)) return false;
            if (PluginFunctionLoader.TryGetFunction(PluginFunctionLoader.PopulateFunctionsDirectoryName, populateFunctionName, out var function))
            {
                var method = function.CompiledMethod;
                return UpdateDependencySubscriptions(function);
            }
            return false;
        }

        public bool UpdateDependencySubscriptions(PluginFunction function)
        {
            if (!function.IsSyntaxTreeValid)
            {
                NeedsUpdateDependencySubscriptionsToBeCalled = true;
                return false;
            }
            CellPopulateManager.UnsubscribeFromAllLocationUpdates(this);
            for (int i = 0; i < function.SheetDependencies.Count; i++)
            {
                var sheetName = function.SheetDependencies[i];
                var row = function.RowDependencies[i];
                var column = function.ColumnDependencies[i];
                sheetName = string.IsNullOrWhiteSpace(sheetName) ? SheetName : sheetName;
                CellPopulateManager.SubscribeToUpdatesAtLocation(this, sheetName, row, column);
            }

            CellPopulateManager.UnsubscribeFromAllCollectionUpdates(this);
            foreach (var collectionName in function.CollectionDependencies)
            {
                CellPopulateManager.SubscribeToCollectionUpdates(this, collectionName);
            }
            NeedsUpdateDependencySubscriptionsToBeCalled = false;
            return true;
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
                if (currentValue == value) return;
                StringProperties[key] = value;
                OnPropertyChanged(nameof(StringProperties));
            }
            else StringProperties.Add(key, value);
            OnPropertyChanged(nameof(StringProperties));
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
            if (BooleanProperties.TryGetValue(key, out var currentValue))
            {
                if (currentValue == value) return;
                BooleanProperties[key] = value;
                OnPropertyChanged(nameof(BooleanProperties));
            }
            else BooleanProperties.Add(key, value);
            OnPropertyChanged(nameof(BooleanProperties));
        }

        #endregion

        #region Numeric Properties

        public Dictionary<string, double> NumericProperties
        {
            get { return numericProperties; }
            set { numericProperties = value; OnPropertyChanged(nameof(NumericProperties)); }
        }

        public static readonly CellModel Empty = new();

        private Dictionary<string, double> numericProperties = [];

        internal double GetNumericProperty(string key)
        {
            if (NumericProperties.TryGetValue(key, out var value)) return value;
            return 0;
        }

        internal void SetNumericProperty(string key, double value)
        {
            if (NumericProperties.TryGetValue(key, out var currentValue) && currentValue != value)
            {
                if (currentValue == value) return;
                NumericProperties[key] = value;
                OnPropertyChanged(nameof(NumericProperties));
            }
            else NumericProperties.Add(key, value);
            OnPropertyChanged(nameof(NumericProperties));
        }

        #endregion

        #region Serialization

        public static string SerializeModel(CellModel model) => JsonSerializer.Serialize(model);

        public static CellModel DeserializeModel(string serializedModel) => JsonSerializer.Deserialize<CellModel>(serializedModel) ?? throw new InvalidOperationException("DeserializeModel failed.");

        #endregion
    }
}