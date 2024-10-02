using Cell.Common;
using Cell.Model;
using System.IO;
using System.Text.Json;

namespace Cell.Persistence
{
    /// <summary>
    /// Persists application settings.
    /// </summary>
    public class ApplicationSettings : PropertyChangedBase
    {
        private const string ApplicationSettingsSaveDirectory = "Application";
        private const string ApplicationSettingsSaveFile = "Settings.json";
        private CellModel _defaultCellStyleCellModel = new();
        private CellModel _defaultSpecialCellStyleCellModel = new();
        private string lastLoadedSheet = "Default";
        /// <summary>
        /// Creates a new instance of the <see cref="ApplicationSettings"/> class.
        /// </summary>
        public ApplicationSettings()
        {
        }

        /// <summary>
        /// Gets or sets the default style that newly created cells will use.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the default style that newly created column and row cells will use.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the name of the sheet that was open when the user last used the project.
        /// </summary>
        public string LastLoadedSheet
        {
            get => lastLoadedSheet;
            set { if (lastLoadedSheet != value) { lastLoadedSheet = value; NotifyPropertyChanged(nameof(LastLoadedSheet)); } }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ApplicationSettings"/> class that will save its state to the specified <see cref="PersistedDirectory"/> when it changes.
        /// </summary>
        /// <param name="persistedDirectory">The directory to persist the application settings to.</param>
        /// <returns>A new application settings object that will auto save.</returns>
        public static ApplicationSettings CreateInstance(PersistedDirectory persistedDirectory)
        {
            var instance = Load(persistedDirectory) ?? new ApplicationSettings();
            instance.PropertyChanged += (s, e) => instance.Save(persistedDirectory);
            return instance;
        }

        private static ApplicationSettings? Load(PersistedDirectory persistedDirectory)
        {
            var path = Path.Combine(ApplicationSettingsSaveDirectory, ApplicationSettingsSaveFile);
            var text = persistedDirectory.LoadFile(path);
            if (text == null) return null;
            return JsonSerializer.Deserialize<ApplicationSettings>(text);
        }

        private void Save(PersistedDirectory persistedDirectory)
        {
            var path = Path.Combine(ApplicationSettingsSaveDirectory, ApplicationSettingsSaveFile);
            var serialized = JsonSerializer.Serialize(this);
            persistedDirectory.SaveFile(path, serialized);
        }
    }
}
