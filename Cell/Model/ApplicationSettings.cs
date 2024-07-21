using Cell.Persistence;
using System.IO;
using System.Text.Json;
using System.Windows.Controls;

namespace Cell.Model
{
    internal class ApplicationSettings : PropertyChangedBase
    {
        private const string ApplicationSettingsSaveDirectory = "Application";
        private const string ApplicationSettingsSaveFile = "Settings.json";
        private static ApplicationSettings? _instance;

        public ApplicationSettings() { } // Required for serialization

        public static ApplicationSettings Instance => _instance ??= CreateInstance();

        public double CodeEditorWidth
        {
            get { return codeEditorWidth; }
            set { if (codeEditorWidth != value) { codeEditorWidth = value; NotifyPropertyChanged(nameof(CodeEditorWidth)); } }
        }
        private double codeEditorWidth = 400;

        public double CodeEditorHeight
        {
            get { return codeEditorHeight; }
            set { if (codeEditorHeight != value) { codeEditorHeight = value; NotifyPropertyChanged(nameof(CodeEditorHeight)); } }
        }
        private double codeEditorHeight = 400;

        public Dock CodeEditorDockPosition
        {
            get { return codeEditorDockPosition; }
            set { if (codeEditorDockPosition != value) { codeEditorDockPosition = value; NotifyPropertyChanged(nameof(CodeEditorDockPosition)); } }
        }
        private Dock codeEditorDockPosition = Dock.Left;

        public static ApplicationSettings CreateInstance()
        {
            _instance = Load() ?? new ApplicationSettings();
            _instance.PropertyChanged += (s, e) => _instance.Save();
            return _instance;
        }

        private static ApplicationSettings? Load()
        {
            var path = Path.Combine(PersistenceManager.SaveLocation, ApplicationSettingsSaveDirectory, ApplicationSettingsSaveFile);
            return File.Exists(path) ? JsonSerializer.Deserialize<ApplicationSettings>(File.ReadAllText(path)) : null;
        }

        private void Save()
        {
            var directory = Path.Combine(PersistenceManager.SaveLocation, ApplicationSettingsSaveDirectory);
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, ApplicationSettingsSaveDirectory);
            var serialized = JsonSerializer.Serialize(this);
            File.WriteAllText(path, serialized);
        }
    }
}
