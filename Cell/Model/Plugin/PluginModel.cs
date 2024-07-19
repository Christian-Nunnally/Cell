
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cell.Model.Plugin
{
    [JsonDerivedType(typeof(PluginModel), typeDiscriminator: "base")]
    [JsonDerivedType(typeof(TodoItem), typeDiscriminator: "todoItem")]
    public class PluginModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;

        override public string ToString()
        {
            return ID;
        }

        public string ID 
        { 
            get => _id; 
            set { if (value != _id) { _id = value; OnPropertyChanged(nameof(ID)); } }
        }
        private string _id = Utilities.GenerateUnqiueId(12);

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
