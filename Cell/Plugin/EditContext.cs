
namespace Cell.Plugin
{
    public class EditContext(string propertyName, string newValue, string oldValue)
    {
        public string PropertyName { get; private set; } = propertyName;

        public string NewValue { get; private set; } = newValue;

        public string OldValue { get; private set; } = oldValue;

        public DateTime EditDate { get; set; } = DateTime.Now;
    }
}
