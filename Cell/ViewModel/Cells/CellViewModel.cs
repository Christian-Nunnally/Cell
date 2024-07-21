using Cell.Model;
using Cell.Persistence;
using Cell.Plugin;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace Cell.ViewModel
{
    public abstract partial class CellViewModel : PropertyChangedBase
    {
        protected SheetViewModel _sheetViewModel;
        private readonly CellModel _model;
        private bool isSelected;

        public CellViewModel(CellModel model, SheetViewModel sheet)
        {
            _sheetViewModel = sheet;
            _model = model;
            BackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BackgroundColorHex));
            ForegroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ForegroundColorHex));
            BorderColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BorderColorHex));
            UpdateBorderThickness(BorderThicknessString);
            _model.PropertyChanged += ModelPropertyChanged;
        }

        private void ModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellModel.Width) ||
                e.PropertyName == nameof(CellModel.Height) ||
                e.PropertyName == nameof(CellModel.Text) ||
                e.PropertyName == nameof(CellModel.Row) ||
                e.PropertyName == nameof(CellModel.Column))
            {
                OnPropertyChanged(e.PropertyName);
                return;
            }
        }

        public CellModel Model => _model;

        public virtual double X
        {
            get => _x;
            set { _x = value; OnPropertyChanged(nameof(X)); }
        }
        private double _x;

        public virtual double Y
        {
            get => _y;
            set { _y = value; OnPropertyChanged(nameof(Y)); }
        }
        private double _y;

        public virtual double Width
        {
            get => _model.Width;
            set { _model.Width = value; OnPropertyChanged(nameof(Width)); }
        }

        public virtual double Height
        {
            get => _model.Height;
            set { _model.Height = value; OnPropertyChanged(nameof(Height)); }
        }

        public virtual int Row
        {
            get => _model.Row;
            set { _model.Row = value; OnPropertyChanged(nameof(Row)); }
        }

        public virtual int Column
        {
            get => _model.Column;
            set { _model.Column = value; OnPropertyChanged(nameof(Column)); }
        }

        public virtual int Index
        {
            get => _model.Index;
            set { _model.Index = value; OnPropertyChanged(nameof(Index)); }
        }

        public virtual CellType CellType
        {
            get => _model.CellType;
            set { _model.CellType = value; OnPropertyChanged(nameof(CellType)); }
        }

        public virtual string Text
        {
            get => _model.Text;
            set
            {
                _model.Text = value?.Replace("\\n", "\n") ?? string.Empty;
                OnPropertyChanged(nameof(Text));
            }
        }

        public virtual string ID
        {
            get => _model.ID;
        }

        public virtual bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public virtual string PopulateFunctionName
        {
            get => _model.PopulateFunctionName;
            set
            {
                if (PluginFunctionLoader.GetOrCreateFunction(PluginFunctionLoader.PopulateFunctionsDirectoryName, value) is not null)
                {
                    _model.PopulateFunctionName = value;
                    OnPropertyChanged(nameof(PopulateFunctionName));
                    OnPropertyChanged(nameof(PopulateFunctionCode));
                }
            }
        }

        public virtual string PopulateFunctionCode
        {
            get => PluginFunctionLoader.TryGetFunction(PluginFunctionLoader.PopulateFunctionsDirectoryName, PopulateFunctionName, out var code) ? code.Code : string.Empty;
            set
            {
                PluginFunctionLoader.UpdateFunctionCode(PluginFunctionLoader.PopulateFunctionsDirectoryName, PopulateFunctionName, value);
                PopulateText();
                OnPropertyChanged(nameof(PopulateFunctionCode));
            }
        }

        public virtual string TriggerFunctionName
        {
            get => _model.TriggerFunctionName;
            set
            {
                if (PluginFunctionLoader.GetOrCreateFunction(PluginFunctionLoader.TriggerFunctionsDirectoryName, value) is not null)
                {
                    _model.TriggerFunctionName = value;
                    OnPropertyChanged(nameof(TriggerFunctionName));
                    OnPropertyChanged(nameof(TriggerFunctionCode));
                };
            }
        }

        public virtual string TriggerFunctionCode
        {
            get => PluginFunctionLoader.TryGetFunction(PluginFunctionLoader.TriggerFunctionsDirectoryName, TriggerFunctionName, out var code) ? code.Code : string.Empty;
            set
            {
                PluginFunctionLoader.UpdateFunctionCode(PluginFunctionLoader.TriggerFunctionsDirectoryName, TriggerFunctionName, value);
                OnPropertyChanged(nameof(TriggerFunctionCode));
            }
        }

        public void PopulateText()
        {
            if (PopulateFunctionName != string.Empty)
            {
                var result = DynamicCellPluginExecutor.RunPopulate(new PluginContext(ApplicationViewModel.Instance), _model);
                if (result.Success)
                {
                    Text = result.Result;
                }
                else
                {
                    Text = result.Result;
                }
            }
        }

        #region Style Properties

        public virtual double FontSize
        {
            get => _model.FontSize;
            set { _model.FontSize = value; OnPropertyChanged(nameof(FontSize)); }
        }

        public virtual string FontFamily
        {
            get => _model.FontFamily;
            set { _model.FontFamily = value; OnPropertyChanged(nameof(FontFamily)); }
        }

        public virtual bool IsFontBold
        {
            get => _model.IsFontBold;
            set { _model.IsFontBold = value; OnPropertyChanged(nameof(IsFontBold)); OnPropertyChanged(nameof(FontWeightForView)); }
        }

        public FontWeight FontWeightForView => IsFontBold ? FontWeights.Bold : FontWeights.Normal;

        public virtual bool IsFontItalic
        {
            get => _model.IsFontItalic;
            set { _model.IsFontItalic = value; OnPropertyChanged(nameof(IsFontItalic)); OnPropertyChanged(nameof(FontStyleForView)); }
        }

        public FontStyle FontStyleForView => IsFontItalic ? FontStyles.Italic : FontStyles.Normal;

        public virtual bool IsFontStrikethrough
        {
            get => _model.IsFontStrikethrough;
            set { _model.IsFontStrikethrough = value; OnPropertyChanged(nameof(IsFontStrikethrough)); OnPropertyChanged(nameof(TextDecorationsForView)); }
        }

        public TextDecorationCollection? TextDecorationsForView => IsFontStrikethrough ? TextDecorations.Strikethrough : null;

        public virtual HorizontalAlignment HorizontalAlignmentForViewCenter => HorizontalAlignmentForView == HorizontalAlignment.Stretch ? HorizontalAlignment.Center : HorizontalAlignmentForView;

        public virtual HorizontalAlignment HorizontalAlignmentForView
        {
            get => _model.HorizontalAlignment;
            set { _model.HorizontalAlignment = value; OnPropertyChanged(nameof(HorizontalAlignmentForView)); OnPropertyChanged(nameof(HorizontalAlignmentForViewCenter)); }
        }

        public virtual VerticalAlignment VerticalAlignmentForViewCenter => VerticalAlignmentForView == VerticalAlignment.Stretch ? VerticalAlignment.Center : VerticalAlignmentForView;

        public virtual VerticalAlignment VerticalAlignmentForView
        {
            get => _model.VerticalAlignment;
            set { _model.VerticalAlignment = value; OnPropertyChanged(nameof(VerticalAlignmentForView)); OnPropertyChanged(nameof(VerticalAlignmentForViewCenter)); }
        }

        public virtual TextAlignment TextAlignmentForView
        {
            get => _model.TextAlignmentForView;
            set { _model.TextAlignmentForView = value; OnPropertyChanged(nameof(TextAlignmentForView)); }
        }

        public virtual string BorderThicknessString
        { 
            get => Model.BorderThicknessString;
            set
            {
                if (UpdateBorderThickness(value))
                {
                    Model.BorderThicknessString = value;
                    OnPropertyChanged(nameof(BorderThicknessString));
                }
            }
        }

        public bool UpdateBorderThickness(string stringBorderThickness)
        {
            var split = stringBorderThickness.Split(',');
            if (split.Length == 1)
            {
                if (!double.TryParse(split[0], out var size)) return false;
                BorderThickness = new Thickness(size, size, size, size);
                OnPropertyChanged(nameof(BorderThickness));
                return true;
            }
            if (split.Length != 4) return false;
            if (!double.TryParse(split[0], out var left)) return false;
            if (!double.TryParse(split[1], out var top)) return false;
            if (!double.TryParse(split[2], out var right)) return false;
            if (!double.TryParse(split[3], out var bottom)) return false;
            BorderThickness = new Thickness(left, top, right, bottom);
            OnPropertyChanged(nameof(BorderThickness));
            return true;
        }

        public virtual Thickness BorderThickness { get; private set; }

        public virtual SolidColorBrush BackgroundColor { get; private set; }

        public void HighlightCell(string color)
        {
            BackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            OnPropertyChanged(nameof(BackgroundColor));
        }

        public void UnhighlightCell()
        {
            BackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BackgroundColorHex));
            OnPropertyChanged(nameof(BackgroundColor));
        }

        public virtual string BackgroundColorHex
        {
            get => _model.BackgroundColorHex;
            set
            {
                if (!IsHexidecimalColorCode().IsMatch(value)) return;
                _model.BackgroundColorHex = value;
                BackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BackgroundColorHex));
                OnPropertyChanged(nameof(BackgroundColorHex));
                OnPropertyChanged(nameof(BackgroundColor));
            }
        }

        public virtual SolidColorBrush ForegroundColor { get; private set; }

        public virtual string ForegroundColorHex
        {
            get => _model.ForegroundColorHex;
            set
            {
                if (!IsHexidecimalColorCode().IsMatch(value)) return;
                _model.ForegroundColorHex = value;
                ForegroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ForegroundColorHex));
                OnPropertyChanged(nameof(ForegroundColorHex));
                OnPropertyChanged(nameof(ForegroundColor));
            }
        }

        public virtual SolidColorBrush BorderColor { get; private set; }

        public virtual string BorderColorHex
        {
            get => _model.BorderColorHex;
            set
            {
                if (!IsHexidecimalColorCode().IsMatch(value)) return;
                _model.BorderColorHex = value;
                BorderColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BorderColorHex));
                OnPropertyChanged(nameof(BorderColorHex));
                OnPropertyChanged(nameof(BorderColor));
            }
        }

        [GeneratedRegex(@"[#][0-9A-Fa-f]{6}\b")]
        private static partial Regex IsHexidecimalColorCode();

        #endregion
    }
}