using Cell.Persistence;
using System.IO;
using System.Text.Json;
using System.Windows.Controls;

namespace Cell.Model
{
    internal class ApplicationSettings : PropertyChangedBase
    {
        private static ApplicationSettings? _instance;

        public ApplicationSettings()
        {
        }

        public static ApplicationSettings CreateInstance()
        {
            var path = Path.Combine(PersistenceManager.SaveLocation, "Application", "Settings.json");
            if (File.Exists(path))
            {
                _instance = JsonSerializer.Deserialize<ApplicationSettings>(File.ReadAllText(path)) ?? new ApplicationSettings();
            }
            else
            {
                _instance = new ApplicationSettings();
            }
            _instance.PropertyChanged += (s, e) => _instance.Save();
            return _instance;
        }

        private void Save()
        {
            var directory = Path.Combine(PersistenceManager.SaveLocation, "Application");
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, "Settings.json");
            var serialized = JsonSerializer.Serialize(this);
            File.WriteAllText(path, serialized);
        }

        public static ApplicationSettings Instance => _instance ??= CreateInstance();

        public double CodeEditorWidth
        {
            get { return codeEditorWidth; }
            set { if (codeEditorWidth != value) { codeEditorWidth = value; OnPropertyChanged(nameof(CodeEditorWidth)); } }
        }
        private double codeEditorWidth = 400;

        public double CodeEditorHeight
        {
            get { return codeEditorHeight; }
            set { if (codeEditorHeight != value) { codeEditorHeight = value; OnPropertyChanged(nameof(CodeEditorHeight)); } }
        }
        private double codeEditorHeight = 400;

        public Dock CodeEditorDockPosition
        {
            get { return codeEditorDockPosition; }
            set { if (codeEditorDockPosition != value) { codeEditorDockPosition = value; OnPropertyChanged(nameof(CodeEditorDockPosition)); } }
        }
        private Dock codeEditorDockPosition = Dock.Left;
    }
}
