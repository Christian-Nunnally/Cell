using Cell.Common;
using Cell.Execution;
using Cell.Persistence;
using Cell.View.Skin;
using Cell.ViewModel.Application;
using Cell.ViewModel.Cells.Types;
using Cell.ViewModel.Cells.Types.Special;
using Cell.ViewModel.Execution;
using System.Text.Json.Serialization;
using System.Windows;

namespace Cell.Model
{
    public class CellModel : PropertyChangedBase
    {
        public static readonly CellModel Empty = new();
        private string borderThickness = "1";
        private CellType cellType = CellType.None;
        private string[] colorHexes = [
            ColorConstants.BackgroundColorConstantHex,
            ColorConstants.BorderColorConstantHex,
            ColorConstants.ControlBackgroundColorConstantHex,
            ColorConstants.BorderColorConstantHex,
            ColorConstants.ForegroundColorConstantHex,
            ColorConstants.AccentColorConstantHex];
        private int column;
        private string contentBorderThickness = "1";
        private string errorText = string.Empty;
        private string font = "Consolas";
        private double fontSize = 10;
        private double height;
        private HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center;
        private string id = Utilities.GenerateUnqiueId(12);
        private int index = 0;
        private bool isFontBold = false;
        private bool isFontItalic = false;
        private bool isFontStrikethrough = false;
        private string margin = "0";
        private string mergedWith = string.Empty;
        private string populateFunctionName = string.Empty;
        private int row;
        private string sheetName = string.Empty;
        private string text = string.Empty;
        private TextAlignment textAlignment = TextAlignment.Center;
        private string triggerFunctionName = string.Empty;
        private VerticalAlignment verticalAlignment = VerticalAlignment.Center;
        private double width;
        public event Action<CellModel>? AfterCellEdited;

        public event Action<CellModel, EditContext>? CellTriggered;

        public Dictionary<string, bool> BooleanProperties { get; set; } = [];

        public string BorderThicknessString
        {
            get { return borderThickness; }
            set { if (borderThickness == value) return; borderThickness = value; NotifyPropertyChanged(nameof(BorderThicknessString)); }
        }

        public CellType CellType
        {
            get => cellType;
            set { if (cellType != value) { cellType = value; NotifyPropertyChanged(nameof(CellType)); } }
        }

        public string[] ColorHexes
        {
            get { return colorHexes; }
            set { if (colorHexes == value) return; colorHexes = value; NotifyPropertyChanged(nameof(ColorHexes)); }
        }

        public int Column
        {
            get => column;
            set { if (column != value) { column = value; NotifyPropertyChanged(nameof(Column)); } }
        }

        public string ContentBorderThicknessString
        {
            get { return contentBorderThickness; }
            set { if (contentBorderThickness == value) return; contentBorderThickness = value; NotifyPropertyChanged(nameof(ContentBorderThicknessString)); }
        }

        [JsonIgnore]
        public DateTime Date { get => DateTime.TryParse(Text, out var value) ? value : DateTime.MinValue; set => Text = value.ToString(); }

        [JsonIgnore]
        public string ErrorText
        {
            get { return errorText; }
            set { if (errorText != value) { errorText = value; NotifyPropertyChanged(nameof(ErrorText)); } }
        }

        public string FontFamily
        {
            get { return font; }
            set { if (font == value) return; font = value; NotifyPropertyChanged(nameof(FontFamily)); }
        }

        public double FontSize
        {
            get { return fontSize; }
            set { if (fontSize == value) return; fontSize = value; NotifyPropertyChanged(nameof(FontSize)); }
        }

        public double Height
        {
            get => height;
            set { if (height != value) { height = value; NotifyPropertyChanged(nameof(Height)); } }
        }

        public HorizontalAlignment HorizontalAlignment
        {
            get { return horizontalAlignment; }
            set { if (horizontalAlignment == value) return; horizontalAlignment = value; NotifyPropertyChanged(nameof(HorizontalAlignment)); }
        }

        public string ID
        {
            get => id;
            set { if (id != null) { id = value; NotifyPropertyChanged(nameof(ID)); } }
        }

        public int Index
        {
            get { return index; }
            set { if (index != value) { index = value; NotifyPropertyChanged(nameof(Index)); } }
        }

