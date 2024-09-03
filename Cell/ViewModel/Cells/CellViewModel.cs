using Cell.Common;
using Cell.Model;
using Cell.ViewModel.Application;
using Cell.ViewModel.Cells.Types.Special;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;

namespace Cell.ViewModel.Cells
{
    public abstract class CellViewModel : PropertyChangedBase
    {
        private readonly CellModel _model;
        protected SheetViewModel _sheetViewModel;
        private bool _isHighlighted;
        private bool _isSelected;
        private SolidColorBrush _selectionBorderColor = new((Color)ColorConverter.ConvertFromString("#ffff0000"));
        private SolidColorBrush _selectionColor = new((Color)ColorConverter.ConvertFromString("#ffff0000"));
        private double _x;
        private double _y;
        public CellViewModel(CellModel model, SheetViewModel sheet)
        {
            _sheetViewModel = sheet;
            _model = model;

            BackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BackgroundColorHex));
            ContentBackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ContentBackgroundColorHex));
            ForegroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ForegroundColorHex));
            BorderColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BorderColorHex));
            ContentBorderColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ContentBorderColorHex));
            ContentHighlightColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ContentHighlightColorHex));
            SelectionColor = new((Color)ColorConverter.ConvertFromString(BackgroundColorHex));
            SelectionBorderColor = new((Color)ColorConverter.ConvertFromString(BackgroundColorHex));
            UpdateBorderThickness(BorderThicknessString);
            UpdateContentBorderThickness(ContentBorderThicknessString);
            _model.PropertyChanged += ModelPropertyChanged;
        }

        public virtual Brush BackgroundColor { get; private set; }

        public virtual string BackgroundColorHex
        {
            get => _model.ColorHexes[(int)ColorFor.Background];
            set
            {
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                _model.SetBackground(value);
                var color = (Color)ColorConverter.ConvertFromString(value);
                BackgroundColor = new SolidColorBrush(color);
                NotifyPropertyChanged(nameof(BackgroundColor), nameof(BackgroundColorHex));
                SelectionColor = new(ColorAdjuster.GetHighlightColor(color, 100));
                SelectionBorderColor = new(ColorAdjuster.GetHighlightColor(color, 175));
            }
        }

        public virtual SolidColorBrush BorderColor { get; private set; }

        public virtual string BorderColorHex
        {
            get => _model.ColorHexes[(int)ColorFor.Border];
            set
            {
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                _model.SetBorder(value);
                var color = (Color)ColorConverter.ConvertFromString(BorderColorHex);
                BorderColor = new SolidColorBrush(color);
                NotifyPropertyChanged(nameof(BorderColor), nameof(BorderColorHex));
            }
        }

        public virtual Thickness BorderThickness { get; private set; }

        public string BorderThicknessBottom
        {
            get => BorderThickness.Top.ToString();
            set
            {
                BorderThicknessString = $"{BorderThickness.Left},{BorderThickness.Top},{BorderThickness.Right},{value}";
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

        public string BorderThicknessRight
        {
            get => BorderThickness.Top.ToString();
            set
            {
                BorderThicknessString = $"{BorderThickness.Left},{BorderThickness.Top},{value},{BorderThickness.Bottom}";
                NotifyBorderThicknessChanged();
            }
        }

        public virtual string BorderThicknessString
        {
            get => _model.BorderThicknessString;
            set
            {
                if (UpdateBorderThickness(value))
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                    _model.BorderThicknessString = value;
                    NotifyPropertyChanged(nameof(BorderThicknessString));
                }
            }
        }

        public string BorderThicknessTop
        {
            get => BorderThickness.Top.ToString();
            set
            {
                BorderThicknessString = $"{value},{value},{value},{value}";
                NotifyBorderThicknessChanged();
            }
        }

        public virtual CellType CellType
        {
            get => _model.CellType;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                _model.CellType = value;
                NotifyPropertyChanged(nameof(CellType));
            }
        }

        public virtual int Column
        {
            get => _model.Column;
            set { _model.Column = value; }
        }

        public virtual SolidColorBrush ContentBackgroundColor { get; private set; }

        public virtual string ContentBackgroundColorHex
        {
            get => _model.ColorHexes[(int)ColorFor.ContentBackground];
            set
            {
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
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
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                _model.SetContentBorder(value);
                ContentBorderColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ContentBorderColorHex));
                NotifyPropertyChanged(nameof(ContentBorderColor), nameof(ContentBorderColorHex));
            }
        }

        public virtual Thickness ContentBorderThickness { get; private set; }

        public string ContentBorderThicknessBottom
        {
            get => ContentBorderThickness.Top.ToString();
            set
            {
                ContentBorderThicknessString = $"{ContentBorderThickness.Left},{ContentBorderThickness.Top},{ContentBorderThickness.Right},{value}";
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

        public string ContentBorderThicknessRight
        {
            get => ContentBorderThickness.Top.ToString();
            set
            {
                ContentBorderThicknessString = $"{ContentBorderThickness.Left},{ContentBorderThickness.Top},{value},{ContentBorderThickness.Bottom}";
                NotifyContentBorderThicknessChanged();
            }
        }

        public virtual string ContentBorderThicknessString
        {
            get => Model.ContentBorderThicknessString;
            set
            {
                if (UpdateContentBorderThickness(value))
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                    _model.ContentBorderThicknessString = value;
                    NotifyPropertyChanged(nameof(ContentBorderThicknessString));
                }
            }
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

        public virtual SolidColorBrush ContentHighlightColor { get; private set; }

        public virtual string ContentHighlightColorHex
        {
            get => _model.ColorHexes[(int)ColorFor.ContentHighlight];
            set
            {
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                _model.SetContentHighlight(value);
                ContentHighlightColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ContentHighlightColorHex));
                NotifyPropertyChanged(nameof(ContentHighlightColor), nameof(ContentHighlightColorHex));
            }
        }

        public virtual string FontFamily
        {
            get => _model.FontFamily;
            set
            {
                ApplicationViewModel.Instance.UndoRedoManager.RecordStateIfRecording(_model);
                _model.FontFamily = value;
                NotifyPropertyChanged(nameof(FontFamily));
            }
        }

        public virtual double FontSize
        {
            get => _model.FontSize;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                _model.FontSize = value;
                NotifyPropertyChanged(nameof(FontSize));
            }
        }

        public FontStyle FontStyleForView => IsFontItalic ? FontStyles.Italic : FontStyles.Normal;

        public FontWeight FontWeightForView => IsFontBold ? FontWeights.Bold : FontWeights.Normal;

        public virtual SolidColorBrush ForegroundColor { get; private set; }

        public virtual string ForegroundColorHex
        {
            get => _model.ColorHexes[(int)ColorFor.Foreground];
            set
            {
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                _model.SetForeground(value);
                ForegroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ForegroundColorHex));
                NotifyPropertyChanged(nameof(ForegroundColor), nameof(ForegroundColorHex));
            }
        }

        public virtual double Height
        {
            get => _model.Height;
            set
            {
                _model.Height = value;
            }
        }

        public virtual HorizontalAlignment HorizontalAlignmentForView
        {
            get => _model.HorizontalAlignment;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                _model.HorizontalAlignment = value;
                NotifyPropertyChanged(nameof(HorizontalAlignmentForView), nameof(HorizontalAlignmentForViewCenter));
            }
        }

        public virtual HorizontalAlignment HorizontalAlignmentForViewCenter => HorizontalAlignmentForView == HorizontalAlignment.Stretch ? HorizontalAlignment.Center : HorizontalAlignmentForView;

        public virtual string ID { get => _model.ID; }

        public virtual int Index
        {
            get => _model.Index;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                _model.Index = value;
                NotifyPropertyChanged(nameof(Index));
            }
        }

        public virtual bool IsFontBold
        {
            get => _model.IsFontBold;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                _model.IsFontBold = value;
                NotifyPropertyChanged(nameof(IsFontBold));
                NotifyPropertyChanged(nameof(FontWeightForView));
            }
        }

        public virtual bool IsFontItalic
        {
            get => _model.IsFontItalic;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                _model.IsFontItalic = value;
                NotifyPropertyChanged(nameof(IsFontItalic));
                NotifyPropertyChanged(nameof(FontStyleForView));
            }
        }

        public virtual bool IsFontStrikethrough
        {
            get => _model.IsFontStrikethrough;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                _model.IsFontStrikethrough = value;
                NotifyPropertyChanged(nameof(IsFontStrikethrough));
                NotifyPropertyChanged(nameof(TextDecorationsForView));
            }
        }

        public virtual bool IsHighlighted
        {
            get => _isHighlighted;
            set
            {
                if (_isHighlighted == value) return;
                _isHighlighted = value;
                NotifyPropertyChanged(nameof(IsHighlighted), nameof(ShouldShowSelectionBorder), nameof(ShouldShowSelectionFill));
            }
        }

        public virtual bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                NotifyPropertyChanged(nameof(IsSelected), nameof(ShouldShowSelectionBorder), nameof(ShouldShowSelectionFill));
            }
        }

        public virtual Thickness Margin { get; private set; }

        public string MarginBottom
        {
            get => Margin.Top.ToString();
            set
            {
                MarginString = $"{Margin.Left},{Margin.Top},{Margin.Right},{value}";
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

        public string MarginRight
        {
            get => Margin.Top.ToString();
            set
            {
                MarginString = $"{Margin.Left},{Margin.Top},{value},{Margin.Bottom}";
                NotifyMarginChanged();
            }
        }

        public virtual string MarginString
        {
            get => Model.MarginString;
            set
            {
                if (UpdateMargin(value))
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                    Model.MarginString = value;
                    NotifyPropertyChanged(nameof(MarginString));
                }
            }
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

        public CellModel Model => _model;

        public virtual string PopulateFunctionName
        {
            get => _model.PopulateFunctionName;
            set
            {
                if (ApplicationViewModel.Instance.PluginFunctionLoader.GetOrCreateFunction("object", value) is not null)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                    _model.PopulateFunctionName = value;
                    NotifyPropertyChanged(nameof(PopulateFunctionName));
                    PopulateText();
                }
            }
        }

        [JsonIgnore]
        public IEnumerable<string> PrettyCellLocationDependencyNames => ApplicationViewModel.Instance.CellPopulateManager.GetAllLocationSubscriptions(Model).Select(x =>
            {
                var split = x.Replace($"{Model.SheetName}_", "").Split('_');
                if (split.Length == 2) return $"{ColumnCellViewModel.GetColumnName(int.Parse(split[1]))}{split[0]}";
                return $"{split[0]}_{ColumnCellViewModel.GetColumnName(int.Parse(split[2]))}{split[1]}";
            });

        [JsonIgnore]
        public List<string> PrettyDependencyNames => [.. ApplicationViewModel.Instance.CellPopulateManager.GetAllCollectionSubscriptions(Model), .. PrettyCellLocationDependencyNames];

        public virtual int Row
        {
            get => _model.Row;
            set { _model.Row = value; }
        }

        public virtual SolidColorBrush SelectionBorderColor
        {
            get => _selectionBorderColor;
            set { if (_selectionBorderColor == value) return; _selectionBorderColor = value; NotifyPropertyChanged(nameof(SelectionBorderColor)); }
        }

        public virtual SolidColorBrush SelectionColor
        {
            get => _selectionColor;
            set { if (_selectionColor == value) return; _selectionColor = value; NotifyPropertyChanged(nameof(SelectionColor)); }
        }

        public virtual bool ShouldShowSelectionBorder => IsSelected || IsHighlighted;

        public virtual bool ShouldShowSelectionFill => IsSelected || IsHighlighted;

        public virtual string Text
        {
            get => _model.Text;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                _model.Text = value?.Replace("\\n", "\n") ?? string.Empty;
            }
        }

        public virtual TextAlignment TextAlignmentForView
        {
            get => _model.TextAlignmentForView;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                _model.TextAlignmentForView = value;
                NotifyPropertyChanged(nameof(TextAlignmentForView));
            }
        }

        public TextDecorationCollection? TextDecorationsForView => IsFontStrikethrough ? TextDecorations.Strikethrough : null;

        public virtual string TriggerFunctionName
        {
            get => _model.TriggerFunctionName;
            set
            {
                if (ApplicationViewModel.Instance.PluginFunctionLoader.GetOrCreateFunction("void", value) is not null)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                    _model.TriggerFunctionName = value;
                    NotifyPropertyChanged(nameof(TriggerFunctionName));
                };
            }
        }

        public virtual VerticalAlignment VerticalAlignmentForView
        {
            get => _model.VerticalAlignment;
            set
            {
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(_model);
                _model.VerticalAlignment = value;
                NotifyPropertyChanged(nameof(VerticalAlignmentForView));
                NotifyPropertyChanged(nameof(VerticalAlignmentForViewCenter));
            }
        }

        public virtual VerticalAlignment VerticalAlignmentForViewCenter => VerticalAlignmentForView == VerticalAlignment.Stretch ? VerticalAlignment.Center : VerticalAlignmentForView;

        public virtual double Width
        {
            get => _model.Width;
            set { _model.Width = value; }
        }

        public virtual double X
        {
            get => _x;
            set { _x = value; NotifyPropertyChanged(nameof(X)); }
        }

        public virtual double Y
        {
            get => _y;
            set { _y = value; NotifyPropertyChanged(nameof(Y)); }
        }

        public void HighlightCell(string color)
        {
            SelectionColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            SelectionBorderColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            IsHighlighted = true;
            NotifyPropertyChanged(nameof(BackgroundColor));
        }

        public void NotifyBorderThicknessChanged()
        {
            NotifyPropertyChanged(nameof(BorderThicknessTop));
            NotifyPropertyChanged(nameof(BorderThicknessLeft));
            NotifyPropertyChanged(nameof(BorderThicknessBottom));
            NotifyPropertyChanged(nameof(BorderThicknessRight));
        }

        public void NotifyContentBorderThicknessChanged()
        {
            NotifyPropertyChanged(nameof(ContentBorderThicknessTop));
            NotifyPropertyChanged(nameof(ContentBorderThicknessLeft));
            NotifyPropertyChanged(nameof(ContentBorderThicknessBottom));
            NotifyPropertyChanged(nameof(ContentBorderThicknessRight));
        }

        public void NotifyMarginChanged()
        {
            NotifyPropertyChanged(nameof(MarginTop));
            NotifyPropertyChanged(nameof(MarginLeft));
            NotifyPropertyChanged(nameof(MarginBottom));
            NotifyPropertyChanged(nameof(MarginRight));
        }

        public void PopulateText()
        {
            Model.PopulateText();
        }

        public void UnhighlightCell()
        {
            SelectionColor = new(ColorAdjuster.GetHighlightColor((Color)ColorConverter.ConvertFromString(BackgroundColorHex), 100));
            SelectionBorderColor = new(ColorAdjuster.GetHighlightColor((Color)ColorConverter.ConvertFromString(BackgroundColorHex), 175));
            IsHighlighted = false;
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

        internal string GetName() => $"{ColumnCellViewModel.GetColumnName(Column)}{Row}";

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
    }
}
