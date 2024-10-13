using Cell.Core.Common;
using System.Text.Json.Serialization;
using System.Windows;

namespace Cell.Model
{
    /// <summary>
    /// The model for the style of a cell.
    /// </summary>
    public class CellStyleModel : PropertyChangedBase
    {
        private string _backgroundColorHex = "#000000";
        private string _borderColorHex = "#000000";
        private string _borderThickness = "1";
        private string _contentBackgroundColorHex = "#000000";
        private string _contentBorderColorHex = "#000000";
        private string _contentBorderThickness = "1";
        private string _font = "Consolas";
        private double _fontSize = 10;
        private string _foregroundColorHex = "#000000";
        private string _highlightColorHex = "#000000";
        private HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Center;
        private bool _isFontBold = false;
        private bool _isFontItalic = false;
        private bool _isFontStrikethrough = false;
        private string _margin = "0";
        private TextAlignment _textAlignment = TextAlignment.Center;
        private VerticalAlignment _verticalAlignment = VerticalAlignment.Center;
        /// <summary>
        /// Gets or sets the background color of the cell.
        /// </summary>
        public string BackgroundColor
        {
            get => _backgroundColorHex;
            set
            {
                if (_backgroundColorHex == value) return;
                if (value == null) return;
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                _backgroundColorHex = value;
                NotifyPropertyChanged(nameof(BackgroundColor));
            }
        }

        /// <summary>
        /// Gets the cell model that this style is applied to.
        /// </summary>
        [JsonIgnore]
        public CellModel? CellModel { get; internal set; }

        /// <summary>
        /// Gets or sets whether the font is bold.
        /// </summary>
        public bool Bold
        {
            get => _isFontBold;
            set
            {
                if (_isFontBold == value) return;
                _isFontBold = value;
                NotifyPropertyChanged(nameof(Bold));
            }
        }

        /// <summary>
        /// Gets or sets the border thickness of the cell.
        /// </summary>
        public string Border
        {
            get { return _borderThickness; }
            set
            {
                if (_borderThickness == value) return;
                _borderThickness = value;
                NotifyPropertyChanged(nameof(Border));
            }
        }

        /// <summary>
        /// Gets or sets the background color of the border of the cell
        /// </summary>
        public string BorderColor
        {
            get => _borderColorHex;
            set
            {
                if (_borderColorHex == value) return;
                if (value == null) return;
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                _borderColorHex = value;
                NotifyPropertyChanged(nameof(BorderColor));
            }
        }

        /// <summary>
        /// Gets or sets the background color of the content of the cell.
        /// </summary>
        public string ContentBackgroundColor
        {
            get => _contentBackgroundColorHex;
            set
            {
                if (_contentBackgroundColorHex == value) return;
                if (value == null) return;
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                _contentBackgroundColorHex = value;
                NotifyPropertyChanged(nameof(ContentBackgroundColor));
            }
        }

        /// <summary>
        /// Gets or sets the thickness of the border of the content of the cell.
        /// </summary>
        public string ContentBorder
        {
            get => _contentBorderThickness;
            set
            {
                if (_contentBorderThickness == value) return;
                _contentBorderThickness = value;
                NotifyPropertyChanged(nameof(ContentBorder));
            }
        }

        /// <summary>
        /// Gets or sets the color of the border of the content of the cell.
        /// </summary>
        public string ContentBorderColor
        {
            get => _contentBorderColorHex;
            set
            {
                if (_contentBorderColorHex == value) return;
                if (value == null) return;
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                _contentBorderColorHex = value;
                NotifyPropertyChanged(nameof(ContentBorderColor));
            }
        }

        /// <summary>
        /// Gets or sets the margin of the content of the cell.
        /// </summary>
        public string ContentMargin
        {
            get => _margin;
            set
            {
                if (_margin == value) return;
                _margin = value;
                NotifyPropertyChanged(nameof(ContentMargin));
            }
        }

        /// <summary>
        /// Gets or sets the font of the cell.
        /// </summary>
        public string Font
        {
            get => _font;
            set
            {
                if (_font == value) return;
                _font = value;
                NotifyPropertyChanged(nameof(Font));
            }
        }

