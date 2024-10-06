namespace Cell.Data
{
    /// <summary>
    /// Inserts an item into a list in a sorted order.
    /// </summary>
    /// <typeparam name="T">The type of item to sort/insert.</typeparam>
    /// <param name="getSortValue">The sort function.</param>
    public class SortedListInserter<T>(Func<int, int?> getSortValue)
    {
        private readonly Func<int, int?> _getSortValue = getSortValue ?? throw new ArgumentNullException(nameof(getSortValue));
        /// <summary>
        /// Inserts the item into the list in a sorted order, using binary search and the provided sort function.
        /// </summary>
        /// <param name="list">The list to insert into.</param>
        /// <param name="newItem">The item to insert.</param>
        /// <param name="newItemCompareValue">The comparison result of the item.</param>
        public void InsertSorted(List<T> list, T newItem, int newItemCompareValue)
        {
            // TODO: find newItemCompareValue from inside this functiion?
            int index = BinarySearchIndex(list, newItemCompareValue);
            list.Insert(index, newItem);
        }

        private int BinarySearchIndex(List<T> list, int newItemCompareValue)
        {
            int low = 0;
            int high = list.Count - 1;

            IComparable newItemValue = newItemCompareValue;

            while (low <= high)
            {
                int mid = low + (high - low) / 2;
                var midValue = _getSortValue(mid);
                midValue ??= 0;

                int comparison = newItemValue.CompareTo(midValue);

                if (comparison == 0) return mid;
                else if (comparison < 0) high = mid - 1;
                else low = mid + 1;
            }

            return low;
        }
    }
}
