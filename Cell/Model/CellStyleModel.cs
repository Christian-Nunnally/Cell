
using System.Windows;
using Cell.Common;
using Cell.View.Skin;

namespace Cell.Model
{
    internal class CellStyleModel : PropertyChangedBase
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
        private bool _isFontBold = false;
        private bool _isFontItalic = false;
        private bool _isFontStrikethrough = false;
        private string _margin = "0";
        private string mergedWith = string.Empty;
        private string populateFunctionName = string.Empty;
        private int row;
        private string sheetName = string.Empty;
        private string text = string.Empty;
        private TextAlignment textAlignment = TextAlignment.Center;
        private string triggerFunctionName = string.Empty;
        private VerticalAlignment verticalAlignment = VerticalAlignment.Center;
        private double width;
        private string _backgroundColorHex;
        private string _foregroundColorHex;
        private string _contentBackgroundColorHex;
        private string _highlightColorHex;
        private string _borderColorHex;
        private string _contentBorderColorHex;

        public event Action<CellModel>? AfterCellEdited;

        public HorizontalAlignment HorizontalAlignment
        {
            get => horizontalAlignment;
            set 
            { 
                if (horizontalAlignment == value) return; 
                horizontalAlignment = value; 
                NotifyPropertyChanged(nameof(HorizontalAlignment)); }
        }

        public string ID
        {
            get => id;
            set 
            {
                if (id == value) return; 
                id = value; 
                NotifyPropertyChanged(nameof(ID)); 
            }
        }

        public int Index
        {
            get => index;
            set 
            {
                if (index == value) return;
                index = value; 
                NotifyPropertyChanged(nameof(Index));
            }
        }

        public bool IsFontBold
        {
            get => _isFontBold;
            set 
            { 
                if (_isFontBold == value) return; 
                _isFontBold = value; 
                NotifyPropertyChanged(nameof(IsFontBold)); 
            }
        }

        public bool IsFontItalic
        {
            get => _isFontItalic;
            set 
            { 
                if (_isFontItalic == value) return; 
                _isFontItalic = value; 
                NotifyPropertyChanged(nameof(IsFontItalic)); 
            }
        }

        public bool IsFontStrikethrough
        {
            get => _isFontStrikethrough;
            set 
            { 
                if (_isFontStrikethrough == value) return; 
                _isFontStrikethrough = value; 
                NotifyPropertyChanged(nameof(IsFontStrikethrough)); 
            }
        }

        public string MarginString
        {
            get => _margin;
            set 
            { 
                if (_margin == value) return; 
                _margin = value; 
                NotifyPropertyChanged(nameof(MarginString)); 
            }
        }

        public string BackgroundColorHex
        {
            get => _backgroundColorHex;
            set
            {
                if (_backgroundColorHex == value) return;
                _backgroundColorHex = value;
                NotifyPropertyChanged(nameof(BackgroundColorHex));
            }
        }

        public string ContentBackgroundColorHex
        {
            get => _contentBackgroundColorHex;
            set
            {
                if (_contentBackgroundColorHex == value) return;
                _contentBackgroundColorHex = value;
                NotifyPropertyChanged(nameof(ContentBackgroundColorHex));
            }
        }

        public string ForegroundColorHex
        {
            get => _foregroundColorHex;
            set
            {
                if (_foregroundColorHex == value) return;
                _foregroundColorHex = value;
                NotifyPropertyChanged(nameof(ForegroundColorHex));
            }
        }

        public string BorderColorHex
        {
            get => _borderColorHex;
            set
            {
                if (_borderColorHex == value) return;
                _borderColorHex = value;
                NotifyPropertyChanged(nameof(BorderColorHex));
            }
        }

        public string ContentBorderColorHex
        {
            get => _contentBorderColorHex;
            set
            {
                if (_contentBorderColorHex == value) return;
                _contentBorderColorHex = value;
                NotifyPropertyChanged(nameof(BorderColorHex));
            }
        }
        public string HighlightColorHex
        {
            get => _highlightColorHex;
            set
            {
                if (_highlightColorHex == value) return;
                _highlightColorHex = value;
                NotifyPropertyChanged(nameof(HighlightColorHex));
            }
        }

        public string BorderThicknessString
        {
            get { return borderThickness; }
            set { if (borderThickness == value) return; borderThickness = value; NotifyPropertyChanged(nameof(BorderThicknessString)); }
        }
    }
}