        public bool IsFontBold
        {
            get { return isFontBold; }
            set { if (isFontBold == value) return; isFontBold = value; NotifyPropertyChanged(nameof(IsFontBold)); }
        }

        public bool IsFontItalic
        {
            get { return isFontItalic; }
            set { if (isFontItalic == value) return; isFontItalic = value; NotifyPropertyChanged(nameof(IsFontItalic)); }
        }

        public bool IsFontStrikethrough
        {
            get { return isFontStrikethrough; }
            set { if (isFontStrikethrough == value) return; isFontStrikethrough = value; NotifyPropertyChanged(nameof(IsFontStrikethrough)); }
        }

        public string MarginString
        {
            get { return margin; }
            set { if (margin == value) return; margin = value; NotifyPropertyChanged(nameof(MarginString)); }
        }

        public string MergedWith
        {
            get => mergedWith;
            set { if (mergedWith != value) { mergedWith = value; NotifyPropertyChanged(nameof(MergedWith)); } }
        }

        public Dictionary<string, double> NumericProperties { get; set; } = [];

        public string PopulateFunctionName
        {
            get { return populateFunctionName; }
            set
            {
                if (populateFunctionName == value) return;
                if (PluginFunctionLoader.TryGetFunction("object", populateFunctionName, out var function))
                {
                    function.StopListeningForDependencyChanges(this);
                }
                populateFunctionName = value;
                if (PluginFunctionLoader.TryGetFunction("object", populateFunctionName, out var function2))
                {
                    function2.StartListeningForDependencyChanges(this);
                    var _ = function2.CompiledMethod;
                    UpdateDependencySubscriptions(function2);
                }
                NotifyPropertyChanged(nameof(PopulateFunctionName));
            }
        }

        public int Row
        {
            get => row;
            set { if (row != value) { row = value; NotifyPropertyChanged(nameof(Row)); } }
        }

        public string SelectedItem => GetStringProperty(nameof(ListCellViewModel.SelectedItem));

        public string SheetName
        {
            get => sheetName;
            set 
            {
                if (sheetName == value) return;
                sheetName = value; 
                NotifyPropertyChanged(nameof(SheetName));

                if (PluginFunctionLoader.TryGetFunction("object", populateFunctionName, out var function1))
                {
                    var _ = function1.CompiledMethod;
                    UpdateDependencySubscriptions(function1);
                }

                if (PluginFunctionLoader.TryGetFunction("object", triggerFunctionName, out var function2))
                {
                    var _ = function2.CompiledMethod;
                    UpdateDependencySubscriptions(function2);
                }
            }
        }

