
namespace Cell.Plugin
{
    public class EditContext(string propertyName, object newValue, object oldValue)
    {
        public string PropertyName { get; private set; } = propertyName;

        public object NewValue { get; private set; } = newValue;

        public object OldValue { get; private set; } = oldValue;

        public DateTime EditDate { get; set; } = DateTime.Now;
    }
}
