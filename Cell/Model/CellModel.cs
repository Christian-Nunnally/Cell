using Cell.Common;
using Cell.Execution;
using Cell.View.Skin;
using Cell.ViewModel.Application;
using Cell.ViewModel.Cells.Types;
using Cell.ViewModel.Cells.Types.Special;
using System.Text.Json.Serialization;
using System.Windows;

namespace Cell.Model
{
    public class CellModel : PropertyChangedBase
    {
        public static readonly CellModel Null = new();
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
            get => colorHexes;
            set { if (colorHexes == value) return; colorHexes = value; NotifyPropertyChanged(nameof(ColorHexes)); }
        }

        public int Column
        {
            get => column;
            set { if (column != value) { column = value; NotifyPropertyChanged(nameof(Column)); } }
        }

        public string ContentBorderThicknessString
        {
            get => contentBorderThickness;
            set { if (contentBorderThickness == value) return; contentBorderThickness = value; NotifyPropertyChanged(nameof(ContentBorderThicknessString)); }
        }

        [JsonIgnore]
        public DateTime Date { get => DateTime.TryParse(Text, out var value) ? value : DateTime.MinValue; set => Text = value.ToString(); }

        [JsonIgnore]
        public string ErrorText
        {
            get => errorText;
            set { if (errorText != value) { errorText = value; NotifyPropertyChanged(nameof(ErrorText)); } }
        }

        public string FontFamily
        {
            get => font;
            set { if (font == value) return; font = value; NotifyPropertyChanged(nameof(FontFamily)); }
        }

        public double FontSize
        {
            get => fontSize;
            set { if (fontSize == value) return; fontSize = value; NotifyPropertyChanged(nameof(FontSize)); }
        }

        public double Height
        {
            get => height;
            set { if (height != value) { height = value; NotifyPropertyChanged(nameof(Height)); } }
        }

        public HorizontalAlignment HorizontalAlignment
        {
            get => horizontalAlignment;
            set { if (horizontalAlignment == value) return; horizontalAlignment = value; NotifyPropertyChanged(nameof(HorizontalAlignment)); }
        }

        public string ID
        {
            get => id;
            set { if (id != null) { id = value; NotifyPropertyChanged(nameof(ID)); } }
        }

        public int Index
        {
            get => index;
            set { if (index != value) { index = value; NotifyPropertyChanged(nameof(Index)); } }
        }

        public bool IsFontBold
        {
            get => isFontBold;
            set { if (isFontBold == value) return; isFontBold = value; NotifyPropertyChanged(nameof(IsFontBold)); }
        }

        public bool IsFontItalic
        {
            get => isFontItalic;
            set { if (isFontItalic == value) return; isFontItalic = value; NotifyPropertyChanged(nameof(IsFontItalic)); }
        }

        public bool IsFontStrikethrough
        {
            get => isFontStrikethrough;
            set { if (isFontStrikethrough == value) return; isFontStrikethrough = value; NotifyPropertyChanged(nameof(IsFontStrikethrough)); }
        }

        public string MarginString
        {
            get => margin;
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
            get => populateFunctionName;
            set
            {
                if (populateFunctionName == value) return;
                populateFunctionName = value;
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
            }
        }

        public Dictionary<string, string> StringProperties { get; set; } = [];

        public string Text
        {
            get => text;
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
            get => textAlignment;
            set { if (textAlignment == value) return; textAlignment = value; NotifyPropertyChanged(nameof(TextAlignment)); }
        }

        public string TriggerFunctionName
        {
            get => triggerFunctionName;
            set
            {
                if (triggerFunctionName == value) return;
                triggerFunctionName = value;
                NotifyPropertyChanged(nameof(TriggerFunctionName));
            }
        }

        public string UserFriendlyCellName => $"{ColumnCellViewModel.GetColumnName(Column)}{Row}";

        [JsonIgnore]
        public double Value { get => double.TryParse(Text, out var value) ? value : 0; set => Text = value.ToString(); }

        public VerticalAlignment VerticalAlignment
        {
            get => verticalAlignment;
            set { if (verticalAlignment == value) return; verticalAlignment = value; NotifyPropertyChanged(nameof(VerticalAlignment)); }
        }

        public double Width
        {
            get => width;
            set { if (width != value) { width = value; NotifyPropertyChanged(nameof(Width)); } }
        }

        public bool GetBooleanProperty(string key) => BooleanProperties.TryGetValue(key, out var value) && value;

        public bool GetBooleanProperty(string key, bool defaultValue)
        {
            return BooleanProperties.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public double GetNumericProperty(string key, double defaultValue = 0) => NumericProperties.TryGetValue(key, out var value) ? value : defaultValue;

        public string GetStringProperty(string key) => StringProperties.TryGetValue(key, out var value) ? value : string.Empty;

        public void PopulateText()
        {
            if (string.IsNullOrEmpty(PopulateFunctionName)) return;
            var result = DynamicCellPluginExecutor.RunPopulate(ApplicationViewModel.Instance.PluginFunctionLoader, new PluginContext(ApplicationViewModel.Instance.CellTracker, ApplicationViewModel.Instance.UserCollectionLoader, Index), this);
            if (result.Result == null) return;
            if (result.Success) Text = result.Result;
            else ErrorText = result.Result;
        }

        public void SetBackground(string color)
        {
            ColorHexes[(int)ColorFor.Background] = color;
            NotifyPropertyChanged(nameof(ColorHexes));
        }

        public void SetBackgrounds(string color)
        {
            SetBackground(color);
            SetContentBackground(color);
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

        public void SetBorders(string color)
        {
            SetBorder(color);
            SetContentBorder(color);
        }

        public void SetColor(string color)
        {
            SetBackgrounds(color);
            SetBorders(color);
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
    }
}
