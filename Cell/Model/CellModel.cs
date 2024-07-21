using Cell.Common;
using Cell.Data;
using Cell.Persistence;
using Cell.Plugin;
using System.Text.Json.Serialization;
using System.Windows;

namespace Cell.Model
{
    public class CellModel : PropertyChangedBase
    {
        public event Action<CellModel, EditContext>? CellTriggered;
        public event Action<CellModel>? AfterCellEdited;
        
        public double Width
        {
            get => width;
            set { if (width != value) { width = value; NotifyPropertyChanged(nameof(Width)); } }
        }
        private double width;

        public double Height
        {
            get => height;
            set { if (height != value) { height = value; NotifyPropertyChanged(nameof(Height)); } }
        }
        private double height;

        public CellType CellType
        {
            get => cellType;
            set { if (cellType != value) { cellType = value; NotifyPropertyChanged(nameof(CellType)); } }
        }
        private CellType cellType = CellType.None;

        public string ID
        {
            get => id;
            set { if (id != value) { id = value; NotifyPropertyChanged(nameof(ID)); } }
        }
        private string id = Utilities.GenerateUnqiueId(12);

        public int Column
        {
            get => column;
            set { if (column != value) { column = value; NotifyPropertyChanged(nameof(Column)); } }
        }
        private int column;

        public int Row
        {
            get => row;
            set { if (row != value) { row = value; NotifyPropertyChanged(nameof(Row)); } }
        }
        private int row;

        public string SheetName
        {
            get => sheetName;
            set { if (sheetName != value) { sheetName = value; NotifyPropertyChanged(nameof(SheetName)); } }
        }
        private string sheetName = string.Empty;

        public string MergedWith
        {
            get => mergedWith;
            set { if (mergedWith != value) { mergedWith = value; NotifyPropertyChanged(nameof(MergedWith)); } }
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
                NotifyPropertyChanged(nameof(Text));
                CellTriggered?.Invoke(this, new EditContext(nameof(Text), text, oldValue));
                // TODO: do we want to run populate after on edit?
                AfterCellEdited?.Invoke(this);
            }
        }
        private string text = string.Empty;

        [JsonIgnore]
        public string ErrorText
        {
            get { return errorText; }
            set { if (errorText != value) { errorText = value; NotifyPropertyChanged(nameof(ErrorText)); } }
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
            set { if (index != value) { index = value; NotifyPropertyChanged(nameof(Index)); } }
        }
        private int index = 0;

        public string BackgroundColorHex
        {
            get { return backgroundColorHex; }
            set { if (backgroundColorHex == value) return; backgroundColorHex = value; NotifyPropertyChanged(nameof(BackgroundColorHex)); }
        }
        private string backgroundColorHex = "#1e1e1e";

        public string ForegroundColorHex
        {
            get { return foregroundColorHex; }
            set { if (foregroundColorHex == value) return; foregroundColorHex = value; NotifyPropertyChanged(nameof(ForegroundColorHex)); }
        }
        private string foregroundColorHex = "#ffffff";

        public string BorderColorHex
        {
            get { return borderColorHex; }
            set { if (borderColorHex == value) return; borderColorHex = value; NotifyPropertyChanged(nameof(BorderColorHex)); }
        }
        private string borderColorHex = "#2d2d30";

        public string BorderThicknessString
        {
            get { return borderThickness; }
            set { if (borderThickness == value) return; borderThickness = value; NotifyPropertyChanged(nameof(BorderThicknessString)); }
        }
        private string borderThickness = "1,1,1,1";

        public double FontSize
        {
            get { return fontSize; }
            set { if (fontSize == value) return; fontSize = value; NotifyPropertyChanged(nameof(FontSize)); }
        }
        private double fontSize = 10;

        public string FontFamily
        {
            get { return font; }
            set { if (font == value) return; font = value; NotifyPropertyChanged(nameof(FontFamily)); }
        }
        private string font = "Consolas";

        public bool IsFontBold
        {
            get { return isFontBold; }
            set { if (isFontBold == value) return; isFontBold = value; NotifyPropertyChanged(nameof(IsFontBold)); }
        }
        private bool isFontBold = false;

        public bool IsFontItalic
        {
            get { return isFontItalic; }
            set { if (isFontItalic == value) return; isFontItalic = value; NotifyPropertyChanged(nameof(IsFontItalic)); }
        }
        private bool isFontItalic = false;

        public bool IsFontStrikethrough
        {
            get { return isFontStrikethrough; }
            set { if (isFontStrikethrough == value) return; isFontStrikethrough = value; NotifyPropertyChanged(nameof(IsFontStrikethrough)); }
        }
        private bool isFontStrikethrough = false;

