using Cell.Common;
using Cell.Model;
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
        private CellModel _defaultCellStyleCellModel = new();
        private CellModel _defaultSpecialCellStyleCellModel = new();

        public ApplicationSettings()
        {
        }

        public Dock CodeEditorDockPosition
        {
            get => codeEditorDockPosition;
            set { if (codeEditorDockPosition != value) { codeEditorDockPosition = value; NotifyPropertyChanged(nameof(CodeEditorDockPosition)); } }
        }

        public double CodeEditorHeight
        {
            get => codeEditorHeight;
            set { if (codeEditorHeight != value) { codeEditorHeight = value; NotifyPropertyChanged(nameof(CodeEditorHeight)); } }
        }

        public double CodeEditorWidth
        {
            get => codeEditorWidth; 
            set { if (codeEditorWidth != value) { codeEditorWidth = value; NotifyPropertyChanged(nameof(CodeEditorWidth)); } }
        }

        public double FunctionManagerWindowHeight
        {
            get => functionManagerWindowHeight;
            set { if (functionManagerWindowHeight != value) { functionManagerWindowHeight = value; NotifyPropertyChanged(nameof(FunctionManagerWindowHeight)); } }
        }

        public double FunctionManagerWindowWidth
        {
            get => functionManagerWindowWidth;      
            set { if (functionManagerWindowWidth != value) { functionManagerWindowWidth = value; NotifyPropertyChanged(nameof(FunctionManagerWindowWidth)); } }
        }

        public bool HighlightPopulateCellDependencies
        {
            get => highlightPopulateCellDependencies;
            set { if (highlightPopulateCellDependencies != value) { highlightPopulateCellDependencies = value; NotifyPropertyChanged(nameof(HighlightPopulateCellDependencies)); } }
        }

        public bool HighlightPopulateCollectionDependencies
        {
            get => highlightPopulateCollectionDependencies;
            set { if (highlightPopulateCollectionDependencies != value) { highlightPopulateCollectionDependencies = value; NotifyPropertyChanged(nameof(HighlightPopulateCollectionDependencies)); } }
        }

        public bool HighlightTriggerCellDependencies
        {
            get => highlightTriggerCellDependencies;
            set { if (highlightTriggerCellDependencies != value) { highlightTriggerCellDependencies = value; NotifyPropertyChanged(nameof(HighlightTriggerCellDependencies)); } }
        }

        public bool HighlightTriggerCollectionDependencies
        {
            get => highlightTriggerCollectionDependencies;  
            set { if (highlightTriggerCollectionDependencies != value) { highlightTriggerCollectionDependencies = value; NotifyPropertyChanged(nameof(HighlightTriggerCollectionDependencies)); } }
        }

        public string LastLoadedSheet
        {
            get => lastLoadedSheet;
            set { if (lastLoadedSheet != value) { lastLoadedSheet = value; NotifyPropertyChanged(nameof(LastLoadedSheet)); } }
        }

        public double LogWindowHeight
        {
            get => logWindowHeight;
            set { if (logWindowHeight != value) { logWindowHeight = value; NotifyPropertyChanged(nameof(LogWindowHeight)); } }
        }

        public double LogWindowWidth
        {
            get => logWindowWidth;
            set { if (logWindowWidth != value) { logWindowWidth = value; NotifyPropertyChanged(nameof(LogWindowWidth)); } }
        }

        public CellModel DefaultCellStyleCellModel
        {
            get => _defaultCellStyleCellModel;
            set
            {
                if (_defaultCellStyleCellModel == value) return;
                _defaultCellStyleCellModel = value;
                _defaultCellStyleCellModel.PropertyChanged += (x, e) => NotifyPropertyChanged(nameof(DefaultCellStyleCellModel));
                NotifyPropertyChanged(nameof(DefaultCellStyleCellModel));
            }
        }

        public CellModel DefaultSpecialCellStyleCellModel
        {
            get => _defaultSpecialCellStyleCellModel;
            set
            {
                if (_defaultSpecialCellStyleCellModel == value) return;
                _defaultSpecialCellStyleCellModel = value;
                _defaultSpecialCellStyleCellModel.PropertyChanged += (x, e) => NotifyPropertyChanged(nameof(DefaultSpecialCellStyleCellModel));
                NotifyPropertyChanged(nameof(DefaultSpecialCellStyleCellModel));
            }
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
