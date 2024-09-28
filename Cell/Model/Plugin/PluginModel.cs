using Cell.Common;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Cell.Model.Plugin
{
    [JsonDerivedType(typeof(PluginModel), typeDiscriminator: "base")]
    [JsonDerivedType(typeof(TodoItem), typeDiscriminator: "todoItem")]
    [JsonDerivedType(typeof(TransactionItem), typeDiscriminator: "transactionItem")]
    [JsonDerivedType(typeof(BudgetCategoryItem), typeDiscriminator: "budgetCategoryItem")]
    [JsonDerivedType(typeof(FoodItem), typeDiscriminator: "foodItem")]
    public class PluginModel : INotifyPropertyChanged, ICloneable
    {
        private static Dictionary<string, Type> _cachedTypes = [];
        private static List<string>? _cachedDataTypeNames;

        private string _id = Utilities.GenerateUnqiueId(12);
        public event PropertyChangedEventHandler? PropertyChanged;

        public string ID
        {
            get => _id;
            set { if (value != _id) { _id = value; OnPropertyChanged(nameof(ID)); } }
        }

        public static IEnumerable<string> GetPluginDataTypeNames()
        {
            return _cachedDataTypeNames ??= Utilities
                .GetTypesInNamespace(Assembly.GetExecutingAssembly(), "Cell.Model.Plugin")
                .Where(x => x.BaseType == typeof(PluginModel))
                .Select(x => x.Name)
                .ToList();
        }

        public virtual object Clone()
        {
            return new PluginModel();
        }

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static Type GetTypeFromString(string typeName)
        {
            if (_cachedTypes.TryGetValue(typeName, out Type? value)) return value;
            var type = Assembly.GetExecutingAssembly().GetType($"Cell.Model.Plugin.{typeName}") ?? throw new CellError($"Unable to find type for the name {typeName}");
            _cachedTypes.Add(typeName, type);
            return type;
        }

        /// <summary>
        /// Returns the ID of this object.
        /// </summary>
        /// <returns>The object unique ID.</returns>
        override public string ToString() => ID;
    }
}
