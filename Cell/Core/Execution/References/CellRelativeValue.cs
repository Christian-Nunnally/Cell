namespace Cell.Core.Execution.References
{
    /// <summary>
    /// A value that can be absolute or relative to a cell.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public class CellRelativeValue<T> where T : notnull
    {
        /// <summary>
        /// The value represented by this <see cref="CellRelativeValue{T}"/>.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Whether the value is relative to a cell.
        /// </summary>
        public bool IsRelative { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="CellRelativeValue{T}"/> class.
        /// </summary>
        /// <param name="initialValue">The initial value.</param>
        public CellRelativeValue(T initialValue)
        {
            Value = initialValue;
        }
    }
}
