using Cell.Common;
using System.Windows;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Cell.Model
#pragma warning disable IDE0130 // Namespace does not match folder structure
{
    public class OldCellModel : PropertyChangedBase
    {
        public Dictionary<string, bool> BooleanProperties { get; set; } = [];

        public string BorderThicknessString { get; set; } = string.Empty;

        public CellType CellType { get; set; }

        public string[] ColorHexes { get; set; } = ["#deadbe", "#aaaaaa", "#bbbbbb", "#111111", "#121212", "#555555"];

        public int Column { get; set; }

        public string ContentBorderThicknessString { get; set; } = string.Empty;

        public string FontFamily { get; set; } = string.Empty;

        public double FontSize { get; set; }

        public double Height { get; set; }

        public HorizontalAlignment HorizontalAlignment { get; set; }

        public string ID { get; set; } = string.Empty;

        public int Index { get; set; }

        public bool IsFontBold { get; set; }

        public bool IsFontItalic { get; set; }

        public bool IsFontStrikethrough { get; set; }

        public string MarginString { get; set; } = string.Empty;

        public string MergedWith { get; set; } = string.Empty;

        public Dictionary<string, double> NumericProperties { get; set; } = [];

        public string PopulateFunctionName { get; set; } = string.Empty;

        public int Row { get; set; }

        public string SheetName { get; set; } = string.Empty;

        public Dictionary<string, string> StringProperties { get; set; } = [];

        public string Text { get; set; } = string.Empty;

        public TextAlignment TextAlignmentForView { get; set; }

        public string TriggerFunctionName { get; set; } = string.Empty;

        public VerticalAlignment VerticalAlignment { get; set; }

        public double Width { get; set; }
    }
}
