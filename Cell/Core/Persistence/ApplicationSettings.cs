using Cell.Common;
using Cell.Model;
using System.IO;
using System.Text.Json;

namespace Cell.Persistence
{
    public class ApplicationSettings : PropertyChangedBase
    {
        private const string ApplicationSettingsSaveDirectory = "Application";
        private const string ApplicationSettingsSaveFile = "Settings.json";
        private CellModel _defaultCellStyleCellModel = new();
        private CellModel _defaultSpecialCellStyleCellModel = new();
        private bool highlightPopulateCellDependencies = true;
        private bool highlightPopulateCollectionDependencies = true;
        private bool highlightTriggerCellDependencies = true;
        private bool highlightTriggerCollectionDependencies = true;
        private string lastLoadedSheet = "Default";
        public ApplicationSettings()
        {
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

        public static ApplicationSettings CreateInstance(PersistedDirectory persistenceManager)
        {
            var instance = Load(persistenceManager) ?? new ApplicationSettings();
            instance.PropertyChanged += (s, e) => instance.Save(persistenceManager);
            return instance;
        }

        private static ApplicationSettings? Load(PersistedDirectory persistenceManager)
        {
            var path = Path.Combine(ApplicationSettingsSaveDirectory, ApplicationSettingsSaveFile);
            var text = persistenceManager.LoadFile(path);
            if (text == null) return null;
            return JsonSerializer.Deserialize<ApplicationSettings>(text);
        }

        private void Save(PersistedDirectory persistenceManager)
        {
            var path = Path.Combine(ApplicationSettingsSaveDirectory, ApplicationSettingsSaveFile);
            var serialized = JsonSerializer.Serialize(this);
            persistenceManager.SaveFile(path, serialized);
        }
    }
}