        public Dictionary<string, string> StringProperties { get; set; } = [];

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
                AfterCellEdited?.Invoke(this);
            }
        }

        public TextAlignment TextAlignmentForView
        {
            get { return textAlignment; }
            set { if (textAlignment == value) return; textAlignment = value; NotifyPropertyChanged(nameof(TextAlignment)); }
        }

        public string TriggerFunctionName
        {
            get { return triggerFunctionName; }
            set
            {
                if (triggerFunctionName == value) return;
                if (PluginFunctionLoader.TryGetFunction("void", triggerFunctionName, out var function))
                {
                    function.StopListeningForDependencyChanges(this);
                }
                triggerFunctionName = value;
                if (PluginFunctionLoader.TryGetFunction("void", triggerFunctionName, out var function2))
                {
                    function2.StartListeningForDependencyChanges(this);
                    var _ = function2.CompiledMethod;
                    UpdateDependencySubscriptions(function2);
                }
                NotifyPropertyChanged(nameof(TriggerFunctionName));
            }
        }

        public string UserFriendlyCellName => $"{ColumnCellViewModel.GetColumnName(Column)}{Row}";

        [JsonIgnore]
        public double Value { get => double.TryParse(Text, out var value) ? value : 0; set => Text = value.ToString(); }

        public VerticalAlignment VerticalAlignment
        {
            get { return verticalAlignment; }
            set { if (verticalAlignment == value) return; verticalAlignment = value; NotifyPropertyChanged(nameof(VerticalAlignment)); }
        }

        public double Width
        {
            get => width;
            set { if (width != value) { width = value; NotifyPropertyChanged(nameof(Width)); } }
        }

        public bool GetBooleanProperty(string key) => BooleanProperties.TryGetValue(key, out var value) && value;

        public double GetNumericProperty(string key, double defaultValue = 0) => NumericProperties.TryGetValue(key, out var value) ? value : defaultValue;

        public string GetStringProperty(string key) => StringProperties.TryGetValue(key, out var value) ? value : string.Empty;

        public void PopulateText()
        {
            if (string.IsNullOrEmpty(PopulateFunctionName)) return;
            var result = DynamicCellPluginExecutor.RunPopulate(new PluginContext(ApplicationViewModel.Instance, Index), this);
            if (result.Result == null) return;
            if (result.Success) Text = result.Result;
            else ErrorText = result.Result;
        }

        public void SetBackground(string color)
        {
            ColorHexes[(int)ColorFor.Background] = color;
            NotifyPropertyChanged(nameof(ColorHexes));
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

        public void SetBorder(string color)
        {
            if (!Utilities.IsHexidecimalColorCode().IsMatch(color)) return;
            if (ColorHexes[(int)ColorFor.Border] == color) return;
            ColorHexes[(int)ColorFor.Border] = color;
            NotifyPropertyChanged(nameof(ColorHexes));
        }

        public void SetContentBackground(string color)
        {
            if (!Utilities.IsHexidecimalColorCode().IsMatch(color)) return;
            if (ColorHexes[(int)ColorFor.ContentBackground] == color) return;
            ColorHexes[(int)ColorFor.ContentBackground] = color;
            NotifyPropertyChanged(nameof(ColorHexes));
        }

        public void SetContentBorder(string color)
        {
            if (!Utilities.IsHexidecimalColorCode().IsMatch(color)) return;
            if (ColorHexes[(int)ColorFor.ContentBorder] == color) return;
            ColorHexes[(int)ColorFor.ContentBorder] = color;
            NotifyPropertyChanged(nameof(ColorHexes));
        }

        public void SetContentHighlight(string color)
        {
            if (!Utilities.IsHexidecimalColorCode().IsMatch(color)) return;
            if (ColorHexes[(int)ColorFor.ContentHighlight] == color) return;
            ColorHexes[(int)ColorFor.ContentHighlight] = color;
            NotifyPropertyChanged(nameof(ColorHexes));
        }

        public void SetForeground(string color)
        {
            if (!Utilities.IsHexidecimalColorCode().IsMatch(color)) return;
            if (ColorHexes[(int)ColorFor.Foreground] == color) return;
            ColorHexes[(int)ColorFor.Foreground] = color;
            NotifyPropertyChanged(nameof(ColorHexes));
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

        public void TriggerCellEdited(EditContext editContext) => CellTriggered?.Invoke(this, editContext);

        public void UpdateDependencySubscriptions(FunctionViewModel function)
        {
            if (function.Model.ReturnType == "void") return;
            if (string.IsNullOrWhiteSpace(SheetName)) return;

            CellPopulateManager.UnsubscribeFromAllLocationUpdates(this);
            CellPopulateManager.UnsubscribeFromAllCollectionUpdates(this);
            if (!string.IsNullOrWhiteSpace(function.Model.Code))
            {
                foreach (var locationDependency in function.LocationDependencies)
                {
                    var sheetName = string.IsNullOrWhiteSpace(locationDependency.SheetName) ? SheetName : locationDependency.SheetName;

                    var row = locationDependency.ResolveRow(this);
                    var column = locationDependency.ResolveColumn(this);
                    if (row == Row && column == Column) continue;
                    if (locationDependency.IsRange)
                    {
                        var rowRangeEnd = locationDependency.ResolveRowRangeEnd(this);
                        var columnRangeEnd = locationDependency.ResolveColumnRangeEnd(this);
                        for (var r = row; r <= rowRangeEnd; r++)
                        {
                            for (var c = column; c <= columnRangeEnd; c++)
                            {
                                CellPopulateManager.SubscribeToUpdatesAtLocation(this, sheetName, r, c);
                            }
                        }
                    }
                    else
                    {
                        CellPopulateManager.SubscribeToUpdatesAtLocation(this, sheetName, row, column);
                    }
                }
                CellPopulateManager.SubscribeToUpdatesAtLocation(this, SheetName, Row, Column);
                foreach (var collectionName in function.CollectionDependencies)
                {
                    CellPopulateManager.SubscribeToCollectionUpdates(this, collectionName);
                }
            }
        }
    }
}