        public HorizontalAlignment HorizontalAlignment
        {
            get { return horizontalAlignment; }
            set { if (horizontalAlignment == value) return; horizontalAlignment = value; NotifyPropertyChanged(nameof(HorizontalAlignment)); }
        }
        private HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center;

        public VerticalAlignment VerticalAlignment
        {
            get { return verticalAlignment; }
            set { if (verticalAlignment == value) return; verticalAlignment = value; NotifyPropertyChanged(nameof(VerticalAlignment)); }
        }
        private VerticalAlignment verticalAlignment = VerticalAlignment.Center;

        public TextAlignment TextAlignmentForView
        {
            get { return textAlignment; }
            set { if (textAlignment == value) return; textAlignment = value; NotifyPropertyChanged(nameof(TextAlignment)); }
        }
        private TextAlignment textAlignment = TextAlignment.Center;

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
                    var _ = function2.CompiledMethod;
                    UpdateDependencySubscriptions(function2);
                }
                NotifyPropertyChanged(nameof(PopulateFunctionName));
            }
        }
        private string populateFunctionName = string.Empty;

        public string TriggerFunctionName
        {
            get { return triggerFunctionName; }
            set { triggerFunctionName = value; NotifyPropertyChanged(nameof(TriggerFunctionName)); }
        }
        private string triggerFunctionName = string.Empty;

        public void UpdateDependencySubscriptions(PluginFunction function)
        {
            if (!function.IsSyntaxTreeValid) throw new InvalidOperationException("Cannot update dependency subscriptions for a function with invalid syntax tree.");
            CellPopulateManager.UnsubscribeFromAllLocationUpdates(this);
            foreach (var locationDependency in function.LocationDependencies)
            {
                var sheetName = string.IsNullOrWhiteSpace(locationDependency.SheetName) ? SheetName : locationDependency.SheetName;
                CellPopulateManager.SubscribeToUpdatesAtLocation(this, sheetName, locationDependency.Row, locationDependency.Column);
            }

            CellPopulateManager.UnsubscribeFromAllCollectionUpdates(this);
            foreach (var collectionName in function.CollectionDependencies)
            {
                CellPopulateManager.SubscribeToCollectionUpdates(this, collectionName);
            }
        }

        public Dictionary<string, string> StringProperties { get; set; } = [];

        internal string GetStringProperty(string key) => StringProperties.TryGetValue(key, out var value) ? value : string.Empty;

        internal void SetStringProperty(string key, string value)
        {
            if (StringProperties.TryGetValue(key, out var currentValue))
            {
                if (currentValue == value) return;
                StringProperties[key] = value;
            }
            else StringProperties.Add(key, value);
            NotifyPropertyChanged(nameof(StringProperties));
        }

        public Dictionary<string, bool> BooleanProperties { get; set; } = [];

        internal bool GetBooleanProperty(string key) => BooleanProperties.TryGetValue(key, out var value) ? value : false;

        internal void SetBooleanProperty(string key, bool value)
        {
            if (BooleanProperties.TryGetValue(key, out var currentValue))
            {
                if (currentValue == value) return;
                BooleanProperties[key] = value;
            }
            else BooleanProperties.Add(key, value);
            NotifyPropertyChanged(nameof(BooleanProperties));
        }

        public Dictionary<string, double> NumericProperties { get; set; } = [];

        internal double GetNumericProperty(string key) => NumericProperties.TryGetValue(key, out var value) ? value : 0;

        internal void SetNumericProperty(string key, double value)
        {
            if (NumericProperties.TryGetValue(key, out var currentValue) && currentValue != value)
            {
                if (currentValue == value) return;
                NumericProperties[key] = value;
            }
            else NumericProperties.Add(key, value);
            NotifyPropertyChanged(nameof(NumericProperties));
        }

        internal void TriggerCellEdited(EditContext editContext) => CellTriggered?.Invoke(this, editContext);

        public int CellsMergedToRight
        {
            get
            {
                var count = 0;
                while (!string.IsNullOrWhiteSpace(MergedWith) && Cells.GetCell(SheetName, Row, Column + 1 + count)?.MergedWith == MergedWith) count++;
                return count;
            }
        }

        public int CellsMergedBelow
        {
            get
            {
                var count = 0;
                while (!string.IsNullOrWhiteSpace(MergedWith) && Cells.GetCell(SheetName, Row + 1 + count, Column)?.MergedWith == MergedWith) count++;
                return count;
            }
        }

        public static readonly CellModel Empty = new();
    }
}