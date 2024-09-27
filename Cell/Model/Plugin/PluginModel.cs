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

        override public string ToString() => ID;
    }
}
