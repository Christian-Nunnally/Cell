using System.Windows;

namespace Cell.Skins
{
    public class SkinResourceDictionary : ResourceDictionary
    {
        private Uri _darkSource;
        private Uri _lightSource;

        public Uri DarkSource
        {
            get { return _darkSource; }
            set
            {
                _darkSource = value;
                UpdateSource();
            }
        }
        public Uri LightSource
        {
            get { return _lightSource; }
            set
            {
                _lightSource = value;
                UpdateSource();
            }
        }

        private void UpdateSource()
        {
            var val = App.Skin == Skin.Dark ? DarkSource : LightSource;
            if (val != null && base.Source != val)
                base.Source = val;
        }
    }
}
