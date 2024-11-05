using Cell.Core.Common;
using System.Text.Json.Serialization;

namespace Cell.Model
{
    /// <summary>
    /// Provides a way to store custom properties on a cell model.
    /// </summary>
    public class CellModelCustomPropertiesModel : PropertyChangedBase
    {
        private CellModel? cellModel;

        /// <summary>
        /// A dictionary of string properties that can be set on the cell.
        /// </summary>
        public Dictionary<string, string> CustomProperties { get; set; } = [];

        /// <summary>
        /// Gets or sets a custom string property stored on the cell. Will not throw exceptions.
        /// </summary>
        /// <param name="key">The name of the value to get or set.</param>
        /// <returns>The value of the property with the given key name.</returns>
        public string this[string key]
        {
            get => GetCustomProperty(key);
            set => SetCustomProperty(key, value);
        }

        /// <summary>
        /// The <see cref="CellModel"/> that this location is associated with.
        /// </summary>
        [JsonIgnore]
        public CellModel? CellModel
        {
            get => cellModel; internal set
            {
                cellModel = value;
            }
        }

        /// <summary>
        /// Gets a custom string property stored on the cell.
        /// </summary>
        /// <param name="key">The name of the custom property to get.</param>
        /// <returns></returns>
        private string GetCustomProperty(string key) => CustomProperties.TryGetValue(key, out var value) ? value : string.Empty;

        /// <summary>
        /// Sets the value of a custom string property on the cell.
        /// </summary>
        /// <param name="key">The custom name to give the property</param>
        /// <param name="value">The value to set the property to</param>
        private void SetCustomProperty(string key, string value)
        {
            if (CustomProperties.TryGetValue(key, out var currentValue))
            {
                if (currentValue == value) return;
                CustomProperties[key] = value;
            }
            else CustomProperties.Add(key, value);
            NotifyPropertyChanged(nameof(CustomProperties));
            NotifyPropertyChanged(key);
        }

        /// <summary>
        /// Sets the value of a custom boolean property on the cell.
        /// </summary>
        /// <param name="key">The name of the custom property to set.</param>
        /// <param name="value">The value to set it to.</param>
        public void SetBooleanProperty(string key, bool value)
        {
            SetCustomProperty(key, value.ToString());
        }

        /// <summary>
        /// Sets the value of a custom numeric property on the cell.
        /// </summary>
        /// <param name="key">The custom name to give the property</param>
        /// <param name="value">The value to set the property to.</param>
        public void SetNumericProperty(string key, double value)
        {
            SetCustomProperty(key, value.ToString());
        }

        /// <summary>
        /// Gets the value of a custom boolean property on the cell. and returns the default value if the property has not been set.
        /// </summary>
        /// <param name="key">The user provided name of the property</param>
        /// <param name="defaultValue">The value to return if this property has never been set on this cell.</param>
        /// <returns>The value of the custom property with the given name if it is a boolean, otherwise the default value.</returns>
        public bool GetBooleanProperty(string key, bool defaultValue = false)
        {
            var doesValueExist = CustomProperties.TryGetValue(key, out var value);
            return (doesValueExist && bool.TryParse(value, out var booleanResult))
                ? booleanResult
                : defaultValue;
        }

        /// <summary>
        /// Gets the value of a custom numeric property on the cell. Returns a default value if the property has not been set.
        /// </summary>
        /// <param name="key">The name of the property to get the value of.</param>
        /// <param name="defaultValue">The value to return if the property has not been set.</param>
        /// <returns>The value of the custom property with the given name if it is a number, otherwise the default value.</returns>
        public double GetNumericProperty(string key, double defaultValue = 0)
        {
            var doesValueExist = CustomProperties.TryGetValue(key, out var value);
            return doesValueExist && double.TryParse(value, out var doubleResult)
                ? doubleResult
                : defaultValue;
        }
    }
}
