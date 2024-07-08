using Cell.Skins;
using System.Windows;

namespace Cell
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Skin Skin { get; set; } = Skin.Dark;
    }
}
