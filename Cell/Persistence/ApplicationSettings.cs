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
        private double logWindowHeight = 400;
        private double logWindowWidth = 400;
        public ApplicationSettings()
        {
        }

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

        public double LogWindowHeight
        {
            get { return logWindowHeight; }
            set { if (logWindowHeight != value) { logWindowHeight = value; NotifyPropertyChanged(nameof(LogWindowHeight)); } }
        }

        public double LogWindowWidth
        {
            get { return logWindowWidth; }
            set { if (logWindowWidth != value) { logWindowWidth = value; NotifyPropertyChanged(nameof(LogWindowWidth)); } }
        }

        public static ApplicationSettings CreateInstance(PersistenceManager persistenceManager)
        {
            var instance = Load(persistenceManager) ?? new ApplicationSettings();
            instance.PropertyChanged += (s, e) => instance.Save(persistenceManager);
            return instance;
        }

        private static ApplicationSettings? Load(PersistenceManager persistenceManager)
        {
            var path = Path.Combine(ApplicationSettingsSaveDirectory, ApplicationSettingsSaveFile);
            var text = persistenceManager.LoadFile(path);
            if (text == null) return null;
            return JsonSerializer.Deserialize<ApplicationSettings>(text);
        }

        private void Save(PersistenceManager persistenceManager)
        {
            var path = Path.Combine(ApplicationSettingsSaveDirectory, ApplicationSettingsSaveFile);
            var serialized = JsonSerializer.Serialize(this);
            persistenceManager.SaveFile(path, serialized);
        }
    }
}
