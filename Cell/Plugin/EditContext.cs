
namespace Cell.Plugin
{
    public class EditContext
    {
        public string Reason { get; private set; }

        public string PropertyName { get; private set; }

        public object NewValue { get; private set; }

        public object OldValue { get; private set; }

        public DateTime EditDate { get; set; } = DateTime.Now;

        public EditContext(string propertyName, object newValue, object oldValue)
        {
            Reason = "PropertyChanged";
            PropertyName = propertyName;
            NewValue = newValue;
            OldValue = oldValue;
        }

        public EditContext(string reason)
        {
            Reason = reason;
            PropertyName = "";
            NewValue = "";
            OldValue = "";
        }
    }
}
