using Cell.Common;
using Cell.Model;
using Cell.Persistence;
using Cell.Plugin;
using System.Windows;
using System.Windows.Media;

namespace Cell.ViewModel
{
    public abstract class CellViewModel : PropertyChangedBase
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
                NotifyPropertyChanged(e.PropertyName);
                return;
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
            set
            {
                isSelected = value;
                NotifyPropertyChanged(nameof(IsSelected));
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
                    NotifyPropertyChanged(nameof(PopulateFunctionName));
                }
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
                    NotifyPropertyChanged(nameof(TriggerFunctionName));
                };
            }
        }

        public void PopulateText()
        {
            if (string.IsNullOrEmpty(PopulateFunctionName)) return;
            var result = DynamicCellPluginExecutor.RunPopulate(new PluginContext(ApplicationViewModel.Instance), _model);
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

        public virtual SolidColorBrush BackgroundColor { get; private set; }

        public void HighlightCell(string color)
        {
            BackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            NotifyPropertyChanged(nameof(BackgroundColor));
        }

        public void UnhighlightCell()
        {
            BackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BackgroundColorHex));
            NotifyPropertyChanged(nameof(BackgroundColor));
        }

        public virtual string BackgroundColorHex
        {
            get => _model.BackgroundColorHex;
            set
            {
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                _model.BackgroundColorHex = value;
                BackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BackgroundColorHex));
                NotifyPropertyChanged(nameof(BackgroundColor), nameof(BackgroundColorHex));
            }
        }

        public virtual SolidColorBrush ForegroundColor { get; private set; }

        public virtual string ForegroundColorHex
        {
            get => _model.ForegroundColorHex;
            set
            {
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                _model.ForegroundColorHex = value;
                ForegroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ForegroundColorHex));
                NotifyPropertyChanged(nameof(ForegroundColor), nameof(ForegroundColorHex));
            }
        }

        public virtual SolidColorBrush BorderColor { get; private set; }

        public virtual string BorderColorHex
        {
            get => _model.BorderColorHex;
            set
            {
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                _model.BorderColorHex = value;
                BorderColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BorderColorHex));
                NotifyPropertyChanged(nameof(BorderColor), nameof(BorderColorHex));
            }
        }

        #endregion
    }
}