        /// <summary>
        /// Gets or sets the font size of the cell.
        /// </summary>
        public double FontSize
        {
            get => _fontSize;
            set
            {
                if (_fontSize == value) return;
                _fontSize = value;
                NotifyPropertyChanged(nameof(FontSize));
            }
        }

        /// <summary>
        /// Gets or sets the color of the foreground of the cell.
        /// </summary>
        public string ForegroundColor
        {
            get => _foregroundColorHex;
            set
            {
                if (_foregroundColorHex == value) return;
                if (value == null) return;
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                _foregroundColorHex = value;
                NotifyPropertyChanged(nameof(ForegroundColor));
            }
        }

        /// <summary>
        /// Gets or sets the color of the mouse over highlight of the cell.
        /// </summary>
        public string HighlightColor
        {
            get => _highlightColorHex;
            set
            {
                if (_highlightColorHex == value) return;
                if (value == null) return;
                if (!Utilities.IsHexidecimalColorCode().IsMatch(value)) return;
                _highlightColorHex = value;
                NotifyPropertyChanged(nameof(HighlightColor));
            }
        }

        /// <summary>
        /// Gets or sets the horizontal alignment of the content of the cell.
        /// </summary>
        public HorizontalAlignment HorizontalAlignment
        {
            get => _horizontalAlignment;
            set
            {
                if (_horizontalAlignment == value) return;
                _horizontalAlignment = value;
                NotifyPropertyChanged(nameof(HorizontalAlignment));
            }
        }

        /// <summary>
        /// Gets or sets whether the font is italic.
        /// </summary>
        public bool Italic
        {
            get => _isFontItalic;
            set
            {
                if (_isFontItalic == value) return;
                _isFontItalic = value;
                NotifyPropertyChanged(nameof(Italic));
            }
        }

        /// <summary>
        /// Gets or sets the strikethrough property of the font.
        /// </summary>
        public bool Strikethrough
        {
            get => _isFontStrikethrough;
            set
            {
                if (_isFontStrikethrough == value) return;
                _isFontStrikethrough = value;
                NotifyPropertyChanged(nameof(Strikethrough));
            }
        }

        /// <summary>
        /// Gets or sets the text alignment of the cell.
        /// </summary>
        public TextAlignment TextAlignment
        {
            get => _textAlignment;
            set
            {
                if (_textAlignment == value) return;
                _textAlignment = value;
                NotifyPropertyChanged(nameof(TextAlignment));
            }
        }

        /// <summary>
        /// Gets or sets the vertical alignment of the cell.
        /// </summary>
        public VerticalAlignment VerticalAlignment
        {
            get => _verticalAlignment;
            set
            {
                if (_verticalAlignment == value) return;
                _verticalAlignment = value;
                NotifyPropertyChanged(nameof(VerticalAlignment));
            }
        }

        /// <summary>
        /// Copies the values of this style into another style.
        /// </summary>
        /// <param name="otherStyle">The style to copy in to.</param>
        public void CopyTo(CellStyleModel otherStyle)
        {
            otherStyle.Font = Font;
            otherStyle.FontSize = FontSize;
            otherStyle.Bold = Bold;
            otherStyle.Italic = Italic;
            otherStyle.Strikethrough = Strikethrough;
            otherStyle.HorizontalAlignment = HorizontalAlignment;
            otherStyle.VerticalAlignment = VerticalAlignment;
            otherStyle.TextAlignment = TextAlignment;
            otherStyle.Border = Border;
            otherStyle.ContentBorder = ContentBorder;
            otherStyle.ContentMargin = ContentMargin;
            otherStyle.BackgroundColor = BackgroundColor;
            otherStyle.ContentBackgroundColor = ContentBackgroundColor;
            otherStyle.ForegroundColor = ForegroundColor;
            otherStyle.BorderColor = BorderColor;
            otherStyle.ContentBorderColor = ContentBorderColor;
            otherStyle.HighlightColor = HighlightColor;
        }
    }
}
