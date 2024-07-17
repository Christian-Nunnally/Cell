
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cell.Model.Plugin
{
    [JsonDerivedType(typeof(PluginModel), typeDiscriminator: "base")]
    [JsonDerivedType(typeof(TodoItem), typeDiscriminator: "todoItem")]
    public class PluginModel
    {
        override public string ToString()
        {
            return JsonSerializer.Serialize(this);
        }

        public static T FromString<T>(string json) where T : new()
        {
            return JsonSerializer.Deserialize<T>(json) ?? new T();
        }

        public string ID { get; set; } = Utilities.GenerateUnqiueId(12);
    }
}
