using Cell.Common;
using System.IO;
using System.Text.Json;
using System.Windows.Controls;

namespace Cell.Persistence
{
    public class ApplicationSettings : PropertyChangedBase
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

        public double FunctionManagerWindowWidth
        {
            get { return functionManagerWindowWidth; }
            set { if (functionManagerWindowWidth != value) { functionManagerWindowWidth = value; NotifyPropertyChanged(nameof(FunctionManagerWindowWidth)); } }
        }
        private double functionManagerWindowWidth = 400;

        public double FunctionManagerWindowHeight
        {
            get { return functionManagerWindowHeight; }
            set { if (functionManagerWindowHeight != value) { functionManagerWindowHeight = value; NotifyPropertyChanged(nameof(FunctionManagerWindowHeight)); } }
        }
        private double functionManagerWindowHeight = 400;

        public Dock CodeEditorDockPosition
        {
            get { return codeEditorDockPosition; }
            set { if (codeEditorDockPosition != value) { codeEditorDockPosition = value; NotifyPropertyChanged(nameof(CodeEditorDockPosition)); } }
        }
        private Dock codeEditorDockPosition = Dock.Left;

        public string LastLoadedSheet
        {
            get { return lastLoadedSheet; }
            set { if (lastLoadedSheet != value) { lastLoadedSheet = value; NotifyPropertyChanged(nameof(LastLoadedSheet)); } }
        }
        private string lastLoadedSheet = "Default";

        public bool HighlightPopulateCellDependencies
        {
            get { return highlightPopulateCellDependencies; }
            set { if (highlightPopulateCellDependencies != value) { highlightPopulateCellDependencies = value; NotifyPropertyChanged(nameof(HighlightPopulateCellDependencies)); } }
        }
        private bool highlightPopulateCellDependencies = true;

        public bool HighlightPopulateCollectionDependencies
        {
            get { return highlightPopulateCollectionDependencies; }
            set { if (highlightPopulateCollectionDependencies != value) { highlightPopulateCollectionDependencies = value; NotifyPropertyChanged(nameof(HighlightPopulateCollectionDependencies)); } }
        }
        private bool highlightPopulateCollectionDependencies = true;

        public bool HighlightTriggerCellDependencies
        {
            get { return highlightTriggerCellDependencies; }
            set { if (highlightTriggerCellDependencies != value) { highlightTriggerCellDependencies = value; NotifyPropertyChanged(nameof(HighlightTriggerCellDependencies)); } }
        }
        private bool highlightTriggerCellDependencies = true;

        public bool HighlightTriggerCollectionDependencies
        {
            get { return highlightTriggerCollectionDependencies; }
            set { if (highlightTriggerCollectionDependencies != value) { highlightTriggerCollectionDependencies = value; NotifyPropertyChanged(nameof(HighlightTriggerCollectionDependencies)); } }
        }
        private bool highlightTriggerCollectionDependencies = true;

        public static ApplicationSettings CreateInstance()
        {
            _instance = Load() ?? new ApplicationSettings();
            _instance.PropertyChanged += (s, e) => _instance.Save();
            return _instance;
        }

        private static ApplicationSettings? Load()
        {
            var path = Path.Combine(PersistenceManager.SaveLocation, ApplicationSettingsSaveDirectory, ApplicationSettingsSaveFile);
            if (!File.Exists(path)) return null;
            var text = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ApplicationSettings>(text);
        }

        private void Save()
        {
            var directory = Path.Combine(PersistenceManager.SaveLocation, ApplicationSettingsSaveDirectory);
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, ApplicationSettingsSaveFile);
            var serialized = JsonSerializer.Serialize(this);
            File.WriteAllText(path, serialized);
        }
    }
}
