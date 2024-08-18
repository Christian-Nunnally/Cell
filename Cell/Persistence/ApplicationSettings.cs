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
        private Dock codeEditorDockPosition = Dock.Left;
        private double codeEditorHeight = 400;
        private double codeEditorWidth = 400;
        private double functionManagerWindowHeight = 400;
        private double functionManagerWindowWidth = 400;
        private bool highlightPopulateCellDependencies = true;
        private bool highlightPopulateCollectionDependencies = true;
        private bool highlightTriggerCellDependencies = true;
        private bool highlightTriggerCollectionDependencies = true;
        private string lastLoadedSheet = "Default";
        public ApplicationSettings()
        {
        }

        public static ApplicationSettings Instance => _instance ??= CreateInstance();

        public Dock CodeEditorDockPosition
        {
            get { return codeEditorDockPosition; }
            set { if (codeEditorDockPosition != value) { codeEditorDockPosition = value; NotifyPropertyChanged(nameof(CodeEditorDockPosition)); } }
        }

        public double CodeEditorHeight
        {
            get { return codeEditorHeight; }
            set { if (codeEditorHeight != value) { codeEditorHeight = value; NotifyPropertyChanged(nameof(CodeEditorHeight)); } }
        }

        public double CodeEditorWidth
        {
            get { return codeEditorWidth; }
            set { if (codeEditorWidth != value) { codeEditorWidth = value; NotifyPropertyChanged(nameof(CodeEditorWidth)); } }
        }

        public double FunctionManagerWindowHeight
        {
            get { return functionManagerWindowHeight; }
            set { if (functionManagerWindowHeight != value) { functionManagerWindowHeight = value; NotifyPropertyChanged(nameof(FunctionManagerWindowHeight)); } }
        }

        public double FunctionManagerWindowWidth
        {
            get { return functionManagerWindowWidth; }
            set { if (functionManagerWindowWidth != value) { functionManagerWindowWidth = value; NotifyPropertyChanged(nameof(FunctionManagerWindowWidth)); } }
        }

        public bool HighlightPopulateCellDependencies
        {
            get { return highlightPopulateCellDependencies; }
            set { if (highlightPopulateCellDependencies != value) { highlightPopulateCellDependencies = value; NotifyPropertyChanged(nameof(HighlightPopulateCellDependencies)); } }
        }

        public bool HighlightPopulateCollectionDependencies
        {
            get { return highlightPopulateCollectionDependencies; }
            set { if (highlightPopulateCollectionDependencies != value) { highlightPopulateCollectionDependencies = value; NotifyPropertyChanged(nameof(HighlightPopulateCollectionDependencies)); } }
        }

        public bool HighlightTriggerCellDependencies
        {
            get { return highlightTriggerCellDependencies; }
            set { if (highlightTriggerCellDependencies != value) { highlightTriggerCellDependencies = value; NotifyPropertyChanged(nameof(HighlightTriggerCellDependencies)); } }
        }

        public bool HighlightTriggerCollectionDependencies
        {
            get { return highlightTriggerCollectionDependencies; }
            set { if (highlightTriggerCollectionDependencies != value) { highlightTriggerCollectionDependencies = value; NotifyPropertyChanged(nameof(HighlightTriggerCollectionDependencies)); } }
        }

        public string LastLoadedSheet
        {
            get { return lastLoadedSheet; }
            set { if (lastLoadedSheet != value) { lastLoadedSheet = value; NotifyPropertyChanged(nameof(LastLoadedSheet)); } }
        }

        public static ApplicationSettings CreateInstance()
        {
            _instance = Load() ?? new ApplicationSettings();
            _instance.PropertyChanged += (s, e) => _instance.Save();
            return _instance;
        }

        private static ApplicationSettings? Load()
        {
            var path = Path.Combine(PersistenceManager.CurrentRootPath, ApplicationSettingsSaveDirectory, ApplicationSettingsSaveFile);
            if (!File.Exists(path)) return null;
            var text = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ApplicationSettings>(text);
        }

        private void Save()
        {
            var directory = Path.Combine(PersistenceManager.CurrentRootPath, ApplicationSettingsSaveDirectory);
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, ApplicationSettingsSaveFile);
            var serialized = JsonSerializer.Serialize(this);
            File.WriteAllText(path, serialized);
        }
    }
}
