namespace Cell.Execution
{
    /// <summary>
    /// TODO: maybe get rid of this in favor of just having more handler functions?
    /// 
    /// Provides information about an edit operation that happened in a cell.
    /// </summary>
    public class EditContext
    {
        /// <summary>
        /// Creates a new instance of the <see cref="EditContext"/> class with a property name and new and old values.
        /// </summary>
        /// <param name="propertyName">The name of the property that was edited.</param>
        /// <param name="newValue">The new value of the property that was edited.</param>
        /// <param name="oldValue">The old value of the property that was edited.</param>
        public EditContext(string propertyName, object newValue, object oldValue)
        {
            Reason = "PropertyChanged";
            PropertyName = propertyName;
            NewValue = newValue;
            OldValue = oldValue;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="EditContext"/> class with a custom reason.
        /// </summary>
        /// <param name="reason">The reason for the edit.</param>
        public EditContext(string reason)
        {
            Reason = reason;
        }

        /// <summary>
        /// Gets the date and time that the edit happened.
        /// </summary>
        public DateTime EditDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets the new value of the cell after the edit.
        /// </summary>
        public object NewValue { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the old value of the cell before the edit.
        /// </summary>
        public object OldValue { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the name of the property that was changed.
        /// </summary>
        public string PropertyName { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the reason that the edit happened.
        /// </summary>
        public string Reason { get; private set; } = string.Empty;
    }
}
