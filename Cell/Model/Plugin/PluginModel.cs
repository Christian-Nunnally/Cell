using Cell.Common;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Cell.Model.Plugin
{
    /// <summary>
    /// Base class for plugin data items. All plugin data items should inherit from this class and list themselves as a JsonDerivedType ike so: [JsonDerivedType(typeof(YourPluginDataTypeName), typeDiscriminator: "yourPluginDataTypeName")]
    /// </summary>
    [JsonDerivedType(typeof(PluginModel), typeDiscriminator: "base")]
    [JsonDerivedType(typeof(TodoItem), typeDiscriminator: "todoItem")]
    [JsonDerivedType(typeof(TransactionItem), typeDiscriminator: "transactionItem")]
    [JsonDerivedType(typeof(BudgetCategoryItem), typeDiscriminator: "budgetCategoryItem")]
    [JsonDerivedType(typeof(FoodItem), typeDiscriminator: "foodItem")]
    public class PluginModel : PropertyChangedBase, ICloneable
    {
        private const string PluginModelTypesNamespace = "Cell.Model.Plugin";
        private static readonly Dictionary<string, Type> _cachedTypes = [];
        private static List<string>? _cachedDataTypeNames;
        private string _id = Utilities.GenerateUnqiueId(12);
        /// <summary>
        /// The unique ID of this plugin data item. Should be unique across all plugin data items of any type.
        /// </summary>
        public string ID
        {
            get => _id;
            set
            {
                if (value == _id) return;
                _id = value;
                NotifyPropertyChanged(nameof(ID));
            }
        }

        /// <summary>
        /// Gets the list of the names of all plugin data types by searching the Cell.Model.Plugin namespace for types that implement PluginModel.
        /// </summary>
        /// <returns>A list of the names of all the data types.</returns>
        public static IEnumerable<string> GetPluginDataTypeNames()
        {
            return _cachedDataTypeNames ??= Utilities
                .GetTypesInNamespace(Assembly.GetExecutingAssembly(), PluginModelTypesNamespace)
                .Where(x => x.BaseType == typeof(PluginModel))
                .Select(x => x.Name)
                .ToList();
        }

        /// <summary>
        /// Gets the reflection Type from a type name. Only works for names returned by GetPluginDataTypeNames.
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        /// <exception cref="CellError"></exception>
        public static Type GetTypeFromString(string typeName)
        {
            if (_cachedTypes.TryGetValue(typeName, out Type? value)) return value;
            var type = Assembly.GetExecutingAssembly().GetType($"{PluginModelTypesNamespace}.{typeName}") ?? throw new CellError($"Unable to find type for the name {typeName}");
            _cachedTypes.Add(typeName, type);
            return type;
        }

        /// <summary>
        /// Copies the public properties to a new plugin model with a new ID and returns the new model.
        /// </summary>
        /// <returns>A new model with identical properties of this model.</returns>
        public virtual object Clone() => new PluginModel();

        /// <summary>
        /// Returns the ID of this object.
        /// </summary>
        /// <returns>The object unique ID.</returns>
        override public string ToString() => ID;
    }
}
