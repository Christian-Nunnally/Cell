using Cell.Common;
using Cell.Model;
using Cell.Persistence;
using Cell.Plugin;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;

namespace Cell.ViewModel
{
    public abstract class CellViewModel : PropertyChangedBase
    {
        protected SheetViewModel _sheetViewModel;
        private readonly CellModel _model;

        public CellViewModel(CellModel model, SheetViewModel sheet)
        {
            _sheetViewModel = sheet;
            _model = model;

            //var randomDouble = new Random().NextDouble();
            //var randomDouble2 = new Random().NextDouble();
            //LinearGradientBrush linear = new LinearGradientBrush();
            //linear.ColorInterpolationMode = ColorInterpolationMode.SRgbLinearInterpolation ;
            //linear.StartPoint = new Point(0, 0);
            //linear.EndPoint = new Point(randomDouble, 1- randomDouble);
            //linear.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#9678b5"), 0.0));
            //linear.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#b272a1"), 0.09));
            //linear.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#c17188"), 0.18));
            //linear.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#c3776f"), 0.27));
            //linear.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#b8825c"), 0.36));
            //linear.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#a48f54"), 0.45));
            //linear.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#8a9b5c"), 0.54));
            //linear.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#6da471"), 0.63));
            //linear.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#50aa8f"), 0.72));
            //linear.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#3dadaf"), 0.81));
            //linear.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#4aadca"), 0.9));
            //linear.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#6fa9dc"), 1.0));

            //BackgroundColor = linear;
            BackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BackgroundColorHex));
            ContentBackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ContentBackgroundColorHex));
            ForegroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ForegroundColorHex));
            BorderColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BorderColorHex));
            ContentBorderColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ContentBorderColorHex));
            ContentHighlightColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ContentHighlightColorHex));
            UpdateBorderThickness(BorderThicknessString);
            UpdateContentBorderThickness(ContentBorderThicknessString);
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
                NotifyPropertyChanged(e.PropertyName);
                return;
            }
            else if (e.PropertyName == nameof(CellModel.ColorHexes))
            {
                BackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BackgroundColorHex));
                ContentBackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ContentBackgroundColorHex));
                ForegroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ForegroundColorHex));
                BorderColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BorderColorHex));
                ContentBorderColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ContentBorderColorHex));
                ContentHighlightColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ContentHighlightColorHex));
                NotifyPropertyChanged(nameof(BackgroundColor));
                NotifyPropertyChanged(nameof(ContentBackgroundColor));
                NotifyPropertyChanged(nameof(ForegroundColor));
                NotifyPropertyChanged(nameof(BorderColor));
                NotifyPropertyChanged(nameof(ContentBorderColor));
                NotifyPropertyChanged(nameof(ContentHighlightColor));
            }
        }

        public CellModel Model => _model;

        public virtual double X
        {
            get => _x;
            set { _x = value; NotifyPropertyChanged(nameof(X)); }
        }
        private double _x;

        public virtual double Y
        {
            get => _y;
            set { _y = value; NotifyPropertyChanged(nameof(Y)); }
        }
        private double _y;

        public virtual double Width
        {
            get => _model.Width;
            set { _model.Width = value; NotifyPropertyChanged(nameof(Width)); }
        }

        public virtual double Height
        {
            get => _model.Height;
            set { _model.Height = value; NotifyPropertyChanged(nameof(Height)); }
        }

        public virtual int Row
        {
            get => _model.Row;
            set { _model.Row = value; NotifyPropertyChanged(nameof(Row)); }
        }

        public virtual int Column
        {
            get => _model.Column;
            set { _model.Column = value; NotifyPropertyChanged(nameof(Column)); }
        }

        public virtual int Index
        {
            get => _model.Index;
            set { _model.Index = value; NotifyPropertyChanged(nameof(Index)); }
        }

        public virtual CellType CellType
        {
            get => _model.CellType;
            set { _model.CellType = value; NotifyPropertyChanged(nameof(CellType)); }
        }

        public virtual string Text
        {
            get => _model.Text;
            set
            {
                _model.Text = value?.Replace("\\n", "\n") ?? string.Empty;
                NotifyPropertyChanged(nameof(Text));
            }
        }

        public virtual string ID
        {
            get => _model.ID;
        }

        public virtual bool IsSelected
        {
            get => isSelected;
            set { if (isSelected == value) return; isSelected = value; NotifyPropertyChanged(nameof(IsSelected), nameof(ShouldShowSelectionBorder)); }
        }
        private bool isSelected;

        public virtual bool IsHighlighted
        {
            get => isHighlighted;
            set { if (isHighlighted == value) return; isHighlighted = value; NotifyPropertyChanged(nameof(IsHighlighted), nameof(ShouldShowSelectionBorder)); }
        }
        private bool isHighlighted;

        public virtual bool ShouldShowSelectionBorder => IsSelected || IsHighlighted;

        public virtual SolidColorBrush SelectionColor
        {
            get => selectionColor;
            set { if (selectionColor == value) return; selectionColor = value; NotifyPropertyChanged(nameof(SelectionColor)); }
        }
        private SolidColorBrush selectionColor = new((Color)ColorConverter.ConvertFromString("#66666666"));

        public virtual SolidColorBrush SelectionBorderColor
        {
            get => selectionBorderColor;
            set { if (selectionBorderColor == value) return; selectionBorderColor = value; NotifyPropertyChanged(nameof(SelectionBorderColor)); }
        }
        private SolidColorBrush selectionBorderColor = new((Color)ColorConverter.ConvertFromString("#66666666"));

        public virtual string PopulateFunctionName
        {
            get => _model.PopulateFunctionName;
            set
            {
                if (PluginFunctionLoader.GetOrCreateFunction("object", value) is not null)
                {
                    _model.PopulateFunctionName = value;
                    NotifyPropertyChanged(nameof(PopulateFunctionName));
                    PopulateText();
                }
            }
        }

        public virtual string TriggerFunctionName
        {
            get => _model.TriggerFunctionName;
            set
            {
                if (PluginFunctionLoader.GetOrCreateFunction("void", value) is not null)
                {
                    _model.TriggerFunctionName = value;
                    NotifyPropertyChanged(nameof(TriggerFunctionName));
                };
            }
        }

        public void PopulateText()
        {
            if (string.IsNullOrEmpty(PopulateFunctionName)) return;
            var result = DynamicCellPluginExecutor.RunPopulate(new PluginContext(ApplicationViewModel.Instance, _model.Index), _model);
            if (result.Success) Text = result.Result;
            else Model.ErrorText = result.Result;
        }

        #region Style Properties

        public virtual double FontSize
        {
            get => _model.FontSize;
            set { _model.FontSize = value; NotifyPropertyChanged(nameof(FontSize)); }
        }

        public virtual string FontFamily
        {
            get => _model.FontFamily;
            set { _model.FontFamily = value; NotifyPropertyChanged(nameof(FontFamily)); }
        }

        public virtual bool IsFontBold
        {
            get => _model.IsFontBold;
            set { _model.IsFontBold = value; NotifyPropertyChanged(nameof(IsFontBold)); NotifyPropertyChanged(nameof(FontWeightForView)); }
        }

        public FontWeight FontWeightForView => IsFontBold ? FontWeights.Bold : FontWeights.Normal;

        public virtual bool IsFontItalic
        {
            get => _model.IsFontItalic;
            set { _model.IsFontItalic = value; NotifyPropertyChanged(nameof(IsFontItalic)); NotifyPropertyChanged(nameof(FontStyleForView)); }
        }

        public FontStyle FontStyleForView => IsFontItalic ? FontStyles.Italic : FontStyles.Normal;

        public virtual bool IsFontStrikethrough
        {
            get => _model.IsFontStrikethrough;
            set { _model.IsFontStrikethrough = value; NotifyPropertyChanged(nameof(IsFontStrikethrough)); NotifyPropertyChanged(nameof(TextDecorationsForView)); }
        }

        public TextDecorationCollection? TextDecorationsForView => IsFontStrikethrough ? TextDecorations.Strikethrough : null;

        public virtual HorizontalAlignment HorizontalAlignmentForViewCenter => HorizontalAlignmentForView == HorizontalAlignment.Stretch ? HorizontalAlignment.Center : HorizontalAlignmentForView;

        public virtual HorizontalAlignment HorizontalAlignmentForView
        {
            get => _model.HorizontalAlignment;
            set { _model.HorizontalAlignment = value; NotifyPropertyChanged(nameof(HorizontalAlignmentForView)); NotifyPropertyChanged(nameof(HorizontalAlignmentForViewCenter)); }
        }

        public virtual VerticalAlignment VerticalAlignmentForViewCenter => VerticalAlignmentForView == VerticalAlignment.Stretch ? VerticalAlignment.Center : VerticalAlignmentForView;

        public virtual VerticalAlignment VerticalAlignmentForView
        {
            get => _model.VerticalAlignment;
            set { _model.VerticalAlignment = value; NotifyPropertyChanged(nameof(VerticalAlignmentForView)); NotifyPropertyChanged(nameof(VerticalAlignmentForViewCenter)); }
        }

        public virtual TextAlignment TextAlignmentForView
        {
            get => _model.TextAlignmentForView;
            set { _model.TextAlignmentForView = value; NotifyPropertyChanged(nameof(TextAlignmentForView)); }
        }

        public virtual string BorderThicknessString
        { 
            get => Model.BorderThicknessString;
            set
            {
                if (UpdateBorderThickness(value))
                {
                    Model.BorderThicknessString = value;
                    NotifyPropertyChanged(nameof(BorderThicknessString));
                }
            }
        }

        public bool UpdateBorderThickness(string stringBorderThickness)
        {
            if (Utilities.TryParseStringIntoThickness(stringBorderThickness, out var thickness))
            {
                BorderThickness = thickness;
                NotifyPropertyChanged(nameof(BorderThickness));
                return true;
            }
            return false;
        }

        public virtual Thickness BorderThickness { get; private set; }

        public virtual string ContentBorderThicknessString
        {
            get => Model.ContentBorderThicknessString;
            set
            {
                if (UpdateContentBorderThickness(value))
                {
                    Model.ContentBorderThicknessString = value;
                    NotifyPropertyChanged(nameof(ContentBorderThicknessString));
                }
            }
        }

        public bool UpdateContentBorderThickness(string stringContentBorderThickness)
        {
            if (Utilities.TryParseStringIntoThickness(stringContentBorderThickness, out var thickness))
            {
                ContentBorderThickness = thickness;
                NotifyPropertyChanged(nameof(ContentBorderThickness));
                return true;
            }
            return false;
        }

        public virtual Thickness ContentBorderThickness { get; private set; }

        public virtual string MarginString
        {
            get => Model.MarginString;
            set
            {
                if (UpdateMargin(value))
                {
                    Model.MarginString = value;
                    NotifyPropertyChanged(nameof(MarginString));
                }
            }
        }

        public bool UpdateMargin(string stringMargin)
        {
            if (Utilities.TryParseStringIntoThickness(stringMargin, out var thickness))
            {
                Margin = thickness;
                NotifyPropertyChanged(nameof(Margin));
                return true;
            }
            return false;
        }

        public virtual Thickness Margin { get; private set; }

        public void HighlightCell(string color)
        {
            SelectionColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            IsHighlighted = true;
            NotifyPropertyChanged(nameof(BackgroundColor));
        }

        public void UnhighlightCell()
        {
            SelectionColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#66666666"));
            IsHighlighted = false;
            NotifyPropertyChanged(nameof(BackgroundColor));
        }

        public virtual Brush BackgroundColor { get; private set; }
        public virtual string BackgroundColorHex
        {
            get => _model.ColorHexes[(int)ColorFor.Background];
            set
            {
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                _model.ColorHexes[(int)ColorFor.Background] = value;
                BackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BackgroundColorHex));
                NotifyPropertyChanged(nameof(BackgroundColor), nameof(BackgroundColorHex));
            }
        }

        public virtual SolidColorBrush BorderColor { get; private set; }
        public virtual string BorderColorHex
        {
            get => _model.ColorHexes[(int)ColorFor.Border];
            set
            {
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                _model.ColorHexes[(int)ColorFor.Border] = value;
                BorderColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BorderColorHex));
                NotifyPropertyChanged(nameof(BorderColor), nameof(BorderColorHex));
            }
        }

        public virtual SolidColorBrush ForegroundColor { get; private set; }
        public virtual string ForegroundColorHex
        {
            get => _model.ColorHexes[(int)ColorFor.Foreground];
            set
            {
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                _model.ColorHexes[(int)ColorFor.Foreground] = value;
                ForegroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ForegroundColorHex));
                NotifyPropertyChanged(nameof(ForegroundColor), nameof(ForegroundColorHex));
            }
        }

        public virtual SolidColorBrush ContentBackgroundColor { get; private set; }
        public virtual string ContentBackgroundColorHex
        {
            get => _model.ColorHexes[(int)ColorFor.ContentBackground];
            set
            {
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                _model.ColorHexes[(int)ColorFor.ContentBackground] = value;
                ContentBackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ContentBackgroundColorHex));
                NotifyPropertyChanged(nameof(ContentBackgroundColor), nameof(ContentBackgroundColorHex));
            }
        }

        public virtual SolidColorBrush ContentBorderColor { get; private set; }
        public virtual string ContentBorderColorHex
        {
            get => _model.ColorHexes[(int)ColorFor.ContentBorder];
            set
            {
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                _model.ColorHexes[(int)ColorFor.ContentBorder] = value;
                ContentBorderColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ContentBorderColorHex));
                NotifyPropertyChanged(nameof(ContentBorderColor), nameof(ContentBorderColorHex));
            }
        }

        public virtual SolidColorBrush ContentHighlightColor { get; private set; }
        public virtual string ContentHighlightColorHex
        {
            get => _model.ColorHexes[(int)ColorFor.ContentHighlight];
            set
            {
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                _model.ColorHexes[(int)ColorFor.ContentHighlight] = value;
                ContentHighlightColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ContentHighlightColorHex));
                NotifyPropertyChanged(nameof(ContentHighlightColor), nameof(ContentHighlightColorHex));
            }
        }

        #endregion

        public string BorderThicknessTop
        {
            get => BorderThickness.Top.ToString();
            set
            {
                BorderThicknessString = $"{value},{value},{value},{value}";
                NotifyBorderThicknessChanged();
            }
        }

        public string BorderThicknessLeft
        {
            get => BorderThickness.Top.ToString();
            set
            {
                BorderThicknessString = $"{value},{BorderThickness.Top},{value},{BorderThickness.Bottom}";
                NotifyBorderThicknessChanged();
            }
        }

        public string BorderThicknessBottom
        {
            get => BorderThickness.Top.ToString();
            set
            {
                BorderThicknessString = $"{BorderThickness.Left},{BorderThickness.Top},{BorderThickness.Right},{value}";
                NotifyBorderThicknessChanged();
            }
        }

        public string BorderThicknessRight
        {
            get => BorderThickness.Top.ToString();
            set
            {
                BorderThicknessString = $"{BorderThickness.Left},{BorderThickness.Top},{value},{BorderThickness.Bottom}";
                NotifyBorderThicknessChanged();
            }
        }

        public void NotifyBorderThicknessChanged()
        {
            NotifyPropertyChanged(nameof(BorderThicknessTop));
            NotifyPropertyChanged(nameof(BorderThicknessLeft));
            NotifyPropertyChanged(nameof(BorderThicknessBottom));
            NotifyPropertyChanged(nameof(BorderThicknessRight));
        }

        public string ContentBorderThicknessTop
        {
            get => ContentBorderThickness.Top.ToString();
            set
            {
                ContentBorderThicknessString = $"{value},{value},{value},{value}";
                NotifyContentBorderThicknessChanged();
            }
        }

        public string ContentBorderThicknessLeft
        {
            get => ContentBorderThickness.Top.ToString();
            set
            {
                ContentBorderThicknessString = $"{value},{ContentBorderThickness.Top},{value},{ContentBorderThickness.Bottom}";
                NotifyContentBorderThicknessChanged();
            }
        }

        public string ContentBorderThicknessBottom
        {
            get => ContentBorderThickness.Top.ToString();
            set
            {
                ContentBorderThicknessString = $"{ContentBorderThickness.Left},{ContentBorderThickness.Top},{ContentBorderThickness.Right},{value}";
                NotifyContentBorderThicknessChanged();
            }
        }

        public string ContentBorderThicknessRight
        {
            get => ContentBorderThickness.Top.ToString();
            set
            {
                ContentBorderThicknessString = $"{ContentBorderThickness.Left},{ContentBorderThickness.Top},{value},{ContentBorderThickness.Bottom}";
                NotifyContentBorderThicknessChanged();
            }
        }

        public void NotifyContentBorderThicknessChanged()
        {
            NotifyPropertyChanged(nameof(ContentBorderThicknessTop));
            NotifyPropertyChanged(nameof(ContentBorderThicknessLeft));
            NotifyPropertyChanged(nameof(ContentBorderThicknessBottom));
            NotifyPropertyChanged(nameof(ContentBorderThicknessRight));
        }

        public string MarginTop
        {
            get => Margin.Top.ToString();
            set
            {
                MarginString = $"{value},{value},{value},{value}";
                NotifyMarginChanged();
            }
        }

        public string MarginLeft
        {
            get => Margin.Top.ToString();
            set
            {
                MarginString = $"{value},{Margin.Top},{value},{Margin.Bottom}";
                NotifyMarginChanged();
            }
        }

        public string MarginBottom
        {
            get => Margin.Top.ToString();
            set
            {
                MarginString = $"{Margin.Left},{Margin.Top},{Margin.Right},{value}";
                NotifyMarginChanged();
            }
        }

        public string MarginRight
        {
            get => Margin.Top.ToString();
            set
            {
                MarginString = $"{Margin.Left},{Margin.Top},{value},{Margin.Bottom}";
                NotifyMarginChanged();
            }
        }

        public void NotifyMarginChanged()
        {
            NotifyPropertyChanged(nameof(MarginTop));
            NotifyPropertyChanged(nameof(MarginLeft));
            NotifyPropertyChanged(nameof(MarginBottom));
            NotifyPropertyChanged(nameof(MarginRight));
        }

        internal string GetName() =>  $"{ColumnCellViewModel.GetColumnName(Column)}{Row}";

        [JsonIgnore]
        public string PrettyCellLocationDependencyNames => string.Join(',', CellPopulateManager.GetAllLocationSubscriptions(Model));

        [JsonIgnore]
        public string PrettyCellCollectionDependencyNames => string.Join(',', CellPopulateManager.GetAllCollectionSubscriptions(Model));
    }
}