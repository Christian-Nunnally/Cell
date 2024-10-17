using Cell.Core.Common;
using System.Windows.Media;

namespace Cell.View.Skin
{
    /// <summary>
    /// Colors used in the application.
    /// </summary>
    public class ColorConstants
    {
        public const string AccentColorConstantHex = "#007acc";
        public const string BackgroundColorConstantHex = "#1e1e1e";
        public const string BorderColorConstantHex = "#3D3D3D";
        public const string ControlBackgroundColorConstantHex = "#252525";
        public const string ErrorForegroundColorConstantHex = "#ff6666";
        public const string ForegroundColorConstantHex = "#ffffff";
        public const string SelectedBorderColorConstantHex = "#707070";
        public const string SelectedColorConstantHex = "#383838";
        public const string ToolWindowHeaderColorConstantHex = "#353535";
        public static readonly Color AccentColorConstant = ColorAdjuster.ConvertHexStringToColor(AccentColorConstantHex);
        public static readonly Color BackgroundColorConstant = ColorAdjuster.ConvertHexStringToColor(BackgroundColorConstantHex);
        public static readonly Color BorderColorConstant = ColorAdjuster.ConvertHexStringToColor(BorderColorConstantHex);
        public static readonly Color ControlBackgroundColorConstant = ColorAdjuster.ConvertHexStringToColor(ControlBackgroundColorConstantHex);
        public static readonly Color ErrorForegroundColorConstant = ColorAdjuster.ConvertHexStringToColor(ErrorForegroundColorConstantHex);
        public static readonly Color ForegroundColorConstant = ColorAdjuster.ConvertHexStringToColor(ForegroundColorConstantHex);
        public static readonly Color SelectedBorderColorConstant = ColorAdjuster.ConvertHexStringToColor(SelectedBorderColorConstantHex);
        public static readonly Color SelectedColorConstant = ColorAdjuster.ConvertHexStringToColor(SelectedColorConstantHex);
        public static readonly Color ToolWindowHeaderColorConstant = ColorAdjuster.ConvertHexStringToColor(ToolWindowHeaderColorConstantHex);

        public static SolidColorBrush ForegroundColorConstantBrush = new (ForegroundColorConstant);
    }
}
