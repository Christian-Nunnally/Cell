using Cell.Model.Plugin;

namespace CellTest.Model
{
    internal class TestPluginModel : PluginModel
    {
        private string _property = string.Empty;
        public string Property
        {
            get => _property;
            set
            {
                if (value == _property) return;
                _property = value;
                OnPropertyChanged(nameof(Property));
            }
        }
    }
}
