
using Cell.View.Converters;
using System.Windows.Media;

namespace Cell.View.Skin
{
    public class ColorConstants
    {
        public static readonly string BackgroundColorConstantHex = "#1e1e1e";
        public static readonly string ForegroundColorConstantHex = "#ffffff";
        public static readonly string BorderColorConstantHex = "#3D3D3D";
        public static readonly string ControlBackgroundColorConstantHex = "#252525";
        public static readonly string SelectedBorderColorConstantHex = "#707070";
        public static readonly string SelectedColorConstantHex = "#383838";
        public static readonly string AccentColorConstantHex = "#007acc";
        public static readonly string ToolWindowHeaderColorConstantHex = "#353535";
        public static readonly string ErrorForegroundColorConstantHex = "#ff6666";

        public static readonly Color BackgroundColorConstant = RGBHexColorConverter.ConvertHexStringToColor(BackgroundColorConstantHex);
        public static readonly Color ForegroundColorConstant = RGBHexColorConverter.ConvertHexStringToColor(ForegroundColorConstantHex);
        public static readonly Color BorderColorConstant = RGBHexColorConverter.ConvertHexStringToColor(BorderColorConstantHex);
        public static readonly Color ControlBackgroundColorConstant = RGBHexColorConverter.ConvertHexStringToColor(ControlBackgroundColorConstantHex);
        public static readonly Color SelectedBorderColorConstant = RGBHexColorConverter.ConvertHexStringToColor(SelectedBorderColorConstantHex);
        public static readonly Color SelectedColorConstant = RGBHexColorConverter.ConvertHexStringToColor(SelectedColorConstantHex);
        public static readonly Color AccentColorConstant = RGBHexColorConverter.ConvertHexStringToColor(AccentColorConstantHex);
        public static readonly Color ToolWindowHeaderColorConstant = RGBHexColorConverter.ConvertHexStringToColor(ToolWindowHeaderColorConstantHex);
        public static readonly Color ErrorForegroundColorConstant = RGBHexColorConverter.ConvertHexStringToColor(ErrorForegroundColorConstantHex);
    }
}
