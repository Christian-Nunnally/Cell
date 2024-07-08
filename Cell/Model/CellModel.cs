using System.Text.Json;

namespace Cell.Model
{
    public class CellModel
    {
        public double X { get; set; }
        
        public double Y { get; set; }
        
        public double Width { get; set; }
        
        public double Height { get; set; }

        public string CellType { get; set; } = string.Empty;

        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string SheetName { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public string BackgroundColorHex { get; set; } = "#1e1e1e";

        public Dictionary<string, bool> BooleanProperties { get; set; } = [];

        public Dictionary<string, double> DoubleProperties { get; set; } = [];

        public Dictionary<string, int> IntegerProperties { get; set; } = [];

        public Dictionary<string, string> StringProperties { get; set; } = [];

        public string GetTextFunctionName { get; set; } = string.Empty;

        public string OnEditFunctionName { get; set; } = string.Empty;

        public static string SerializeModel(CellModel model)
        {
            return JsonSerializer.Serialize(model);
        }

        public static CellModel DeserializeModel(string serializedModel)
        {
            return JsonSerializer.Deserialize<CellModel>(serializedModel) ?? throw new InvalidOperationException("DeserializeModel failed.");
        }
    }
}
